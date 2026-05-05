# Contributor onboarding

Human-oriented guide to the repository. Project overview and badges: [`README.md`](../README.md). **Coding assistants** should read [`AGENTS.md`](../AGENTS.md) at the repo root first.

## Where truth lives (read in this order)

1. **`openspec/`** — what to build, acceptance criteria, and the active change’s `tasks.md`.
2. **`AGENTS.md`** — how assistants should work (OpenSpec flow, conventions, links). Its **Active OpenSpec change** is which `tasks.md` to follow; that is **not** the same thing as the numbered “Project status” rows in the root `README.md` (roadmap of shipped capabilities).
3. **`CONTEXT.md`** per bounded context — domain vocabulary when those files exist (see [`CONTEXT-MAP.md`](../CONTEXT-MAP.md)); glossary content is added over time, not duplicated here.
4. **`docs/adr/`** — system-wide architecture decisions that are hard to reverse.

If this file or the root `README.md` disagrees with `openspec/` or `AGENTS.md`, treat the latter as authoritative and fix the human-facing doc.

## Clone, restore, test

Prerequisites: [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) and (optionally) a free Sepolia RPC key from [Infura](https://infura.io/) or [Alchemy](https://www.alchemy.com/).

```bash
git clone https://github.com/YOUR_USERNAME/sovereign-id-openspec.git
cd sovereign-id-openspec

dotnet restore SovereignID.sln
dotnet test SovereignID.sln

dotnet run --project src/legacy/SovereignID.Demo.Phase1
```

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

Architecture rules: `openspec/specs/solution-architecture/spec.md`.

## Auth demo (Phase 2)

Set a 32-byte signing key and run the Auth API:

```powershell
$env:AUTH_JWT_SIGNING_KEY="replace-with-a-random-32-byte-secret"
dotnet run --project src/bc-auth/SovereignID.Auth.Api
```

Open [http://localhost:5000/](http://localhost:5000/) (or the URL from `dotnet run`) and use **Sign in with Ethereum**. MetaMask signs the SIWE payload; the page shows the issued JWT and signer address.

## Local configuration (optional)

`appsettings.Development.json` is gitignored. Copy the example and fill non-secret defaults if you prefer files over environment variables:

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

## Integration tests (Sepolia)

```bash
dotnet test SovereignID.sln --filter "Category=Integration"
```

Tests are skipped when required variables are missing so CI stays green without secrets.

## Tests with coverage

```bash
dotnet test SovereignID.sln --filter "Category!=Integration" --settings coverlet.runsettings --results-directory coverage
```

Coverage outputs under `coverage/**`:

- `coverage.opencover.xml` (SonarQube)
- `coverage.cobertura.xml` (CI artifact and per-BC validation)

`coverlet.runsettings` enforces a local 70% floor (`line`, `branch`, `method`).

### Integration test convention

`[Trait("Category","Integration")]` is only for tests that need real external I/O (live Sepolia RPC, deployed contracts, external DB, or filesystem outside test output).

`SovereignID.Auth.IntegrationTests` uses in-process `WebApplicationFactory<Program>` without external I/O; those tests intentionally omit the `Integration` trait and run in default CI coverage.

## CI and SonarQube

`.github/workflows/ci.yml` (`ubuntu-latest`): restore, build, test with coverage (`Category!=Integration`), per-BC coverage via `ci/Check-BcCoverage.ps1`, uploads artifact **`coverage`**, then publishes commit status **`SovereignID / CI`** on the **PR head** (or on the pushed commit for `push` to `main`) so required checks match the tip of the branch. Fork PRs skip that step (same limitation as Sonar).

`.github/workflows/sonarqube.yml` (`ubuntu-latest`, same OS as **`CI`**): runs **after** a successful **`CI`** run via `workflow_run`. It downloads the **`coverage`** artifact from that CI run, then runs `dotnet-sonarscanner begin` with OpenCover paths, `sonar.exclusions` / `sonar.coverage.exclusions`, and `sonar.qualitygate.wait=true`, followed by `dotnet build` and `scanner end`. A final step publishes a **commit status** `SovereignID / SonarQube` on the **PR branch tip** (`workflow_run.pull_requests[0].head.sha` when the parent run is a `pull_request`; otherwise `head_sha`) so branch protection can require it. Do **not** rely on **`SonarQube / Build and analyze`** alone for merge gating on PRs: checks from `workflow_run` often do not bind to the PR head the same way as `CI / build-test`.

### Block merges when Sonar fails (branch protection)

1. Repository **Settings** → **Branches** (or **Rules** / rulesets) → rule for **`main`**.
2. Enable **Require status checks to pass before merging**.
3. Add required checks (use **+ Add checks** after a green run — do **not** type ad‑hoc names like `ci /`):
   - **`SovereignID / CI`** (commit status from `ci.yml`; reliable gate on the PR head).
   - **`SovereignID / SonarQube`** (commit status from `sonarqube.yml`; reliable Sonar gate on PRs).

Until both complete successfully on the PR’s latest commit, GitHub will not allow merging (subject to your other rule options). **`CI / build-test`** and **`SonarQube / Build and analyze`** can stay as non‑required (informational) if the UI shows duplicate or “Expected” rows that do not merge with the Actions check.

**Fork PRs:** Sonar is skipped when `head_repository` is not this repo; **`SovereignID / CI`** is not posted when the PR head is on a fork. Required checks on those contexts block fork merges unless you bypass or change the rules. Prefer branches on the upstream repo for contributor flows that must pass both gates.

### SonarCloud quality gate (`Sonar way`, free plan)

On SonarCloud **Free**, **custom quality gates** (clone/edit) are not available; the org uses the built-in **`Sonar way`** gate. That gate still enforces strong rules on **new code**, including **test coverage** (typically **≥ 80 %** on new code — see your org’s **Conditions on New Code** list).

**What you should verify once:**

1. Organization **`skiiinnny`** → **Administration** → **Quality Gates** → open **`Sonar way`** and read the **Coverage** (new code) condition.
2. Project **`Skiiinnny_SovereingID`** → **Project settings** → **Quality Gate** → confirm the project uses **`Sonar way`** (default), not a custom gate blocked by plan limits.

**Overall / per-BC 70 %:** enforced in **GitHub Actions** via `ci/Check-BcCoverage.ps1` and locally via `coverlet.runsettings`; SonarCloud Free does **not** add a second “overall coverage ≥ 70 %” gate condition.

Optional: attach a screenshot of the project’s quality gate assignment or `Sonar way` conditions to the verification PR (e.g. inside `<details>`).

Requires **`SONAR_TOKEN`** (SonarCloud **user** token, not a project token) in repository secrets.

**PR checklist:** target `main`; confirm **`SovereignID / CI`** and **`SovereignID / SonarQube`** are green on the PR head; confirm SonarCloud shows the branch without auth errors.

## Deploying `Notary.sol`

Compile `contracts/Notary.sol` in [Remix](https://remix.ethereum.org/) (or your toolchain), deploy to Sepolia, set `NOTARY_CONTRACT_ADDRESS`. Fund the wallet from a Sepolia faucet (e.g. [sepoliafaucet.com](https://www.sepoliafaucet.com/)) before sending transactions.

## OpenSpec layout

This project uses [OpenSpec](https://github.com/Fission-AI/OpenSpec). Specs, designs, and task lists live under `openspec/`. Start with `openspec/config.yaml` and `openspec/specs/`.

## Agent-specific docs

- Issues / `gh` CLI: [`docs/agents/issue-tracker.md`](agents/issue-tracker.md)
- Triage labels: [`docs/agents/triage-labels.md`](agents/triage-labels.md)
- Domain documentation consumption: [`docs/agents/domain.md`](agents/domain.md)

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

## Standards (external)

- [EIP-4361](https://eips.ethereum.org/EIPS/eip-4361) — Sign-In with Ethereum
- [W3C DID Core](https://www.w3.org/TR/did-core/)
- [W3C VC Data Model](https://www.w3.org/TR/vc-data-model/)
