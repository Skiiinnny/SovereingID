# SovereignID

> Decentralized self-sovereign digital identity platform · .NET 9 · Ethereum Sepolia

[![Status](https://img.shields.io/badge/status-active%20development-blue)]()
[![Stack](https://img.shields.io/badge/stack-.NET%209%20%2B%20Nethereum-purple)]()
[![Network](https://img.shields.io/badge/network-Sepolia%20testnet-green)]()

## What is this?

SovereignID lets users authenticate and share identity credentials without
depending on Google, Microsoft, or any centralized provider.

- **No passwords** — authentication via cryptographic signature (SIWE)
- **No central authority** — identity anchored on public blockchain
- **User-controlled** — credentials live in the user's wallet, not your database
- **Privacy-preserving** — prove claims without revealing raw data

## Documentation map

| Audience | Start here |
|----------|------------|
| People contributing to the repo | [`docs/onboarding.md`](docs/onboarding.md) — clone, build, tests, CI, env vars, OpenSpec pointers |
| AI coding assistants (Cursor, etc.) | [`AGENTS.md`](AGENTS.md) — spec-first workflow, active phase, conventions |
| Domain language by bounded context | [`CONTEXT-MAP.md`](CONTEXT-MAP.md) — links to per-context `CONTEXT.md` files as they are added |

Authoritative order when docs conflict: `openspec/` → `AGENTS.md` → `CONTEXT.md` (per context) → `docs/adr/` → human summaries in this README and `docs/onboarding.md`.

## Architecture

```
Issuer API  →  signs Verifiable Credentials
Holder      →  stores and presents VCs from wallet
Verifier API →  validates VCs without contacting the Issuer
Auth Service →  SIWE login → JWT session
```

Full architecture: [openspec/specs/architecture.md](openspec/specs/architecture.md)

## Project status

| Phase | Description | Status |
|-------|-------------|--------|
| 1 | Crypto primitives (keys, signing, notarization) | ✅ Demo + tests |
| 2 | SIWE authentication (login without password) | ✅ Demo + tests |
| 3 | W3C Verifiable Credentials (issue + verify) | ⏳ Pending |
| 4 | KYC portable use case + revocation | ⏳ Pending |
| 5 | Deployment + portfolio | ⏳ Pending |

This table is the **product roadmap** (what exists in the tree). For **OpenSpec**—which `tasks.md` assistants should execute—see **Active OpenSpec change** in [`AGENTS.md`](AGENTS.md); that pointer tracks the live change folder under `openspec/changes/`, not the same numbering as the rows above.

## Quick start

Prerequisites: [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0).

```bash
git clone https://github.com/YOUR_USERNAME/sovereign-id-openspec.git
cd sovereign-id-openspec
dotnet restore SovereignID.sln
dotnet test SovereignID.sln
```

Optional: run the Phase 1 console demo with `dotnet run --project src/legacy/SovereignID.Demo.Phase1`.

**Everything else** (Auth API demo, Sepolia env vars, coverage commands, Sonar/CI checklist, repo tree, OpenSpec layout): **[`docs/onboarding.md`](docs/onboarding.md)**.

## Standards

- [EIP-4361](https://eips.ethereum.org/EIPS/eip-4361) — Sign-In with Ethereum
- [W3C DID Core](https://www.w3.org/TR/did-core/) — Decentralized Identifiers
- [W3C VC Data Model](https://www.w3.org/TR/vc-data-model/) — Verifiable Credentials

## License

MIT
