## Purpose

Define and enforce the foundational Onion/Clean architecture constraints for
SovereignID, including layer boundaries, dependency direction, bounded-context
isolation, shared-kernel scope, and architecture-test enforcement.

## Requirements

### Requirement: Layered project structure per bounded context

The solution SHALL organize production code into the following layered
projects, with one set per bounded context (`auth`, `issuer`, `verifier`):

- `SovereignID.<BC>.Domain` — entities, value objects, domain services,
  ports.
- `SovereignID.<BC>.Application` — commands, queries, handlers,
  application services.
- `SovereignID.<BC>.Infrastructure` — adapters implementing Domain and
  Application ports.
- `SovereignID.<BC>.Api` — HTTP/composition entry point (added by each
  BC's own change, not by this foundation change).

In addition, the solution SHALL contain three shared-kernel projects:

- `SovereignID.SharedKernel.Domain` — cross-context value objects and
  ports.
- `SovereignID.SharedKernel.Application` — CQRS abstractions
  (`ICommand<T>`, `IQuery<T>`, handler interfaces).
- `SovereignID.SharedKernel.Infrastructure` — Nethereum-based adapters
  reused by every BC's Infrastructure layer.

#### Scenario: All required projects exist after foundation change

- **WHEN** the solution `SovereignID.sln` is loaded after this change is
  applied
- **THEN** it contains exactly these production projects (in addition to
  any legacy projects under `src/legacy/`):
  `SovereignID.SharedKernel.Domain`,
  `SovereignID.SharedKernel.Application`,
  `SovereignID.SharedKernel.Infrastructure`,
  `SovereignID.Auth.Domain`, `SovereignID.Auth.Application`,
  `SovereignID.Auth.Infrastructure`,
  `SovereignID.Issuer.Domain`, `SovereignID.Issuer.Application`,
  `SovereignID.Issuer.Infrastructure`,
  `SovereignID.Verifier.Domain`, `SovereignID.Verifier.Application`,
  `SovereignID.Verifier.Infrastructure`

#### Scenario: Folder layout matches solution layout

- **WHEN** an inspector lists the `src/` directory
- **THEN** it finds exactly the folders `shared/`, `bc-auth/`,
  `bc-issuer/`, `bc-verifier/`, `legacy/`
- **AND** every production project from the previous scenario lives under
  exactly one of those folders

### Requirement: Dependency direction (inward only)

Each project SHALL depend only on projects belonging to inner layers, per
the following allowed-edges table. Any other project reference MUST cause
the architecture-tests build to fail.

| From                                     | Allowed references                                                                          |
|------------------------------------------|---------------------------------------------------------------------------------------------|
| `SharedKernel.Domain`                    | (none)                                                                                      |
| `SharedKernel.Application`               | `SharedKernel.Domain`                                                                       |
| `SharedKernel.Infrastructure`            | `SharedKernel.Domain`, `SharedKernel.Application`                                           |
| `<BC>.Domain`                            | `SharedKernel.Domain`                                                                       |
| `<BC>.Application`                       | `<BC>.Domain`, `SharedKernel.Application` (transitively `SharedKernel.Domain`)              |
| `<BC>.Infrastructure`                    | `<BC>.Domain`, `<BC>.Application`, `SharedKernel.Infrastructure`                            |
| `<BC>.Api` (added by per-BC change)      | `<BC>.Application`, `<BC>.Infrastructure`                                                   |

#### Scenario: Domain has no outward references

- **WHEN** the architecture tests inspect `SovereignID.SharedKernel.Domain`
  or any `SovereignID.<BC>.Domain` assembly
- **THEN** it depends on no other `SovereignID.*` assembly except
  (for BC domains) `SovereignID.SharedKernel.Domain`

#### Scenario: Application does not reference Infrastructure

- **WHEN** the architecture tests inspect any `SovereignID.<BC>.Application`
  or `SovereignID.SharedKernel.Application` assembly
- **THEN** it does not reference any `*.Infrastructure` assembly

#### Scenario: Disallowed reference fails the build

- **GIVEN** a developer adds a `<ProjectReference>` from
  `SovereignID.Auth.Domain` to `SovereignID.Auth.Infrastructure`
- **WHEN** `dotnet test SovereignID.sln` runs
- **THEN** the architecture-tests project fails with a message identifying
  the offending edge and the rule it violates

### Requirement: Bounded-context isolation

Projects belonging to one bounded context MUST NOT reference projects
belonging to another. Inter-BC communication, when introduced in future
changes, SHALL happen exclusively through transport mechanisms exposed by
each BC's `*.Api` (HTTP, message bus) — never through project references.

#### Scenario: Cross-BC reference is forbidden

- **GIVEN** any project named `SovereignID.Auth.*`
- **WHEN** the architecture tests inspect its references
- **THEN** none of them match `SovereignID.Issuer.*` or
  `SovereignID.Verifier.*`
- **AND** the same property holds for any `SovereignID.Issuer.*` or
  `SovereignID.Verifier.*` project mutatis mutandis

#### Scenario: Each BC.Api is independently publishable

- **WHEN** any future `SovereignID.<BC>.Api` is published with
  `dotnet publish`
- **THEN** the published output contains no assembly belonging to a
  different bounded context

### Requirement: Shared kernel scope is minimal

`SovereignID.SharedKernel.Domain` SHALL contain only value objects and
ports that are genuinely cross-context and behaviorally stable. It SHALL
NOT contain entities, aggregates, or domain concepts whose meaning differs
between bounded contexts (notably: `VerifiableCredential`,
`VerifiablePresentation`, `Claim`, `Nonce`, `RevocationStatus`).

The initial allowed contents are:

- Value objects: `DecentralizedIdentifier`, `EthereumAddress`,
  `Sha256Hash`, `PublicKey`, `Signature`.
- Ports: `ISigner`, `ISignatureVerifier`, `IHasher`, `IBlockchainAnchor`,
  `IBlockchainQuery`, `IClock`, `IGuidGenerator`.

Any future addition to `SharedKernel.Domain` MUST be justified in a
dedicated change's `design.md`.

#### Scenario: BC-specific concepts are not in the kernel

- **WHEN** an inspector lists public types in
  `SovereignID.SharedKernel.Domain`
- **THEN** no type name contains `Credential`, `Presentation`, `Nonce`,
  or `Revocation`

#### Scenario: Kernel grows only via documented change

- **GIVEN** a change adds a new public type to `SharedKernel.Domain`
- **WHEN** the change is reviewed
- **THEN** its `design.md` contains an explicit decision justifying why
  the type belongs to the shared kernel rather than to a specific
  bounded context

### Requirement: Application layer uses lightweight CQRS abstractions

`SovereignID.SharedKernel.Application` SHALL define the marker and handler
interfaces used by every BC's Application layer:

- `ICommand<TResult>` — marker for state-changing intents.
- `IQuery<TResult>` — marker for read-only intents.
- `ICommandHandler<TCommand, TResult>` and
  `IQueryHandler<TQuery, TResult>` — async handler contracts that take a
  `CancellationToken`.

Each BC SHALL express its use cases as command/query records implementing
these markers, and as handler classes implementing the corresponding
handler interface. No external mediator dependency (e.g., MediatR) is
permitted for this convention.

#### Scenario: Handler contracts are async with cancellation

- **WHEN** an inspector reads the signatures of `ICommandHandler<,>` and
  `IQueryHandler<,>`
- **THEN** their `HandleAsync` method returns `Task<TResult>` and accepts
  a `CancellationToken`

#### Scenario: No external mediator package referenced

- **WHEN** the architecture tests inspect any `SovereignID.*` project
  outside `legacy/`
- **THEN** none of them reference a NuGet package whose id starts with
  `MediatR`

### Requirement: Phase 1 code isolated as legacy

Phase 1 projects MUST remain isolated in legacy paths.

All Phase 1 production projects (`SovereignID.Crypto`,
`SovereignID.Chain`, `SovereignID.Demo.Phase1`) SHALL live under
`src/legacy/` and their tests under `tests/legacy/`. Their contents and
public API MUST remain unchanged by this foundation change. No project
outside `src/legacy/` or `tests/legacy/` MAY reference any project under
`src/legacy/`.

#### Scenario: Legacy projects relocated without modification

- **WHEN** the change is applied
- **THEN** `src/legacy/SovereignID.Crypto/`,
  `src/legacy/SovereignID.Chain/` and
  `src/legacy/SovereignID.Demo.Phase1/` exist
- **AND** the public types they expose (`KeyPair`, `MessageSigner`,
  `SignatureVerifier`, `HashHelper`, `SepoliaClient`,
  `DocumentNotarizer`) retain their previous signatures byte-for-byte

#### Scenario: Phase 1 tests remain green

- **WHEN** `dotnet test SovereignID.sln` runs after the change
- **THEN** every test that existed before the change still passes
- **AND** no Phase 1 test project references any new (non-legacy)
  production project

#### Scenario: New code cannot consume legacy

- **GIVEN** any non-legacy project (e.g., `SovereignID.Auth.Domain`)
- **WHEN** the architecture tests inspect its references
- **THEN** none of them point to a project under `src/legacy/`

### Requirement: Nethereum is confined to infrastructure layers

Nethereum dependencies MUST be restricted to infrastructure or legacy code.

The Nethereum NuGet packages (`Nethereum.Signer`, `Nethereum.Web3`,
`Nethereum.Contracts`, etc.) MAY only be referenced by projects ending in
`.Infrastructure` or by projects under `src/legacy/`. Domain and
Application layers MUST remain free of any Nethereum reference, direct or
transitive (other than through their allowed Infrastructure dependencies).

#### Scenario: Domain layer has no Nethereum reference

- **WHEN** the architecture tests inspect any `*.Domain` assembly outside
  `legacy/`
- **THEN** it has no direct or transitive reference to a NuGet package
  whose id starts with `Nethereum.`

#### Scenario: Application layer has no Nethereum reference

- **WHEN** the architecture tests inspect any `*.Application` assembly
  outside `legacy/`
- **THEN** it has no direct or transitive reference to a NuGet package
  whose id starts with `Nethereum.`

### Requirement: Architecture rules are enforced by automated tests

A test project `SovereignID.Architecture.Tests` SHALL exist under
`tests/architecture/` and SHALL run as part of `dotnet test
SovereignID.sln`. It MUST encode every rule defined in this spec
(dependency direction, BC isolation, kernel scope, legacy isolation,
Nethereum confinement, no-mediator policy) as automated assertions using
`NetArchTest.Rules`. The test project MUST discover production
assemblies dynamically so that newly added `SovereignID.*` projects are
covered automatically.

#### Scenario: Tests run on standard test command

- **WHEN** `dotnet test SovereignID.sln` runs
- **THEN** the architecture-tests project is included in the test run
- **AND** all its assertions pass

#### Scenario: Newly added project is automatically covered

- **GIVEN** a developer adds a new project `SovereignID.X.Domain` to
  the solution
- **WHEN** `dotnet test SovereignID.sln` runs
- **THEN** the architecture-tests project loads the new assembly without
  any code change to the test project itself
- **AND** evaluates the same rules against it

#### Scenario: Failed rule produces actionable message

- **WHEN** any architecture rule fails
- **THEN** the failure message names the offending assembly, the rule
  that was violated, and the dependency edge that caused the violation
