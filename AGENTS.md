# AGENTS.md — AI Assistant Instructions

> SovereignID project · OpenSpec configuration

## Project Context

You are helping build **SovereignID**, a .NET platform for decentralized
self-sovereign digital identity. The stack is C# / .NET 9, Nethereum,
SIWE (EIP-4361), W3C Verifiable Credentials, and Ethereum Sepolia testnet.

The developer is a Senior .NET engineer learning blockchain/identity concepts
while building a portfolio product. Favor correctness and clean architecture
over brevity. Always explain crypto concepts when they appear in code.

**Human contributors:** see [`docs/onboarding.md`](docs/onboarding.md) for clone/build/CI and how this repo’s docs relate to `openspec/` and this file.

## Spec-Driven Rules

1. **Read the spec before writing code.** Always check `openspec/specs/` and
   the active change folder before implementing anything.

2. **One change at a time.** Work on the current phase tasks in `tasks.md`.
   Do not skip ahead to future phases.

3. **Update tasks as you go.** Mark tasks `[x]` when complete.

4. **Never hardcode secrets.** No private keys, RPC URLs, or API keys in source.
   Always use environment variables or `appsettings.Development.json` (gitignored).

5. **Always write tests.** Every public method in `SovereignID.Crypto` needs a
   unit test. Integration tests (hitting Sepolia) must be marked
   `[Trait("Category","Integration")]` so they can be skipped in CI.

## Code Conventions

- Async everywhere: `Task<T>`, never `.Result` or `.Wait()`
- Records for immutable data (KeyPair, VC, DID Document)
- Interfaces for all external dependencies (blockchain, storage)
- XML doc comments on all public APIs
- No `Console.WriteLine` in library code — use `ILogger<T>`

## Naming

| Concept | C# Type Name |
|---------|-------------|
| Key pair | `KeyPair` |
| Verifiable Credential | `VerifiableCredential` |
| Verifiable Presentation | `VerifiablePresentation` |
| Decentralized Identifier | `DecentralizedIdentifier` (or `Did`) |
| SIWE message | `SiweMessage` (use Nethereum type) |

## What NOT to do

- Do not implement ZK proofs — out of scope for v1
- Do not deploy to mainnet — Sepolia only
- Do not add UI until Phase 4
- Do not use `.Wait()` or block on async
- Do not log private keys, even partially

## Active OpenSpec change (not the README roadmap)

**Two different “phases”:**

- **README → Project status** — Product roadmap (which milestones are already shipped in the repo, e.g. crypto + SIWE demos). Use that table for **scope and marketing truth**.
- **This section** — Which folder under `openspec/changes/` holds the **`tasks.md` you must follow** for spec-driven implementation. That folder name does not always match the roadmap row number.

**Current change (follow this `tasks.md`):**  
None — no product-phase OpenSpec change is active. In-flight repo work (example): `openspec/changes/sonar-coverage-phase-2/tasks.md`. When Phase 3 (or the next milestone) is proposed, point this line at its folder.

**Reference (archived changes):**  
Phase 1 crypto foundations → `openspec/changes/archive/2026-04-19-phase-1-crypto-foundations/`.  
Phase 2 — SIWE authentication → `openspec/changes/archive/2026-05-03-phase-2-siwe-auth/`.  
SonarQube debt (Fase 2 alignment) → `openspec/changes/archive/2026-05-03-sonar-debt-phase-2/`.

When the current change is complete and moved to `openspec/changes/archive/`, update the **Current change** path above to the next active folder.

## Agent skills

### Issue tracker

GitHub Issues in this clone’s default remote (`gh` CLI). See `docs/agents/issue-tracker.md`.

### Triage labels

Canonical roles use the default label strings (`needs-triage`, `needs-info`, `ready-for-agent`, `ready-for-human`, `wontfix`). See `docs/agents/triage-labels.md`.

### Domain docs

Multi-context: one `CONTEXT.md` per bounded context, indexed from `CONTEXT-MAP.md` at the repo root; system ADRs in `docs/adr/`. See `docs/agents/domain.md`. Glossary files are added when domain language is fixed; the map lists their **paths**, not a guarantee that every file is populated yet.
