# SovereignID — Master Design Document

> **Canonical reference for all phases.**
> Version: 1.0 · Date: April 2026
> Owner: SovereignID project · Branch: `feature/sonar-coverage-phase-2`
>
> This document is the **single source of truth** for architecture, design
> decisions, and roadmap. Every spec under `openspec/changes/*` must align
> with it. Updates to this document require explicit versioning and changelog.

---

## Table of Contents

1. [Purpose & Scope](#1-purpose--scope)
2. [System Vision](#2-system-vision)
3. [Architectural Principles](#3-architectural-principles)
4. [System Architecture](#4-system-architecture)
5. [Bounded Contexts](#5-bounded-contexts)
6. [Cross-Cutting Concerns](#6-cross-cutting-concerns)
7. [Decentralized Storage Strategy](#7-decentralized-storage-strategy)
8. [Security Model](#8-security-model)
9. [Quality Gates](#9-quality-gates)
10. [Phase Roadmap](#10-phase-roadmap)
11. [Spec Generation Plan](#11-spec-generation-plan)
12. [Glossary](#12-glossary)
13. [Decision Log (ADRs)](#13-decision-log-adrs)
14. [Changelog](#14-changelog)

---

## 1. Purpose & Scope

### 1.1 Purpose of this document

This is the **master design document** for SovereignID. It exists to:

- Provide a single reference for architecture decisions across all phases
- Prevent inconsistencies as new bounded contexts (BCs) are added
- Give the AI agent (Cursor + OpenSpec) a stable context to reason from
- Enable spec generation that respects established patterns
- Document the "why" behind every major decision (ADRs)

### 1.2 What this document is NOT

- It is **not** an implementation spec — those live in `openspec/changes/*`
- It is **not** a tutorial — assumes familiarity with .NET, DDD, blockchain
- It is **not** immutable — changes are tracked in the Changelog (§14)

### 1.3 Audience

- Project author (developer)
- Thesis advisor (academic reviewer)
- AI agent (Cursor) when generating new code
- Future contributors

---

## 2. System Vision

### 2.1 Mission

SovereignID enables **self-sovereign digital identity** for academic credentials.
Students hold their own credentials (degrees, transcripts, certifications, diplomas),
issuers sign them cryptographically, and verifiers validate them without contacting
the issuer.

### 2.2 Core value proposition

| Stakeholder | Pain point today | SovereignID solution |
|-------------|------------------|---------------------|
| Student | Loses physical degree, slow re-issuance | Permanent digital credential in own wallet |
| Employer | Cannot verify credentials in real time | Verify in seconds via blockchain proof |
| Institution | Manual verification requests, fraud risk | Issue once, verify automatically forever |
| Society | $48M+/year wasted on KYC redundancy | Portable, reusable identity layer |

### 2.3 Non-goals (explicitly out of scope for v1)

- Production deployment to Ethereum Mainnet
- Mobile native apps (iOS/Android)
- Zero-knowledge proofs (zk-SNARKs/STARKs)
- Account abstraction (ERC-4337)
- Integration with existing institutional databases (SAP, PeopleSoft, etc.)
- Multi-chain support beyond Sepolia

### 2.4 Use cases (priority order)

1. **MVP (thesis defense)**: Issue + verify academic credentials in Sepolia
2. **Post-thesis**: Production deploy on a Layer 2 (Polygon, Base) with a real institution pilot
3. **Long-term**: Open-source SDK adopted by other educational institutions

---

## 3. Architectural Principles

These principles are **non-negotiable** and apply to all current and future code.

### 3.1 Onion Architecture

Every bounded context follows the same layered structure:

```
Domain  ←  no dependencies on anything except SharedKernel.Domain
   ↑
Application  ←  depends only on Domain
   ↑
Infrastructure  ←  implements Domain ports, may use external libs (Nethereum, EF, IPFS)
   ↑
Api / Host  ←  composition root; wires everything up
```

**Enforcement**: validated by `ArchitectureRulesTests` in CI.
**Rationale**: keeps domain logic pure, testable, and replaceable.

### 3.2 Bounded Context Isolation

Each BC (`bc-auth`, `bc-issuer`, `bc-verifier`, `bc-storage`) is independent:

- BCs **never** reference each other directly
- Communication between BCs (when needed) happens via:
  - Shared DTOs in `SharedKernel.Application`
  - HTTP/messaging at the boundary
- Each BC owns its own domain model, even if concepts overlap

**Enforcement**: `BCs_Cannot_Reference_Each_Other` test in CI.

### 3.3 Hexagonal Ports & Adapters

- Every external dependency is defined as an interface (Port) in Domain
- Implementations (Adapters) live in Infrastructure
- Application layer depends only on Ports

**Examples already in code**:
- `INonceGenerator` (port) → `SecureRandomNonceGenerator` (adapter)
- `ISiweMessageParser` (port) → `ManualSiweMessageParser` (adapter)
- `IJwtTokenIssuer` (port) → `JwtBearerTokenIssuer` (adapter)

### 3.4 CQRS without MediatR

Commands and Queries are explicit interfaces in `SharedKernel.Application`:

```csharp
public interface ICommandHandler<in TCommand, TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
}

public interface IQueryHandler<in TQuery, TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
}
```

**Rationale**: explicit dispatch, no magic, no MediatR dependency.
**Enforcement**: `No_Project_References_MediatR_Package` test in CI.

### 3.5 Result over Exceptions

Business errors return `Result<TValue, TError>`. Exceptions are reserved for:

- Truly exceptional conditions (DB down, RPC unreachable)
- Programming errors (invalid arguments at construction time)

Domain validation errors flow through `Result.Failure(AuthError)`.

### 3.6 Async Everywhere

- Every I/O method returns `Task<T>` or `ValueTask<T>`
- Never `.Result` or `.Wait()` — caught by code review
- All ports take `CancellationToken` as last parameter

### 3.7 No Secrets in Source

- Private keys, signing keys, RPC URLs: environment variables only
- `appsettings.Development.json` is gitignored
- `appsettings.Development.example.json` shows non-secret defaults

### 3.8 Immutability First

- Records for value objects (`Nonce`, `EthereumAddress`, `ChainId`)
- `sealed` on classes that are not designed for inheritance
- Domain entities expose `private set;` for state changes

---

## 4. System Architecture

### 4.1 High-level diagram

```
┌────────────────────────────────────────────────────────────────┐
│                           CLIENT LAYER                         │
│   ┌──────────────┐   ┌──────────────┐   ┌──────────────────┐   │
│   │  MetaMask    │   │  Holder      │   │  Verifier UI     │   │
│   │  (wallet)    │   │  Wallet UI   │   │  (any web app)   │   │
│   └──────┬───────┘   └──────┬───────┘   └────────┬─────────┘   │
└──────────┼──────────────────┼────────────────────┼─────────────┘
           │                  │                    │
           │ SIWE             │ Holds VCs          │ Verifies VPs
           ▼                  ▼                    ▼
┌────────────────────────────────────────────────────────────────┐
│                           API LAYER (.NET 9)                   │
│   ┌──────────────┐   ┌──────────────┐   ┌──────────────────┐   │
│   │  Auth API    │   │  Issuer API  │   │  Verifier API    │   │
│   │  bc-auth     │   │  bc-issuer   │   │  bc-verifier     │   │
│   └──────┬───────┘   └──────┬───────┘   └────────┬─────────┘   │
└──────────┼──────────────────┼────────────────────┼─────────────┘
           │                  │                    │
           │ JWT              │ Sign + Store       │ Read + Verify
           ▼                  ▼                    ▼
┌────────────────────────────────────────────────────────────────┐
│                  DECENTRALIZED INFRASTRUCTURE                  │
│   ┌────────────────────────┐   ┌─────────────────────────┐     │
│   │  IPFS / web3.storage   │   │  Ethereum Sepolia       │     │
│   │  (VC documents)        │   │  - CredentialRegistry   │     │
│   │  Returns CID           │   │  - RevocationRegistry   │     │
│   │                        │   │  - Notary (legacy)      │     │
│   └────────────────────────┘   └─────────────────────────┘     │
└────────────────────────────────────────────────────────────────┘
```

### 4.2 Data flow: emit a credential

```
1. Issuer authenticates (SIWE → JWT)
2. Issuer calls POST /credentials/issue with subject DID + claims
3. Issuer API (bc-issuer):
   a. Builds VC JSON-LD per W3C VC Data Model 1.1
   b. Signs VC with institution's private key (EIP-712 over JSON-LD)
   c. Uploads VC to IPFS via web3.storage → returns CID
   d. Computes SHA-256 of VC
   e. Calls CredentialRegistry.register(credentialId, subjectDid, cid, hash)
   f. Returns to issuer: VC + CID + transaction hash
4. Issuer delivers VC to student (out of band: email, QR, etc.)
```

### 4.3 Data flow: verify a credential

```
1. Verifier receives VP from holder (containing VC reference or full VC)
2. Verifier API (bc-verifier):
   a. Extracts credentialId from VP
   b. Calls CredentialRegistry.get(credentialId) → returns issuerDid, cid, hash
   c. Downloads VC from IPFS using CID
   d. Verifies SHA-256(downloadedVC) == hash on-chain
   e. Verifies issuer signature using issuer's public key (resolved from DID)
   f. Checks RevocationRegistry.isRevoked(credentialId) == false
   g. Verifies VP signature (proves holder controls the subject DID)
   h. Returns: { valid: true, claims: {...} }
```

---

## 5. Bounded Contexts

### 5.1 bc-auth (DONE — Phase 2)

**Purpose**: SIWE authentication. Issues JWT sessions.

**Domain**:
- `AuthChallenge`, `Nonce`, `ChainId`, `SiweMessage`, `EthereumAddress`
- Ports: `INonceGenerator`, `ISiweMessageParser`, `ISiweSignatureVerifier`,
  `IJwtTokenIssuer`, `IAuthChallengeRepository`

**Application**: `GenerateNonceQueryHandler`, `VerifySiweCommandHandler`

**Infrastructure**: `SecureRandomNonceGenerator`, `ManualSiweMessageParser`,
`NethereumSiweSignatureVerifier`, `JwtBearerTokenIssuer`,
`InMemoryAuthChallengeRepository`, `AuthChallengeEvictionHostedService`

**API endpoints**: `GET /auth/nonce`, `POST /auth/verify`

### 5.2 bc-issuer (NEW — Phase 3)

**Purpose**: Issue Verifiable Credentials. Sign with institution key. Store on IPFS + Ethereum.

**Domain (new)**:
- Entities: `Credential` (aggregate), `Issuer`
- Value objects: `CredentialId` (UUID), `DecentralizedIdentifier` (DID),
  `CredentialType` (enum: TituloGraduacion, CertificadoNotas, Certificacion, Diploma),
  `IpfsCid`, `CredentialClaims` (typed per credential type)
- Ports: `ICredentialSigner`, `IIpfsUploader`, `ICredentialRegistryWriter`,
  `IRevocationRegistryWriter`, `IIssuerKeyProvider`

**Application**: `IssueCredentialCommandHandler`, `RevokeCredentialCommandHandler`,
`GetCredentialQueryHandler`

**Infrastructure**:
- `Eip712CredentialSigner` (signs VC with EIP-712 typed data)
- `Web3StorageUploader` (uploads to IPFS via web3.storage HTTP API)
- `EthereumCredentialRegistryWriter` (calls smart contract)
- `EthereumRevocationRegistryWriter`
- `EnvironmentIssuerKeyProvider` (reads from `ISSUER_PRIVATE_KEY` env var)

**API endpoints**:
- `POST /credentials/issue` (requires JWT)
- `POST /credentials/revoke` (requires JWT)
- `GET /credentials/{id}` (public)

**Smart contracts (new)**:
- `CredentialRegistry.sol` — maps credentialId → { issuer, subject, cid, hash, timestamp }
- `RevocationRegistry.sol` — maps credentialId → revoked (bool)

### 5.3 bc-verifier (NEW — Phase 3)

**Purpose**: Verify Verifiable Presentations. Read-only context.

**Domain (new)**:
- Value objects: `VerifiablePresentation`, `VerificationResult`
- Ports: `ICredentialRegistryReader`, `IRevocationRegistryReader`,
  `IIpfsDownloader`, `ICredentialSignatureVerifier`

**Application**: `VerifyPresentationCommandHandler`

**Infrastructure**:
- `EthereumCredentialRegistryReader`
- `EthereumRevocationRegistryReader`
- `Web3StorageDownloader`
- `Eip712CredentialSignatureVerifier`

**API endpoints**:
- `POST /credentials/verify` (public, no auth required — verification is open)

### 5.4 bc-storage (NEW — Phase 3, shared infrastructure)

**Purpose**: Abstract IPFS/Filecoin operations. Used by both `bc-issuer` and `bc-verifier`.

**Decision**: implemented as a shared infrastructure module, not a full BC.
Lives in `src/shared/SovereignID.Storage.Infrastructure/`.

**Why not a full BC?**
- No business logic of its own
- Pure technical adapter
- Both `bc-issuer` and `bc-verifier` need it identically

### 5.5 Cross-BC communication rules

| From | To | Allowed? | How |
|------|-----|----------|-----|
| bc-auth | bc-issuer | NO direct ref | Issuer API verifies JWT via shared JWT validation |
| bc-issuer | bc-verifier | NO | None — they only share Ethereum state |
| bc-verifier | bc-issuer | NO | None — verifier reads on-chain data |
| Any BC | bc-storage | YES | Via Storage abstractions in SharedKernel |

---

## 6. Cross-Cutting Concerns

### 6.1 Time

- All timestamps: `DateTimeOffset` in UTC
- Never use `DateTime.Now` or `DateTime.UtcNow` directly in code
- Always inject `IClock` (already in SharedKernel)
- Tests use `TestClock` that allows `Advance(TimeSpan)`

### 6.2 IDs

- All public IDs: UUID v4
- Generated via `IGuidGenerator` (already in SharedKernel)
- Never auto-increment integers

### 6.3 Logging

- Use `ILogger<T>` from `Microsoft.Extensions.Logging`
- Never log: private keys, JWTs (full), nonces (until consumed)
- Log: addresses (truncated), credential IDs, error codes, timing

### 6.4 Configuration

- Bind via `IOptions<T>` pattern with `ValidateDataAnnotations()`
- Required secrets validated at startup (fail fast)
- Examples in `appsettings.Development.example.json`

### 6.5 Error handling

- Domain errors: `Result<T, AuthError>` (or equivalent per BC)
- Infrastructure errors: caught and wrapped at adapter boundary
- API: maps domain errors to HTTP status codes consistently:
  - `nonce_unknown`, `nonce_expired`, `nonce_consumed`, `signature_mismatch` → **401**
  - `unsupported_chain`, `siwe_parse_failed`, `invalid_credential_format` → **400**
  - `credential_not_found` → **404**
  - `credential_revoked` → **410 Gone**
  - Internal errors → **500** (logged, generic message returned)

---

## 7. Decentralized Storage Strategy

### 7.1 The principle

**Ethereum stores proofs, not data.**

```
┌─────────────────────────────────────────────────────────────┐
│  IPFS / Filecoin (via web3.storage)                         │
│  - Full VC JSON-LD documents                                │
│  - Free up to 5GB on free tier                              │
│  - Guaranteed persistence via Filecoin deals                │
└─────────────────────────────────────────────────────────────┘
                          │
                          │  CID (Content Identifier)
                          ▼
┌─────────────────────────────────────────────────────────────┐
│  Ethereum Sepolia                                           │
│  - SHA-256 of VC                                            │
│  - CID pointer to IPFS                                      │
│  - Issuer DID, Subject DID                                  │
│  - Issuance timestamp                                       │
│  - Revocation status                                        │
└─────────────────────────────────────────────────────────────┘
```

### 7.2 What goes where

| Data | Location | Why |
|------|----------|-----|
| VC JSON-LD (full document, with claims) | IPFS | Too large/expensive for chain |
| SHA-256 of VC | Ethereum | Integrity proof |
| IPFS CID | Ethereum | Pointer for retrieval |
| Issuer DID | Ethereum | Public, needed for verification |
| Subject DID | Ethereum | Public, links credential to holder |
| Revocation status | Ethereum | Must be on-chain (single source of truth) |
| Personal data (name, DOB) | **Inside VC on IPFS** | Privacy via selective disclosure later |
| Private keys | Local wallet (MetaMask) | Never leaves user's device |

### 7.3 Service choice: web3.storage

**Decision**: use `web3.storage` (built on Filecoin + IPFS) for v1.

**Rationale**:
- Free tier: 5GB (enough for thousands of VCs)
- HTTP API (no need to run an IPFS node)
- Content addressed: same VC always produces same CID
- Persistence guaranteed via Filecoin deals
- Used by major Web3 projects (proof of maturity)

**Alternatives evaluated**:
- Self-hosted IPFS node — too much ops overhead
- Arweave — adds another token to manage
- Ceramic — overkill for immutable VCs

### 7.4 Privacy model

**Important**: by default, IPFS content is public. Anyone with the CID can read the VC.

**Mitigations for v1**:
- VCs include only minimal claims (no full DOB, no addresses)
- Selective disclosure handled at presentation time (Holder presents only required claims)
- For sensitive credentials, encrypt VC before upload using subject's public key (Phase 4)

---

## 8. Security Model
> See openspec/specs/security-annex.md for the authoritative
> security architecture, threat model, and control matrix.
> This section provides only a high-level summary.

### 8.1 Trust boundaries

```
[Trusted]   Domain logic (pure, tested, reviewed)
[Trusted]   Smart contracts (audited, immutable once deployed)
[Trusted]   Cryptographic primitives (Nethereum, .NET CryptoAPI)

[Untrusted] User input (SIWE messages, VPs, request bodies)
[Untrusted] IPFS content (could be tampered → verify hash always)
[Untrusted] RPC providers (could censor → multiple providers in production)
```

### 8.2 Threat model summary

| Threat | Mitigation | Where implemented |
|--------|-----------|-------------------|
| Replay attack on SIWE | Single-use nonces with TTL | `AuthChallenge.Consume()` |
| Phishing (sign for wrong domain) | Validate `Chain ID` and `domain` | `ChainId.Create()` |
| Tampered SIWE message | EcRecover comparison case-insensitive | `VerifySiweCommandHandler` |
| Forged VC | EIP-712 signature verification | `Eip712CredentialSignatureVerifier` (Phase 3) |
| Tampered VC on IPFS | SHA-256 hash registered on-chain | `EthereumCredentialRegistry` (Phase 3) |
| Revoked credential reused | On-chain revocation registry checked | `RevocationRegistryReader` (Phase 3) |
| Holder presents someone else's VC | VP signature must come from subject DID | `VerifyPresentationCommandHandler` (Phase 3) |
| JWT secret leak | Loaded from env var, validated >=32 bytes | `JwtBearerTokenIssuer` |
| Issuer key compromise | Out of scope v1 — documented in operations playbook |

### 8.3 Mandatory security controls per BC

Every BC handling crypto operations MUST include:

1. **Adversarial test suite**: at least one test per attack vector
2. **Input validation at the boundary**: every public method validates inputs
3. **Audit log**: security-relevant operations logged with structured fields
4. **Secret hygiene**: no secrets in source, in logs, or in error messages

---

## 9. Quality Gates

These are non-negotiable for any PR to merge into `main`.

### 9.1 Build & test

- `dotnet build` succeeds with zero warnings (TreatWarningsAsErrors)
- `dotnet test` passes 100% (excluding `Category=Integration` in CI)
- Per-BC coverage ≥ 70% (line, branch, method) — enforced by `Check-BcCoverage.ps1`

### 9.2 Architecture rules (CI-enforced)

All `ArchitectureRulesTests` pass:
- `SharedKernel_Domain_Has_No_SovereignID_Dependencies`
- `BC_Domain_Only_References_SharedKernel_Domain`
- `Application_Layer_Has_No_Reference_To_Infrastructure`
- `BCs_Cannot_Reference_Each_Other`
- `No_Project_Outside_Legacy_References_Legacy`
- `Nethereum_Is_Confined_To_Infrastructure_And_Legacy`
- `No_Project_References_MediatR_Package`

### 9.3 Static analysis

- SonarCloud quality gate must pass (no new bugs, vulnerabilities, code smells above threshold)
- (TO ADD) `SecurityCodeScan` — SAST for .NET
- (TO ADD) `Dependabot` — weekly NuGet scan for CVEs

### 9.4 Pull request checklist

Every PR must complete `.github/PULL_REQUEST_TEMPLATE.md`:
- No private keys, secrets, or PII in code or logs
- New public methods have unit tests
- New crypto operations have adversarial tests
- Updated `AGENTS.md` if active phase changed
- Updated this document if architecture changed (with version bump)

---

## 10. Phase Roadmap

### 10.1 Status snapshot

| Phase | Description | Status | Bounded contexts |
|-------|-------------|--------|------------------|
| 1 | Crypto primitives (legacy demo) | ✅ DONE | `legacy/Crypto`, `legacy/Chain` |
| 2 | SIWE authentication | ✅ DONE | `bc-auth` |
| 3 | Verifiable Credentials + IPFS | ⏳ NEXT | `bc-issuer`, `bc-verifier`, `bc-storage` |
| 4 | Holder wallet UI + selective disclosure | ⏳ Pending | `bc-holder` (frontend) |
| 5 | Deployment + portfolio | ⏳ Pending | DevOps |

### 10.2 Pre-Phase 3 housekeeping

Before starting Phase 3, complete these recommendations from the audit:

- [ ] Update `AGENTS.md`: change Active Phase to "Phase 3 — Verifiable Credentials"
- [ ] Add `ExpirationTime` validation in `VerifySiweCommandHandler` + adversarial test
- [ ] Add `.github/dependabot.yml` for NuGet weekly scans
- [ ] Add `SecurityCodeScan` to `ci.yml`
- [ ] Add `.github/PULL_REQUEST_TEMPLATE.md` with security checklist
- [ ] Make `SessionTtlHours` configurable via `AuthOptions`

### 10.3 Phase 3 — Verifiable Credentials (estimated 4 weeks)

**Week 1**: Foundation
- Define VC JSON-LD schemas for the 4 academic credential types
- Define EIP-712 typed data structures
- Implement `bc-issuer` Domain layer (entities, value objects, ports)
- Implement `bc-verifier` Domain layer

**Week 2**: Issuer
- Implement `Eip712CredentialSigner`
- Deploy `CredentialRegistry.sol` and `RevocationRegistry.sol` to Sepolia
- Implement `EthereumCredentialRegistryWriter`
- Implement `Web3StorageUploader` (`bc-storage`)
- `IssueCredentialCommandHandler` end-to-end

**Week 3**: Verifier
- Implement `EthereumCredentialRegistryReader`
- Implement `Web3StorageDownloader`
- Implement `Eip712CredentialSignatureVerifier`
- `VerifyPresentationCommandHandler` end-to-end

**Week 4**: Integration & polish
- API endpoints for both BCs
- Integration tests on Sepolia
- Demo: emit a credential, verify it from a different machine, no shared DB

### 10.4 Phase 4 — Holder Wallet (estimated 3 weeks)

- Blazor WebAssembly wallet for the holder
- Receive VCs (paste JSON or scan QR)
- Store locally (IndexedDB)
- Build VPs and submit to verifier
- (Stretch) Selective disclosure: present only specific claims

### 10.5 Phase 5 — Deployment & portfolio (estimated 2 weeks)

- Deploy Auth API + Issuer API + Verifier API to Railway/Render (free tier)
- Public demo URL
- Architecture diagram + 3-min video
- Thesis defense rehearsal

---

## 11. Spec Generation Plan

This is the list of **OpenSpec changes** to generate next, in order. Each one
becomes a folder under `openspec/changes/` with `proposal.md`, `design.md`,
and `tasks.md`.

### 11.1 Pre-Phase-3 specs

| # | Spec name | Scope | Estimated effort |
|---|-----------|-------|------------------|
| 1 | `pre-phase-3-housekeeping` | Update AGENTS.md, add Dependabot, SecurityCodeScan, PR template | 1 day |
| 2 | `auth-expiration-time-validation` | Validate ExpirationTime in SIWE messages | 1 day |
| 3 | `auth-session-ttl-config` | Make SessionTtl configurable | 0.5 day |

### 11.2 Phase 3 specs (split for manageability)

| # | Spec name | Scope | Estimated effort |
|---|-----------|-------|------------------|
| 4 | `phase-3-vc-domain-model` | Domain layer for `bc-issuer` and `bc-verifier`, no infrastructure | 1 week |
| 5 | `phase-3-credential-registry-contract` | Solidity contracts + deployment scripts | 3 days |
| 6 | `phase-3-issuer-eip712-signing` | EIP-712 signer + tests with known vectors | 4 days |
| 7 | `phase-3-storage-web3storage-adapter` | `bc-storage` IPFS upload/download | 4 days |
| 8 | `phase-3-issuer-api` | Issuer endpoints, end-to-end with adversarial tests | 1 week |
| 9 | `phase-3-verifier-api` | Verifier endpoints + integration tests on Sepolia | 1 week |

### 11.3 Phase 4 specs (preliminary)

| # | Spec name | Scope |
|---|-----------|-------|
| 10 | `phase-4-holder-wallet-blazor` | Blazor WASM wallet for the holder |
| 11 | `phase-4-vp-builder` | Verifiable Presentation construction |
| 12 | `phase-4-selective-disclosure` | Present subset of claims (stretch) |

### 11.4 How to use this plan with Cursor

For each spec in the table:

```
Read openspec/specs/master-design.md and the existing specs in openspec/changes/.

Create openspec/changes/{spec-name}/ with:
- proposal.md
- design.md
- tasks.md

Follow the format used in openspec/changes/phase-2-siwe-auth/.
The change must align with all principles in master-design.md sections 3, 6, 7, 8, 9.

After creating the spec, summarize what was created and wait for my approval before any implementation.
```

---

## 12. Glossary

| Term | Definition |
|------|------------|
| **BC** | Bounded Context — a self-contained module with its own domain model |
| **VC** | Verifiable Credential — a W3C-standardized signed credential document |
| **VP** | Verifiable Presentation — a wrapper that presents one or more VCs to a verifier |
| **DID** | Decentralized Identifier — a self-owned identifier (e.g., `did:ethr:sepolia:0x...`) |
| **SIWE** | Sign-In with Ethereum (EIP-4361) — auth via wallet signature |
| **CID** | Content Identifier — IPFS hash-based content address |
| **Holder** | The entity holding a VC (in this project: the student) |
| **Issuer** | The entity emitting a VC (in this project: the institute) |
| **Verifier** | The entity validating a VC (in this project: an employer or another university) |
| **EIP-712** | Standard for typed structured data signing in Ethereum |
| **JSON-LD** | JSON for Linked Data — the format used for W3C Verifiable Credentials |
| **CSPRNG** | Cryptographically Secure Pseudo-Random Number Generator |

---

## 13. Decision Log (ADRs)

Brief Architecture Decision Records. Each decision links to the rationale.

### ADR-001: Onion Architecture per BC
**Decision**: every BC follows Domain → Application → Infrastructure → Api.
**Rationale**: enforces dependency direction, enables testing in isolation, swappable infrastructure.
**Status**: Adopted (Phase 2).

### ADR-002: No MediatR
**Decision**: explicit `ICommandHandler<,>` and `IQueryHandler<,>` interfaces.
**Rationale**: avoid hidden dispatch, reduce dependencies, simpler debugging.
**Status**: Adopted (Phase 2). Enforced in CI.

### ADR-003: Manual SIWE parser
**Decision**: implement EIP-4361 parser ourselves instead of using `Nethereum.Templates.Siwe`.
**Rationale**: .NET 9 compatibility issues with the package; full control of validation; teaches SIWE deeply for thesis defense.
**Status**: Adopted (Phase 2).

### ADR-004: In-memory nonce repository for v1
**Decision**: `ConcurrentDictionary` in process for nonces.
**Rationale**: nonces are ephemeral (10 min TTL); no need for persistence in MVP; reduces operational complexity.
**Tradeoff**: nonces lost on restart (acceptable — user retries).
**Status**: Adopted (Phase 2). Reevaluate if multi-instance deployment in Phase 5.

### ADR-005: Sepolia testnet only for v1
**Decision**: no mainnet deployment.
**Rationale**: project is academic; real ETH costs not justified; identical EVM behavior.
**Status**: Adopted (all phases).

### ADR-006: web3.storage for IPFS
**Decision**: use web3.storage HTTP API for IPFS uploads/downloads in `bc-storage`.
**Rationale**: free tier sufficient, no node ops, Filecoin persistence guarantees.
**Alternatives rejected**: self-hosted IPFS (ops burden), Arweave (extra token).
**Status**: Adopted for Phase 3.

### ADR-007: EIP-712 for VC signatures
**Decision**: sign VCs using EIP-712 typed structured data instead of plain JSON-LD signatures.
**Rationale**: native Ethereum signing, MetaMask-friendly, well-defined hashing.
**Status**: Adopted for Phase 3.

### ADR-008: bc-storage as shared infrastructure, not full BC
**Decision**: place IPFS adapter in `src/shared/SovereignID.Storage.Infrastructure/`.
**Rationale**: no business logic of its own; both `bc-issuer` and `bc-verifier` need it identically.
**Status**: Adopted for Phase 3.

### ADR-009: Result over Exceptions for domain errors
**Decision**: use `Result<TValue, TError>` for business logic outcomes.
**Rationale**: explicit error handling, no hidden control flow, easier to test.
**Exceptions reserved for**: truly exceptional conditions and programming errors.
**Status**: Adopted (Phase 2).

### ADR-010: Ports always take CancellationToken
**Decision**: every async port method accepts `CancellationToken` as last parameter.
**Rationale**: enables graceful shutdown, request cancellation, test timeouts.
**Status**: Adopted (Phase 2).

---

## 14. Changelog

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-04-28 | SovereignID team | Initial master design document. Consolidates Phase 1 and Phase 2 decisions; sets foundation for Phase 3+. |

---

> **End of master design document.**
> For implementation specs, see `openspec/changes/`.
> For audit reports, see `docs/audits/`.
> Questions or proposals: open an issue with the `design` label.
