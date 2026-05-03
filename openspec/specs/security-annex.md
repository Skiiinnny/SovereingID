# SovereignID — Security Architecture Annex

> **Annex to the Master Design Document — Section 8 expanded.**
> Version: 1.0 · Date: April 2026
> Status: Authoritative reference for all security-related specs

This annex expands Section 8 of `master-design.md` with concrete policies,
controls, and mandatory implementations. It supersedes any conflicting
guidance elsewhere.

---

## Table of Contents

1. [Threat Model (STRIDE-based)](#1-threat-model)
2. [Adversary Profiles](#2-adversary-profiles)
3. [Priority 1 — PII Leakage Prevention](#3-priority-1--pii-leakage-prevention)
4. [Priority 2 — Replay & Identity Spoofing](#4-priority-2--replay--identity-spoofing)
5. [Priority 3 — Issuer Key Compromise](#5-priority-3--issuer-key-compromise)
6. [Priority 4 — Credential Forgery](#6-priority-4--credential-forgery)
7. [Priority 5 — Denial of Service](#7-priority-5--denial-of-service)
8. [Defense in Depth Layers](#8-defense-in-depth-layers)
9. [Mandatory Security Controls Matrix](#9-mandatory-security-controls-matrix)
10. [Security Specs to Generate](#10-security-specs-to-generate)
11. [Security Test Catalog](#11-security-test-catalog)
12. [Incident Response Playbook](#12-incident-response-playbook)

---

## 1. Threat Model

We use STRIDE to classify threats, mapped to the academic credentials
context (issuer = institute, holder = student, verifier = employer).

| STRIDE | Threat | Mapped priority |
|--------|--------|-----------------|
| **S**poofing | Student presents another student's credential | P2 |
| **T**ampering | Modify VC after issuance | P4 |
| **R**epudiation | Issuer denies having issued a credential | (covered by on-chain logs) |
| **I**nformation Disclosure | Personal data of student leaks publicly | **P1** |
| **D**enial of Service | Attacker exhausts API or RPC quota | P5 |
| **E**levation of Privilege | Attacker becomes issuer without authorization | P3 |

The priorities (P1-P5) you ranked drive the depth of controls in each area.

---

## 2. Adversary Profiles

Four adversary profiles in scope:

### Profile A — External attacker
- **Capability**: network access, can intercept HTTPS at the edge
- **Motivation**: monetary (sell credentials, ransomware) or reputational
- **Mitigation focus**: TLS, HSTS, secrets in env vars, dependency scanning

### Profile B — Malicious student
- **Capability**: can sign valid SIWE messages with their own wallet, can craft malicious VPs
- **Motivation**: forge their own credentials, present someone else's, claim degrees they didn't earn
- **Mitigation focus**: subject DID binding in VP, issuer signature verification, on-chain registry

### Profile C — Malicious insider (institute admin)
- **Capability**: legitimate API access, can issue arbitrary credentials, can read all DB data
- **Motivation**: corruption (sell fake degrees), data theft, sabotage
- **Mitigation focus**: audit log on-chain, multi-sig issuance for high-value credentials, separation of duties

### Profile D — Supply chain attacker
- **Capability**: compromise NuGet packages, GitHub Actions, dependencies
- **Motivation**: persistence in many downstream systems
- **Mitigation focus**: Dependabot, signed releases, pinned versions, SLSA-aware CI

---

## 3. Priority 1 — PII Leakage Prevention

> **The single most important risk in the project.** Once data is on a public
> blockchain or public IPFS, it cannot be deleted. GDPR-equivalent regulations
> (and basic ethics) demand we never put plaintext PII on either.

### 3.1 Data classification

Every piece of data MUST be classified before storage:

| Class | Examples | Allowed storage |
|-------|----------|-----------------|
| **Public** | DID, credential ID, credential type, issuance date, hash, CID | Ethereum, IPFS public |
| **Quasi-identifying** | Career name, graduation year | IPFS public (acceptable risk) |
| **PII (personal)** | Full name, DOB, national ID, address, contact | **NEVER on-chain or IPFS public** |
| **Sensitive PII** | Ethnicity, health, disabilities | Out of scope v1 |

### 3.2 Mandatory policies

**P1.1 — Minimal claims policy**
- VCs MUST contain only the minimum claims necessary for the credential type
- No full DOB; use age-related claims (over18, over21) when possible
- No national ID; use DID as identifier
- No physical addresses

**P1.2 — Encryption before IPFS for PII-containing VCs**
- If a VC contains any PII, encrypt the VC body with the subject's public key
  before uploading to IPFS
- Use ECIES (Elliptic Curve Integrated Encryption Scheme) compatible with secp256k1
- Only the holder (with their private key) can decrypt
- The public CID and hash on Ethereum still work for integrity verification

**P1.3 — Field-level redaction at presentation**
- VPs MUST allow the holder to omit fields that the verifier doesn't need
- Hash tree structure (Merkle proofs) for selective disclosure (Phase 4)
- Until Phase 4, presentation is all-or-nothing — document this limitation

**P1.4 — No PII in logs**
- Logs MUST NOT contain: full names, DOBs, national IDs, contact info
- Allowed in logs: DIDs (truncated), credential IDs (UUIDs), error codes, timing
- Use structured logging with schema validation in CI (Serilog + custom analyzer)

**P1.5 — No PII in error messages returned to clients**
- Error responses MUST be generic for non-authenticated calls
- Detailed errors only for authenticated, authorized callers (and still no PII)

### 3.3 Implementation requirements

| Component | Requirement |
|-----------|-------------|
| `bc-issuer` | Validates VC schema before signing — rejects PII-containing fields not in allowlist |
| `bc-issuer` | Encrypts VC with subject pubkey when ANY PII field present (P1.2) |
| `bc-verifier` | Documents in API response which fields were not disclosed |
| Logging middleware | Redacts known PII patterns automatically (regex for emails, phone, IDs) |
| API responses | Generic error codes only; `error_detail` only behind authentication |

---

## 4. Priority 2 — Replay & Identity Spoofing

### 4.1 Already implemented (verified in audit)

These controls exist and are tested in the current Phase 2 code:

- ✅ Single-use nonces (consumed on first valid use)
- ✅ Nonce TTL (10 min default, configurable)
- ✅ Chain ID locked to Sepolia (rejects mainnet signatures = anti-phishing)
- ✅ Address recovered from signature must match address in SIWE message
- ✅ Case-insensitive comparison (checksum vs lowercase)

### 4.2 Additional controls required for Phase 3+

**P2.1 — VP nonce binding**
- Verifier issues a `presentation_nonce` before accepting a VP
- VP signature MUST include the nonce
- Same single-use TTL semantics as auth nonces
- Prevents replay of captured VPs to other verifiers

**P2.2 — Subject DID binding in VPs**
- The VP signature MUST be verifiable against the public key of the
  `credentialSubject.id` of every VC in the VP
- Without this, a student could present another student's VC

**P2.3 — Audience binding**
- VPs MUST include the verifier's identifier (`aud` claim or equivalent)
- Verifier rejects VPs whose audience is not itself

**P2.4 — Time-bound presentations**
- VPs include `iat` (issued at) and `exp` (expires at)
- Verifier rejects VPs older than 5 minutes

**P2.5 — Anti-CSRF for state-changing endpoints**
- POST endpoints require either:
  - SameSite=Strict cookie, OR
  - Bearer token in Authorization header (current approach)
- Documented in API contract

---

## 5. Priority 3 — Issuer Key Compromise

> The issuer's private key is the **highest-value asset** in the system.
> A leaked key = anyone can issue valid credentials for that institution.

### 5.1 Key management policy

**P3.1 — Key separation**
- Different keys for different environments (dev, test, prod-pilot)
- Different keys per institution (when supporting multiple)
- Issuer key NEVER reused for any other purpose (no SIWE auth, no personal use)

**P3.2 — Key storage**
| Environment | Storage |
|-------------|---------|
| Local dev | `.env` file (gitignored) — clearly marked TEST KEY |
| CI | GitHub Secrets — accessed only by deployment workflows |
| Sepolia demo | Environment variable — short-lived test key, rotatable |
| Production pilot (future) | HSM (hardware) or AWS KMS / Azure Key Vault |
| Multi-issuer production | One key per issuer in their own KMS |

**P3.3 — Key rotation procedure**
- Documented in `docs/security/key-rotation.md`
- New issuer DID published on-chain with rotation event
- Old credentials remain valid (signature still verifiable)
- New credentials use new key
- Time-bound overlap window (configurable)

**P3.4 — Compromise detection**
- Audit log on-chain: every issuance event is public on the registry
- Anomaly detection: alerting on unexpected issuance patterns (post-MVP)
- For thesis MVP: at minimum, log issuances structurally for offline analysis

**P3.5 — Compromise response**
- Immediately revoke the compromised key's authority via `IssuerRegistry`
  smart contract (deployed on-chain control)
- Mass-revoke credentials issued in the suspicious window via batch call
- See §12 (Incident Response Playbook)

### 5.2 Mandatory implementations for Phase 3

| Control | Implementation |
|---------|---------------|
| Key never in source | Validated by SecurityCodeScan + git pre-commit hook |
| Key validated at startup | `IIssuerKeyProvider` throws if key < 32 bytes or invalid format |
| Key never logged | Logging middleware filters keys; explicit test for this |
| Key rotation event | `IssuerRegistry.sol` supports `rotateKey(oldDid, newDid, signature)` |
| Multi-sig for high-value (Phase 5+) | Documented in roadmap; not in MVP |

---

## 6. Priority 4 — Credential Forgery

### 6.1 Forgery vectors and defenses

| Forgery vector | Defense |
|----------------|---------|
| Modify VC JSON after issuance | SHA-256 on-chain — verification fails |
| Re-sign VC with attacker key | Issuer DID on-chain — verifier checks signer = registered issuer |
| Submit VC for non-existent credential ID | `CredentialRegistry.get()` returns null — verifier rejects |
| Replace VC content on IPFS | IPFS is content-addressed — different content = different CID |
| Forge issuer signature directly | Cryptographic security of secp256k1 — computationally infeasible |
| Issuer ID collision | UUIDs (128 bits) + on-chain registry uniqueness |

### 6.2 Mandatory verification flow

Every verification MUST execute these checks IN ORDER:

```
1. Parse VP — fail on malformed JSON
2. Verify VP signature against credentialSubject.id of each VC
3. For each VC in VP:
   a. Extract credentialId
   b. Read CredentialRegistry on-chain → get { issuerDid, cid, hash, timestamp }
   c. Read RevocationRegistry on-chain → fail if revoked
   d. Verify timestamp < now AND timestamp not too old (configurable max age)
   e. Download VC from IPFS using CID
   f. Compute SHA-256 of downloaded VC → must equal hash on-chain
   g. Resolve issuerDid → get public key
   h. Verify issuer signature on VC using public key
   i. Check issuerDid is in IssuerRegistry (if multi-issuer support)
4. Extract claims and return
```

If ANY step fails, the entire verification fails with a specific error code.

### 6.3 Mandatory implementations

- `Eip712CredentialSignatureVerifier` with vector tests against known good/bad signatures
- `IpfsContentIntegrityVerifier` that always re-hashes downloaded content
- `CredentialAgeValidator` with configurable max age
- Explicit rejection of credentials whose `issuanceDate` is in the future (clock skew tolerance: 1 minute)

---

## 7. Priority 5 — Denial of Service

> Lower priority but still must be addressed. The thesis demo must survive
> a curious user hitting the nonce endpoint repeatedly.

### 7.1 Mandatory controls

**P5.1 — Rate limiting per IP**
- `GET /auth/nonce`: max 10 req/min per IP
- `POST /auth/verify`: max 20 req/min per IP
- `POST /credentials/verify`: max 60 req/min per IP (read-heavy)
- `POST /credentials/issue`: max 20 req/min per authenticated issuer
- Use ASP.NET Core built-in `AddRateLimiter` (no external dependency)

**P5.2 — Request size limits**
- Max body size: 10 KB for auth endpoints (SIWE messages are small)
- Max body size: 100 KB for VC operations (large VCs are a smell)
- Reject larger requests with 413 Payload Too Large

**P5.3 — Timeout policy**
- All outbound calls (RPC, IPFS) have explicit timeouts
- Default: 10 seconds for RPC, 30 seconds for IPFS download
- Use `IHttpClientFactory` with policies via Polly

**P5.4 — Resource exhaustion protection**
- `InMemoryAuthChallengeRepository` MUST cap total entries (configurable, default 100,000)
- When cap reached, oldest expired entries evicted aggressively
- When still at cap with no expired entries, reject new nonce requests with 503

**P5.5 — IPFS availability**
- Verifier caches downloaded VCs with TTL (configurable, default 5 min)
- Cache key: CID (immutable by definition, safe to cache long)
- On IPFS failure, retry with exponential backoff (max 3 attempts)

---

## 8. Defense in Depth Layers

Every operation passes through ALL applicable layers — no shortcut allowed.

```
┌────────────────────────────────────────────────────────────┐
│  Layer 1: Network                                          │
│  HTTPS only, HSTS, modern TLS, valid certificate           │
└────────────────────────────────────────────────────────────┘
                          ↓
┌────────────────────────────────────────────────────────────┐
│  Layer 2: Edge                                             │
│  Rate limiting, body size limits, CORS allowlist           │
└────────────────────────────────────────────────────────────┘
                          ↓
┌────────────────────────────────────────────────────────────┐
│  Layer 3: API contract                                     │
│  Schema validation (FluentValidation), required headers    │
└────────────────────────────────────────────────────────────┘
                          ↓
┌────────────────────────────────────────────────────────────┐
│  Layer 4: Authentication                                   │
│  JWT validation, issuer/audience check, expiration         │
└────────────────────────────────────────────────────────────┘
                          ↓
┌────────────────────────────────────────────────────────────┐
│  Layer 5: Authorization                                    │
│  Role-based: only Issuer role can call /credentials/issue  │
└────────────────────────────────────────────────────────────┘
                          ↓
┌────────────────────────────────────────────────────────────┐
│  Layer 6: Application logic                                │
│  Business rules, Result<T,E> error flow                    │
└────────────────────────────────────────────────────────────┘
                          ↓
┌────────────────────────────────────────────────────────────┐
│  Layer 7: Domain validation                                │
│  Value object construction, invariants enforced            │
└────────────────────────────────────────────────────────────┘
                          ↓
┌────────────────────────────────────────────────────────────┐
│  Layer 8: Cryptographic verification                       │
│  Signature recovery, on-chain hash comparison              │
└────────────────────────────────────────────────────────────┘
                          ↓
┌────────────────────────────────────────────────────────────┐
│  Layer 9: Audit                                            │
│  Structured log of every security-relevant operation       │
└────────────────────────────────────────────────────────────┘
```

If a check fails at any layer, the operation aborts. No exception bubbles
to the user — generic error returned, full detail in audit log.

---

## 9. Mandatory Security Controls Matrix

Every PR adding/modifying code in a BC must comply with the controls
relevant to its area.

### 9.1 bc-auth controls

| Control | Status | Source |
|---------|--------|--------|
| Single-use nonces | ✅ Implemented | `AuthChallenge.Consume()` |
| Nonce TTL | ✅ Implemented | configurable via `NonceTtlSeconds` |
| CSPRNG nonces | ✅ Implemented | `SecureRandomNonceGenerator` |
| Chain ID validation | ✅ Implemented | `ChainId.Create()` |
| Case-insensitive address compare | ✅ Implemented | `VerifySiweCommandHandler` |
| ExpirationTime validation | ⏳ Pending | Hallazgo INFO-01 del audit |
| Rate limiting | ⏳ Pending | New for P5 |
| JWT signing key validation | ✅ Implemented | `JwtBearerTokenIssuer` |
| Audit log of auth events | ⏳ Pending | New for P3 |

### 9.2 bc-issuer controls

| Control | Status | Source |
|---------|--------|--------|
| Issuer key never in source | ⏳ Required | New |
| Issuer key validated at startup | ⏳ Required | New |
| Issuer key never logged | ⏳ Required | New |
| VC schema validation (no unauthorized PII) | ⏳ Required | New for P1 |
| VC encryption when PII present | ⏳ Required | New for P1.2 |
| EIP-712 signature with vector tests | ⏳ Required | New |
| Audit log of issuance events (on-chain) | ⏳ Required | New for P3.4 |
| Rate limiting per authenticated issuer | ⏳ Required | New for P5.1 |

### 9.3 bc-verifier controls

| Control | Status | Source |
|---------|--------|--------|
| VP signature verification | ⏳ Required | New for P2.2 |
| Subject DID binding check | ⏳ Required | New for P2.2 |
| Audience binding check | ⏳ Required | New for P2.3 |
| Time-bound presentations | ⏳ Required | New for P2.4 |
| Mandatory verification flow (§6.2) | ⏳ Required | New for P4 |
| IPFS content re-hashing | ⏳ Required | New for P4 |
| Revocation check on every verify | ⏳ Required | New for P4 |
| Cache with TTL for IPFS reads | ⏳ Required | New for P5.5 |
| Public rate limiting | ⏳ Required | New for P5.1 |

### 9.4 bc-storage controls

| Control | Status | Source |
|---------|--------|--------|
| Content-addressed integrity (CID = hash) | Inherent | IPFS property |
| ECIES encryption helper for PII VCs | ⏳ Required | New for P1.2 |
| Timeout on uploads/downloads | ⏳ Required | New for P5.3 |
| Retry with exponential backoff | ⏳ Required | New for P5.5 |
| No PII leak in error messages | ⏳ Required | New for P1.5 |

### 9.5 Cross-cutting controls

| Control | Status |
|---------|--------|
| HTTPS only with HSTS | ⏳ Required at deployment (Phase 5) |
| CORS allowlist (no wildcards) | ⏳ Required |
| Security headers middleware (CSP, X-Frame-Options, X-Content-Type-Options) | ⏳ Required |
| Logging middleware with PII redaction | ⏳ Required for P1.4 |
| `SecurityCodeScan` SAST in CI | ⏳ Required (was P1 in audit) |
| `Dependabot` for NuGet CVEs | ⏳ Required (was P3 in audit) |
| PR template with security checklist | ⏳ Required (was P5 in audit) |
| Git pre-commit hook for secret detection | ⏳ Recommended (`gitleaks`) |

---

## 10. Security Specs to Generate

These are the specs to add to `openspec/changes/` IN ORDER, before continuing
with normal Phase 3 work. They are bundled into security-focused changes:

### 10.1 Pre-Phase-3 security baseline

| # | Spec name | Scope | Effort |
|---|-----------|-------|--------|
| S1 | `security-baseline-tooling` | SecurityCodeScan, Dependabot, gitleaks pre-commit, PR template | 1 day |
| S2 | `security-pii-redaction-middleware` | Logging middleware that redacts PII patterns automatically | 1 day |
| S3 | `security-rate-limiting` | ASP.NET rate limiter for all endpoints with per-route policies | 1 day |
| S4 | `security-headers-middleware` | HSTS, CSP, X-Frame-Options, X-Content-Type-Options | 0.5 day |
| S5 | `security-request-size-limits` | Body size limits per endpoint | 0.5 day |
| S6 | `auth-expiration-time-validation` | (already in roadmap) | 1 day |
| S7 | `security-audit-log-baseline` | Structured audit log for all auth events | 1 day |

### 10.2 Phase 3 security-specific specs

| # | Spec name | Scope | Effort |
|---|-----------|-------|--------|
| S8 | `issuer-key-management` | `IIssuerKeyProvider` with validation, rotation hooks | 2 days |
| S9 | `vc-schema-validation` | Whitelist of allowed fields per credential type, reject extras | 2 days |
| S10 | `vc-pii-encryption-ecies` | ECIES encryption for VCs containing PII (P1.2) | 4 days |
| S11 | `eip712-vc-signing-with-vectors` | EIP-712 signer + 20+ test vectors (good/bad signatures) | 3 days |
| S12 | `verifier-mandatory-flow` | The 9-step verification flow from §6.2 with adversarial tests | 1 week |
| S13 | `vp-binding-policies` | Subject DID, audience, time, nonce binding (P2) | 4 days |
| S14 | `revocation-registry-on-chain` | `RevocationRegistry.sol` + `IRevocationChecker` | 3 days |

### 10.3 Phase 5 security-specific specs (deployment-time)

| # | Spec name | Scope |
|---|-----------|-------|
| S15 | `production-tls-and-hsts` | TLS 1.3, HSTS with preload |
| S16 | `production-cors-policy` | Strict allowlist for production domains |
| S17 | `production-secrets-kms` | Migration from env vars to AWS KMS or Azure Key Vault |
| S18 | `incident-response-runbook` | Documented procedures for §12 |

---

## 11. Security Test Catalog

Every BC handling crypto MUST have at minimum these adversarial test categories.

### 11.1 Universal categories (all BCs)

- **Input fuzzing**: malformed JSON, oversized payloads, null/empty/unicode edge cases
- **Authentication bypass**: missing token, expired token, wrong signature, swapped keys
- **Authorization bypass**: lower-role user attempting higher-role action
- **Replay**: identical request twice within and outside TTL
- **Time manipulation**: future timestamps, very old timestamps, clock skew
- **Encoding attacks**: hex case mixing, leading zeros, alternative representations

### 11.2 bc-auth specific

- Replay attack on SIWE message (DONE — `Replay_ReturnsNonceConsumed`)
- Wrong Chain ID (DONE — `WrongChainId_ReturnsUnsupportedChain`)
- Tampered SIWE message (DONE — `TamperedMessage_ReturnsSignatureMismatch`)
- Unknown nonce (DONE — `UnknownNonce_ReturnsNonceUnknown`)
- Malformed payload (DONE — `MalformedPayload_ReturnsSiweParseFailed`)
- Expired SIWE `ExpirationTime` field (PENDING — gap from audit)
- Concurrent verification of same nonce (PENDING — race condition test)

### 11.3 bc-issuer specific (Phase 3)

- Forged JWT (signed with wrong key) MUST be rejected
- Issuance attempt without authorization MUST return 403
- VC with PII field not in allowlist MUST be rejected with schema_violation
- VC with future `issuanceDate` MUST be rejected
- VC body size exceeding limit MUST be rejected
- Issuance with key shorter than 32 bytes MUST fail at startup
- Concurrent issuances MUST not produce duplicate credential IDs

### 11.4 bc-verifier specific (Phase 3)

- VC with valid signature but absent from registry MUST fail
- VC present in registry but tampered on IPFS MUST fail (hash mismatch)
- VC valid but revoked MUST fail
- VP signed by wrong subject DID MUST fail
- VP without audience binding MUST be rejected (when audience configured)
- VP older than max age MUST be rejected
- Same VP submitted twice in window MUST be rejected (presentation nonce single-use)
- Verification with stale IPFS cache MUST be acceptable (CIDs are immutable)

### 11.5 bc-storage specific (Phase 3)

- Upload of empty content MUST fail
- Upload of oversized content MUST fail
- Download of non-existent CID MUST timeout gracefully
- Download retry MUST respect exponential backoff (no thundering herd)
- Encryption with invalid public key MUST fail at parameter validation

---

## 12. Incident Response Playbook

Mandatory documented procedures for the most likely incidents. Each
gets its own runbook in `docs/security/incidents/`.

### 12.1 Incident: Issuer key suspected compromised

**Detection signals**: unexpected issuance events, alert from monitoring,
external report, key file accessed by unauthorized process.

**Response steps**:
1. Stop the Issuer API (revoke its access to the key immediately)
2. Call `IssuerRegistry.revokeIssuer(issuerDid)` on-chain
3. Identify suspicious credential IDs in the time window
4. Batch-revoke them via `RevocationRegistry.revokeBatch(ids)`
5. Generate new issuer key pair, register new DID
6. Re-issue legitimate credentials with new key (offline reconciliation)
7. Public communication to verifiers (out of scope MVP — documented for Phase 5)

**Recovery time objective**: < 4 hours for academic context.

### 12.2 Incident: PII leak on IPFS

**Detection signals**: external report, audit finding, content classifier alert.

**Response steps**:
1. Identify the leaked CID(s) and their associated credentials
2. Mark associated credentials as revoked on-chain
3. Request IPFS unpinning from web3.storage (best effort — IPFS is decentralized)
4. Notify affected subjects (out of scope MVP — documented)
5. Root-cause analysis: how did PII bypass schema validation?
6. Update VC schema validation rules to prevent recurrence

**Note**: IPFS is essentially permanent for any popular content. The lesson:
**prevention over response** for PII. Once leaked, it cannot be deleted.

### 12.3 Incident: Mass DoS on verifier

**Detection signals**: rate limit alerts, RPC quota exhaustion, IPFS quota.

**Response steps**:
1. Tighten rate limits temporarily (lower thresholds via config)
2. Increase cache TTL for IPFS reads (CIDs are immutable, safe to cache long)
3. If using free tier RPC, switch to paid tier or rotate providers
4. Identify attacking IP ranges, block at edge
5. Post-mortem: were the limits adequate? Should defaults change?

### 12.4 Incident: Smart contract bug discovered

**Detection signals**: external researcher, audit finding, unexpected on-chain state.

**Response steps**:
1. Document the bug clearly (PoC if possible)
2. If exploitable: pause new issuances at the API layer (cannot pause the contract — immutable)
3. Migrate to new contract version (deploy new, point API to new address)
4. Old credentials still verifiable against old contract (immutable history)
5. Communicate to verifiers about the new contract address

---

## Appendix A — Dependencies and Tools to Add

| Tool | Purpose | Priority |
|------|---------|----------|
| `SecurityCodeScan.VS2019` (NuGet) | SAST for .NET, detects common vulnerabilities | High |
| `Microsoft.AspNetCore.RateLimiting` | Built-in rate limiter | High |
| `FluentValidation.AspNetCore` | Request validation pipelines | High |
| `Polly` | Retry/timeout/circuit breaker for outbound calls | High |
| `Serilog` + `Serilog.Sinks.Console` | Structured logging with redaction support | High |
| `gitleaks` (pre-commit) | Detect secrets in commits before push | Medium |
| `dotnet-outdated` | Surface outdated packages | Medium |
| `GitHub Dependabot` | Automated CVE alerts | High (already recommended in audit) |

---

## Appendix B — How this annex relates to existing documents

- Extends Section 8 of `master-design.md` (Security Model)
- Provides concrete implementations of P1-P5 priorities
- All specs in §10 inherit principles from `master-design.md` §3 (Architectural Principles)
- All controls in §9 must be enforced by tests defined in §11
- All incident procedures in §12 will become detailed runbooks in `docs/security/incidents/`

---

> **End of Security Architecture Annex.**
> Updates require a version bump and changelog entry in `master-design.md`.
