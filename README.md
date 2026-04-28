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
| 2 | SIWE authentication (login without password) | ✅ Demo + tests |
| 3 | W3C Verifiable Credentials (issue + verify) | ⏳ Pending |
| 4 | KYC portable use case + revocation | ⏳ Pending |
| 5 | Deployment + portfolio | ⏳ Pending |

## Repository layout (.NET)

```
SovereignID.sln
src/
  shared/
    SovereignID.SharedKernel.Domain/
    SovereignID.SharedKernel.Application/
    SovereignID.SharedKernel.Infrastructure/
  bc-auth/
    SovereignID.Auth.Domain/
    SovereignID.Auth.Application/
    SovereignID.Auth.Infrastructure/
  bc-issuer/
    SovereignID.Issuer.Domain/
    SovereignID.Issuer.Application/
    SovereignID.Issuer.Infrastructure/
  bc-verifier/
    SovereignID.Verifier.Domain/
    SovereignID.Verifier.Application/
    SovereignID.Verifier.Infrastructure/
  legacy/                    # frozen Phase 1 code, not referenced by new code
    SovereignID.Crypto/
    SovereignID.Chain/
    SovereignID.Demo.Phase1/
tests/
  architecture/SovereignID.Architecture.Tests/
  legacy/SovereignID.Crypto.Tests/
  legacy/SovereignID.Chain.Tests/  # includes optional Sepolia integration tests
contracts/
  Notary.sol                   # minimal on-chain hash registry
```

Architecture rules are specified in `openspec/specs/solution-architecture/spec.md`.

## Quick Start

Prerequisites: [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) and (optionally) a free Sepolia RPC key from [Infura](https://infura.io/) or [Alchemy](https://www.alchemy.com/).

```bash
git clone https://github.com/YOUR_USERNAME/sovereign-id-openspec.git
cd sovereign-id-openspec

dotnet restore SovereignID.sln
dotnet test SovereignID.sln

dotnet run --project src/legacy/SovereignID.Demo.Phase1
```

### Run Auth demo (Phase 2)

Set a 32-byte signing key and run the Auth API:

```powershell
$env:AUTH_JWT_SIGNING_KEY="replace-with-a-random-32-byte-secret"
dotnet run --project src/bc-auth/SovereignID.Auth.Api
```

Then open [http://localhost:5000/](http://localhost:5000/) (or the URL shown by `dotnet run`) and click **Sign in with Ethereum**.
MetaMask will sign the SIWE payload and the page will render the issued JWT and signer address.

### Local configuration (optional)

`appsettings.Development.json` is ignored by git. Copy the example file and fill only non-secret defaults if you prefer files over environment variables:

```bash
cp src/legacy/SovereignID.Demo.Phase1/appsettings.Development.example.json src/legacy/SovereignID.Demo.Phase1/appsettings.Development.json
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
dotnet run --project src/legacy/SovereignID.Demo.Phase1
```

### Integration tests (Sepolia)

```bash
dotnet test SovereignID.sln --filter "Category=Integration"
```

Tests are skipped automatically when required variables are missing, so CI stays green without secrets.

### Run tests with coverage

```bash
dotnet test SovereignID.sln --filter "Category!=Integration" --settings coverlet.runsettings --results-directory coverage
```

Coverage reports are generated per test project under `coverage/**` in both formats:

- `coverage.opencover.xml` (used by SonarQube)
- `coverage.cobertura.xml` (used by CI artifact and per-BC validation)

The shared `coverlet.runsettings` enforces a local 70% threshold (`line`, `branch`, `method`) so the command fails early when coverage drops below the floor.

### Integration test convention

`[Trait("Category","Integration")]` is reserved only for tests that require real external I/O (for example live Sepolia RPC, deployed contracts, external DB, or filesystem outside test output directories).

`SovereignID.Auth.IntegrationTests` currently hosts in-process `WebApplicationFactory<Program>` tests that do not hit external I/O; those tests intentionally do not carry the `Integration` trait and run in default CI coverage.

### CI / SonarQube pipeline

Build and test now run in `.github/workflows/ci.yml` (`ubuntu-latest`) and produce the `coverage/` artifact after:

- restore
- build
- test with coverage (`Category!=Integration`)
- per-BC coverage check via `ci/Check-BcCoverage.ps1`

Sonar analysis runs in `.github/workflows/sonarqube.yml` (`windows-latest`), downloads the same coverage artifact, and passes:

- `sonar.cs.opencover.reportsPaths`
- `sonar.exclusions`
- `sonar.coverage.exclusions`
- `sonar.qualitygate.wait=true` (on `dotnet-sonarscanner begin`, not `end`)

### SonarCloud in CI

The repository includes a dedicated workflow at `.github/workflows/sonarqube.yml`.

- Triggered on `push` to `main` and on `pull_request` (`opened`, `synchronize`, `reopened`).
- Uses `dotnet-sonarscanner` on `windows-latest` with cache enabled.
- Requires the `SONAR_TOKEN` secret configured in the repository settings.

PR verification checklist:

1. Open or update a PR targeting `main`.
2. Confirm both workflows complete successfully: `CI` and `SonarQube`.
3. Confirm SonarCloud reports the analysis for the PR branch with no token/auth errors.

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
