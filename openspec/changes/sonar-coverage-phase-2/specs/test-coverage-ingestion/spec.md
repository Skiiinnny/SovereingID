## ADDED Requirements

### Requirement: Test coverage reports are produced for every test project

Every xUnit test project under `tests/` SHALL emit a coverage report in both OpenCover and Cobertura formats on each `dotnet test` invocation that uses the project's standard settings.

The configuration SHALL live in a single `coverlet.runsettings` file at
the repository root, and every test project SHALL pick up
`coverlet.collector` automatically through a shared
`Directory.Build.props` under `tests/`, so that no per-project
`PackageReference` to `coverlet.collector` is required.

The runsettings file SHALL set `Format` to `opencover,cobertura`,
`DeterministicReport` to `false`, and `SingleHit` to `false`.

`DeterministicReport` MUST stay `false` whenever `Format` includes
`opencover`: Coverlet's OpenCover reporter throws
`NotSupportedException` for deterministic coverage results. Cobertura
still emits normally; Sonar continues to consume
`coverage.opencover.xml` via `sonar.cs.opencover.reportsPaths`.

#### Scenario: A single `dotnet test` command produces both formats for every test project

- **WHEN** a developer runs `dotnet test SovereignID.sln --filter "Category!=Integration" --settings coverlet.runsettings --results-directory coverage` from the repository root
- **THEN** the `coverage/` directory contains at least one
  `coverage.opencover.xml` file and one `coverage.cobertura.xml` file
  per executed test project
- **AND** no test project needs an explicit
  `<PackageReference Include="coverlet.collector" ... />` in its
  `.csproj` because the package is provided by `tests/Directory.Build.props`

#### Scenario: Future test projects inherit the coverage setup

- **WHEN** a new xUnit test project is added anywhere under `tests/`
- **THEN** running `dotnet test` with the root `coverlet.runsettings`
  produces OpenCover and Cobertura reports for the new project
  without any additional configuration in the new `.csproj`

### Requirement: Coverage exclusions are documented and applied in three layers

The change SHALL apply coverage exclusions at three orthogonal layers,
each one addressing a distinct concern. Each exclusion SHALL be
traceable to a written reason in `design.md`.

Layer 1 — `coverlet.runsettings` `Exclude`, `ExcludeByFile`, and
`ExcludeByAttribute`:

- `Exclude` SHALL filter out the legacy assemblies
  (`SovereignID.Crypto*`, `SovereignID.Chain*`,
  `SovereignID.Demo.Phase1*`) and types matching the `*Marker` suffix.
- `ExcludeByAttribute` SHALL include
  `ExcludeFromCodeCoverageAttribute`, `GeneratedCodeAttribute`,
  `CompilerGeneratedAttribute`, and `Obsolete`.
- `ExcludeByFile` SHALL exclude `**/Program.cs`, `**/*.g.cs`,
  `**/obj/**`, `**/bin/**`.

Layer 2 — Source-level `[ExcludeFromCodeCoverage]` attribute:

- Applied at type level on hosting/composition-root code
  (`Program.cs` of `SovereignID.Auth.Api`) and on trivial
  infrastructure adapters (`SystemClock`, `GuidGenerator`),
  configuration records (`AuthOptions` in `Auth.Api` and in
  `Auth.Infrastructure`), HTTP DTO records in `Auth.Api/Contracts/`,
  every `*Marker` type, and `CqrsAbstractions.cs`.

Layer 3 — SonarQube scanner properties passed at `scanner begin`:

- `sonar.exclusions` SHALL include
  `src/bc-auth/SovereignID.Auth.Api/wwwroot/**` (frontend JS
  fully out of analysis) and `src/legacy/**` (frozen).
- `sonar.coverage.exclusions` SHALL include `src/legacy/**`,
  `**/*Marker.cs`, and `**/Program.cs` as defensive redundancy for
  coverage-only exclusion.

#### Scenario: Legacy code never contributes to coverage

- **WHEN** a coverage report is generated and consumed by any
  downstream step (per-BC check, Sonar scanner, artifact consumer)
- **THEN** no file under `src/legacy/` appears in the numerator or
  denominator of the overall or per-BC line-coverage calculation

#### Scenario: Frontend JavaScript stays out of Sonar analysis

- **WHEN** the Sonar scanner runs with the project configuration of
  this change
- **THEN** no file under
  `src/bc-auth/SovereignID.Auth.Api/wwwroot/` appears as an analyzed
  file in the SonarCloud project
- **AND** no JavaScript-specific issues are raised against those
  files

#### Scenario: Hosting and DTOs are excluded via attribute

- **WHEN** an analyzer or Sonar inspects the source files of
  `SovereignID.Auth.Api`
- **THEN** `Program.cs`, `Infrastructure/SystemClock.cs`,
  `Infrastructure/GuidGenerator.cs`, `Configuration/AuthOptions.cs`,
  and every record under `Contracts/` carry the
  `[ExcludeFromCodeCoverage]` attribute at the type level
- **AND** those files do not appear with non-zero
  `linesToCover` in the coverage report produced by Coverlet

### Requirement: SonarCloud consumes OpenCover reports and enforces a custom Quality Gate

The `SovereignID` SonarCloud project (`Skiiinnny_SovereingID`) SHALL be analyzed with a scanner configuration that:

- receives the OpenCover coverage reports via
  `sonar.cs.opencover.reportsPaths` pointing to every
  `coverage.opencover.xml` emitted by `dotnet test`, and
- waits for the Quality Gate result via
  `sonar.qualitygate.wait=true`, blocking the pull-request check
  if the gate fails.

A custom Quality Gate named `SovereignID default` SHALL be
maintained in SonarCloud, cloned from `Sonar way`, with the
following conditions in addition to all `Sonar way` defaults
(ratings A, new duplications ≤ 3 %, new hotspots reviewed
100 %):

- `new_coverage` ≥ 80 % (inherited; kept explicit).
- `coverage` ≥ 70 % on overall project code.

The `SovereignID default` gate SHALL be assigned to the
`Skiiinnny_SovereingID` project.

#### Scenario: Sonar reports non-zero coverage after this change is applied

- **WHEN** the Sonar workflow finishes on the merge commit of this
  change
- **THEN** the `Skiiinnny_SovereingID` project shows a non-zero
  `coverage` metric in SonarCloud
- **AND** the `tests` metric reflects the number of unit tests
  executed

#### Scenario: A failing Quality Gate blocks the pull request

- **WHEN** a pull request introduces code that drops
  `new_coverage` below 80 % or `coverage` below 70 %
- **THEN** the Sonar workflow job finishes with a non-zero exit
  code
- **AND** the GitHub required check `SonarQube / Build and
  analyze` shows a failure on the pull request
- **AND** the Sonar project page attributes the failure to the
  specific failing condition

#### Scenario: The `SovereignID default` gate is the one applied

- **WHEN** an inspector opens the `Skiiinnny_SovereingID` project
  settings in SonarCloud
- **THEN** the assigned Quality Gate is `SovereignID default`
- **AND** the gate's condition list contains both
  `new_coverage` ≥ 80 % and `coverage` ≥ 70 %

### Requirement: Per-bounded-context coverage floor is enforced in CI

Each non-legacy bounded context (`bc-auth`, `bc-issuer`, `bc-verifier`, and the `shared` kernel) SHALL maintain a line coverage of at least 70 %, measured from the Cobertura report produced in CI, independent of the project-wide average enforced by SonarCloud.

The enforcement SHALL happen in the CI pipeline via a PowerShell
script at `ci/Check-BcCoverage.ps1`, invoked by the `ci.yml`
workflow after `dotnet test` and before uploading the coverage
artifact. The script SHALL:

- group classes by bounded-context prefix
  (`SovereignID.Auth.*` → `bc-auth`,
  `SovereignID.Issuer.*` → `bc-issuer`,
  `SovereignID.Verifier.*` → `bc-verifier`,
  `SovereignID.SharedKernel.*` → `shared`),
- ignore `SovereignID.Crypto.*`, `SovereignID.Chain.*`,
  and `SovereignID.Demo.Phase1` (legacy),
- compute line coverage per bounded context,
- exit with a non-zero status and a human-readable summary if any
  bounded context with covered lines > 0 is below 70 %,
- exit zero (with a warning line) if a bounded context has zero
  covered lines and zero coverable lines, so that empty future BCs
  do not break CI.

#### Scenario: A BC below the floor blocks CI

- **WHEN** `dotnet test` produces a Cobertura report in which
  `bc-auth` shows 65 % line coverage
- **THEN** `ci/Check-BcCoverage.ps1` exits with a non-zero status
- **AND** the `build-test` job fails
- **AND** the `sonar-scan` job does not run (GitHub Actions
  `needs:` chain)

#### Scenario: An empty BC does not block CI

- **WHEN** `dotnet test` produces a Cobertura report that contains
  no classes with prefix `SovereignID.Issuer.*` (because
  `bc-issuer` has no production code yet)
- **THEN** `ci/Check-BcCoverage.ps1` exits with status zero
- **AND** the job log contains a warning line identifying
  `bc-issuer` as "no coverable lines" rather than a failure

#### Scenario: Per-BC thresholds are configurable without touching the spec

- **WHEN** a maintainer wants to change the per-BC floor from 70
  to 75
- **THEN** the change is expressed by editing a single constant or
  variable at the top of `ci/Check-BcCoverage.ps1`
- **AND** no YAML workflow file or spec change is required

### Requirement: The CI pipeline is consolidated into two workflows

The repository SHALL provide exactly two GitHub Actions workflows
related to build, test, and analysis for the .NET code base:

- `ci.yml` on `ubuntu-latest`, responsible for restore, build,
  test (with the `Category!=Integration` filter and the root
  `coverlet.runsettings`), per-BC coverage enforcement, and
  uploading the coverage reports as an artifact.
- `sonarqube.yml` on `windows-latest`, responsible for
  downloading the coverage artifact and running the SonarScanner
  (`begin` → `build` → `end`) with the properties required by
  this spec, including `sonar.qualitygate.wait=true`.

The workflow file `.github/workflows/dotnet.yml` SHALL be
removed from the repository as redundant with `ci.yml`.

Triggers SHALL be aligned: both workflows activate on `push` to
`main` and on `pull_request` against `main`
(`opened`, `synchronize`, `reopened`).

#### Scenario: No workflow duplicates build or tests

- **WHEN** an inspector lists `.github/workflows/` after this
  change is applied
- **THEN** the directory contains `ci.yml`, `sonarqube.yml`, and
  no file named `dotnet.yml`
- **AND** no workflow file besides `ci.yml` invokes
  `dotnet test` for the `SovereignID.sln` solution in the main
  build path

#### Scenario: Sonar reuses the CI coverage artifact

- **WHEN** `sonarqube.yml` executes on a pull request
- **THEN** it downloads the coverage artifact produced by the
  corresponding `ci.yml` run
- **AND** it passes
  `sonar.cs.opencover.reportsPaths=<path to coverage>/**/coverage.opencover.xml`
  to the scanner `begin` step
- **AND** it does not invoke `dotnet test` itself

#### Scenario: Sonar workflow waits for the gate verdict

- **WHEN** the scanner `begin` step runs
- **THEN** the command line includes `/d:sonar.qualitygate.wait=true`
- **AND** the `scanner end` step returns the Quality Gate status as
  its own exit code (zero only if the gate passed), after analysis
  upload and gate polling configured during `begin`

### Requirement: The `Integration` test category is reserved for external I/O

A test method or class in this repository SHALL carry the
`[Trait("Category", "Integration")]` attribute **if and only if**
its execution requires real external I/O: a live Sepolia JSON-RPC
endpoint, a deployed on-chain contract, an external database, or
filesystem locations beyond the test's own output directory.

Tests that exercise the HTTP pipeline of `SovereignID.Auth.Api`
via `WebApplicationFactory<Program>` with in-process test doubles
(e.g. `TestClock`, `DeterministicNonceGenerator`) SHALL NOT carry
the `Integration` trait, even if the containing project is named
`SovereignID.Auth.IntegrationTests`. The project name is retained
for historical reasons and documented in the README note.

Tests marked `[Integration]` SHALL be skipped in the default CI
run via the filter `Category!=Integration`.

Tests not marked `[Integration]` SHALL run in every CI
invocation and SHALL contribute to the Coverlet-produced
coverage reports.

#### Scenario: Auth WebApplicationFactory tests run in CI

- **WHEN** the `ci.yml` workflow runs `dotnet test` with the
  filter `Category!=Integration`
- **THEN** all seven `AuthEndpointsTests` methods execute
- **AND** they contribute to the coverage of
  `SovereignID.Auth.Api`, `SovereignID.Auth.Infrastructure`,
  `SovereignID.Auth.Application`, and `SovereignID.Auth.Domain`

#### Scenario: Sepolia RPC tests are skipped in CI

- **WHEN** the `ci.yml` workflow runs `dotnet test` without the
  `SEPOLIA_RPC_URL` environment variable
- **THEN** `SepoliaClientIntegrationTests` and
  `DocumentNotarizerIntegrationTests` are skipped via
  `[SkippableFact]` plus the `Category!=Integration` filter
- **AND** the CI run reports success

#### Scenario: README documents the convention

- **WHEN** a new contributor opens `README.md`
- **THEN** there is a note stating that the
  `Integration` trait tracks external I/O only
- **AND** the note clarifies that the project name
  `SovereignID.Auth.IntegrationTests` refers to the
  WebApplicationFactory style of integration, not to the
  `[Integration]` trait

### Requirement: Local developers can enforce the same coverage floor before pushing

A developer SHALL be able to reproduce the CI coverage floor locally
with a single command, using the same `coverlet.runsettings` file,
without any additional tooling beyond the .NET 9 SDK.

The `coverlet.runsettings` file SHALL therefore include a local
threshold that approximates the per-BC floor:
`Threshold` = 70, `ThresholdType` = `line,branch,method`,
`ThresholdStat` = `total`.

#### Scenario: Local test run warns when coverage is below the threshold

- **WHEN** a developer runs
  `dotnet test SovereignID.sln --filter "Category!=Integration" --settings coverlet.runsettings`
  against a worktree where the test suite produces less than
  70 % line, branch, or method coverage for a test project
- **THEN** the `dotnet test` command finishes with a non-zero
  exit code produced by Coverlet's threshold assertion
- **AND** the error message identifies the failing metric and
  the actual value

#### Scenario: README documents the local command

- **WHEN** a contributor looks for how to run tests with coverage
- **THEN** `README.md` contains a "Run tests with coverage"
  section that documents the command above and explains the
  location of the generated reports under `coverage/`
