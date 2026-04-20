# SovereignID

> Decentralized self-sovereign digital identity platform · .NET 9 · Ethereum Sepolia

[![Status](https://img.shields.io/badge/status-phase%201%20in%20progress-blue)]()
[![Stack](https://img.shields.io/badge/stack-.NET%209%20%2B%20Nethereum-purple)]()
[![Network](https://img.shields.io/badge/network-Sepolia%20testnet-green)]()

## What is this?

SovereignID lets users authenticate and share identity credentials without
depending on Google, Microsoft, or any centralized provider.

- **No passwords** — authentication via cryptographic signature (SIWE)
- **No central authority** — identity anchored on public blockchain
- **User-controlled** — credentials live in the user's wallet, not your database
- **Privacy-preserving** — prove claims without revealing raw data

## Architecture

```
Issuer API  →  signs Verifiable Credentials
Holder      →  stores and presents VCs from wallet
Verifier API →  validates VCs without contacting the Issuer
Auth Service →  SIWE login → JWT session
```

Full architecture: [openspec/specs/architecture.md](openspec/specs/architecture.md)

## Project Status

| Phase | Description | Status |
|-------|-------------|--------|
| 1 | Crypto primitives (keys, signing, notarization) | ✅ Demo + tests |
| 2 | SIWE authentication (login without password) | ⏳ Pending |
| 3 | W3C Verifiable Credentials (issue + verify) | ⏳ Pending |
| 4 | KYC portable use case + revocation | ⏳ Pending |
| 5 | Deployment + portfolio | ⏳ Pending |

## Repository layout (.NET)

```
SovereignID.sln
src/
  SovereignID.Crypto/        # keys, personal_sign, SHA-256 helpers
  SovereignID.Chain/         # Sepolia RPC + Notary client
  SovereignID.Demo.Phase1/    # runnable console walkthrough
tests/
  SovereignID.Crypto.Tests/
  SovereignID.Chain.Tests/   # includes optional Sepolia integration tests
contracts/
  Notary.sol                   # minimal on-chain hash registry
```

## Quick Start

Prerequisites: [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) and (optionally) a free Sepolia RPC key from [Infura](https://infura.io/) or [Alchemy](https://www.alchemy.com/).

```bash
git clone https://github.com/YOUR_USERNAME/sovereign-id-openspec.git
cd sovereign-id-openspec

dotnet restore SovereignID.sln
dotnet test SovereignID.sln

dotnet run --project src/SovereignID.Demo.Phase1
```

### Local configuration (optional)

`appsettings.Development.json` is ignored by git. Copy the example file and fill only non-secret defaults if you prefer files over environment variables:

```bash
cp src/SovereignID.Demo.Phase1/appsettings.Development.example.json src/SovereignID.Demo.Phase1/appsettings.Development.json
```

Never commit private keys. Prefer environment variables:

| Variable | Purpose |
|----------|---------|
| `SEPOLIA_RPC_URL` | HTTPS JSON-RPC endpoint for Sepolia |
| `NOTARY_CONTRACT_ADDRESS` | Deployed `Notary.sol` address |
| `NOTARIZE_DEMO_PRIVATE_KEY` | Funded test key **env only** for demo transactions |
| `NOTARIZE_TEST_PRIVATE_KEY` | Same idea, used by integration tests |
| `SEPOLIA_TEST_ADDRESS` | Optional; defaults to zero address for balance smoke test |

PowerShell example:

```powershell
$env:SEPOLIA_RPC_URL="https://sepolia.infura.io/v3/<YOUR_KEY>"
dotnet run --project src/SovereignID.Demo.Phase1
```

### Integration tests (Sepolia)

```bash
dotnet test SovereignID.sln --filter "Category=Integration"
```

Tests are skipped automatically when required variables are missing, so CI stays green without secrets.

### Deploying `Notary.sol`

Compile `contracts/Notary.sol` in [Remix](https://remix.ethereum.org/) (or your toolchain), deploy to Sepolia, and export `NOTARY_CONTRACT_ADDRESS`. Request Sepolia ETH from a public faucet (for example [sepoliafaucet.com](https://www.sepoliafaucet.com/)) before sending transactions.

## Phase 1 demo output (shape)

With only crypto configured (no RPC), the demo still exercises wallets + `personal_sign` verification. When `SEPOLIA_RPC_URL` is present it prints live block height and the generated wallet balance. When `NOTARY_CONTRACT_ADDRESS` and `NOTARIZE_DEMO_PRIVATE_KEY` are present it notarizes `"My important document v1.0"` and verifies the returned transaction hash.

```
=== SovereignID Phase 1 Demo ===

[KeyPair]
Address : 0x...
Public  : 0x04bc...a74509
Private : *** (hidden in output)

[Sign & Verify]
Message   : "Hello SovereignID"
Signature : 0x6225...85181b
Recovered : 0x...
Match     : ✓ TRUE

[Chain — Sepolia]
Block     : 8,432,901
Balance   : 0.05 ETH

[Notarize]
Content   : "My important document v1.0"
SHA256    : 0xabc123...
TxHash    : 0xdef456...
Verified  : ✓ TRUE
```

## Spec-Driven Development

This project uses [OpenSpec](https://github.com/Fission-AI/OpenSpec).
All specs, designs, and task lists live in `openspec/`.

```
openspec/
├── config.yaml             ← OpenSpec workflow (schema) + project context
├── specs/
│   ├── architecture.md     ← system design and contracts
│   └── scenarios.md        ← user scenarios and acceptance criteria
└── changes/
    └── phase-1-crypto-foundations/
        ├── proposal.md     ← why this change
        ├── design.md       ← technical approach
        └── tasks.md        ← implementation checklist
```

## Standards

- [EIP-4361](https://eips.ethereum.org/EIPS/eip-4361) — Sign-In with Ethereum
- [W3C DID Core](https://www.w3.org/TR/did-core/) — Decentralized Identifiers
- [W3C VC Data Model](https://www.w3.org/TR/vc-data-model/) — Verifiable Credentials

## License

MIT
