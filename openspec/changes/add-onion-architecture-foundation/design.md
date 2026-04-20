# Design: Add Onion Architecture Foundation

> SovereignID · openspec/changes/add-onion-architecture-foundation/design.md

## Context

Phase 1 produced three projects (`SovereignID.Crypto`, `SovereignID.Chain`,
`SovereignID.Demo.Phase1`) sufficient for a console demo. Phases 2–5 require
three independently deployable services (Auth, Issuer, Verifier), persistence
and external integrations. The current code mixes domain concepts with
Nethereum infrastructure, which would force costly rework if carried into
those services.

This change introduces the structural foundation — Onion / Clean Architecture
combined with a strangler-fig migration — so future phases can grow inside
clear boundaries without paying refactor tax later. It establishes:

- A small **Shared Kernel** with cross-context primitives and ports.
- One layered stack per **Bounded Context** (`Auth`, `Issuer`, `Verifier`),
  with `Domain → Application → Infrastructure → Api` direction enforced.
- A **legacy** zone that freezes Phase 1 code untouched until the new stack
  replaces it organically.
- A **lightweight CQRS** convention for the Application layer (no external
  mediator dependency).
- **Architecture tests** that fail the build on dependency violations.

Stakeholders: the developer (sole author, Senior .NET, learning blockchain).
Constraints from `AGENTS.md`: async everywhere, records for immutable data,
interfaces for external dependencies, no secrets in source, every public
method tested.

### Current state (relevant)

```
src/
├── SovereignID.Crypto/         (Phase 1, depends on Nethereum.Signer)
├── SovereignID.Chain/          (Phase 1, depends on Nethereum.Web3 + Crypto)
└── SovereignID.Demo.Phase1/    (Phase 1 console demo)
tests/
├── SovereignID.Crypto.Tests/
└── SovereignID.Chain.Tests/
```

### Target state (after this change)

```
src/
├── shared/
│   ├── SovereignID.SharedKernel.Domain/
│   └── SovereignID.SharedKernel.Application/      (CQRS abstractions)
│   └── SovereignID.SharedKernel.Infrastructure/   (Nethereum adapters)
├── bc-auth/
│   ├── SovereignID.Auth.Domain/
│   ├── SovereignID.Auth.Application/
│   └── SovereignID.Auth.Infrastructure/
├── bc-issuer/
│   ├── SovereignID.Issuer.Domain/
│   ├── SovereignID.Issuer.Application/
│   └── SovereignID.Issuer.Infrastructure/
├── bc-verifier/
│   ├── SovereignID.Verifier.Domain/
│   ├── SovereignID.Verifier.Application/
│   └── SovereignID.Verifier.Infrastructure/
└── legacy/
    ├── SovereignID.Crypto/                       (frozen)
    ├── SovereignID.Chain/                        (frozen)
    └── SovereignID.Demo.Phase1/                  (frozen)
tests/
├── shared/SovereignID.SharedKernel.Domain.Tests/
├── shared/SovereignID.SharedKernel.Infrastructure.Tests/
├── architecture/SovereignID.Architecture.Tests/  (NetArchTest)
└── legacy/                                       (Phase 1 tests, unchanged)
```

The three `*.Api` projects are intentionally **not** created here; each
arrives with its BC-specific change so this foundation stays purely
structural.

## Goals / Non-Goals

**Goals:**

- Establish a layered structure where dependency direction is enforced by
  build (architecture tests), not just by convention.
- Guarantee bounded-context isolation from day 0 so each `*.Api` can be
  packaged and deployed independently.
- Confine all Nethereum usage to `SharedKernel.Infrastructure`; no other
  project (except `legacy/`) references Nethereum directly.
- Define a single Application-layer convention (lightweight hand-rolled
  CQRS) so all future BCs look the same.
- Migrate Phase 1 by isolation, not rewrite: legacy code keeps working and
  keeps its tests green; nothing in the new structure depends on it.
- Keep the existing `dotnet test SovereignID.sln` command working end-to-end.

**Non-Goals:**

- No business logic in any new project. Domain/Application/Infrastructure
  layers ship with the minimum types needed for the architecture tests to
  pass and the solution to compile (typically one or two empty marker
  files per project).
- No `.Api` projects in this change.
- No SIWE, VC issuance or verification logic.
- No removal or rewrite of legacy code. Legacy is frozen, not deleted.
- No persistence layer (no EF Core, no repositories, no DB).
- No CI pipeline changes beyond what `dotnet test` already exercises.
- No documentation rewrite of `README.md` beyond a short pointer to the
  new structure.

## Decisions

### D1 — Domain reading: identity as the core, crypto as infrastructure

**Decision:** The Domain models identity concepts (DID, VC, VP, Claim, Proof,
Issuer, Holder, Verifier, RevocationStatus). Cryptographic primitives are
ports in `SharedKernel.Domain` (`ISigner`, `ISignatureVerifier`, `IHasher`)
implemented in `SharedKernel.Infrastructure` against Nethereum.

**Rationale:** Allows swapping the crypto engine (e.g., BouncyCastle, HSM)
without touching Issuer/Verifier code. Keeps the Domain free of any external
dependency, which is the primary lever Onion architecture provides.

**Alternatives considered:**

- *Crypto as part of Domain* (rejected): cleaner conceptual fit for SSI
  (the key *is* the identity), but couples Domain to a crypto
  implementation choice. Rejected to preserve testability and
  swappability.

### D2 — One layered stack per Bounded Context, three separate stacks

**Decision:** `Auth`, `Issuer` and `Verifier` each get their own
`Domain / Application / Infrastructure / Api` set of projects under
`src/bc-<name>/`. They never reference each other at the project level.

**Rationale:** The `architecture.md` spec already calls for independent
deployability. Enforcing it through project structure (and architecture
tests) prevents "convenience" cross-references that would break
deployability later.

**Alternatives considered:**

- *Single shared `Domain`, `Application`, `Infrastructure`* (rejected):
  smaller solution but couples the three services into one deployable
  unit, contradicting the stated architecture.
- *Per-BC `.sln` files* (deferred): could be added later if build times
  become a problem; not required now.

### D3 — Shared Kernel scope: minimal and stable

**Decision:** `SharedKernel.Domain` contains only:

- Value objects shared by all BCs and unlikely to drift:
  `DecentralizedIdentifier`, `EthereumAddress`, `Sha256Hash`, `Signature`,
  `PublicKey`.
- Cross-cutting ports: `ISigner`, `ISignatureVerifier`, `IHasher`,
  `IBlockchainAnchor`, `IBlockchainQuery`, `IClock`, `IGuidGenerator`.

It does **not** contain `VerifiableCredential`, `VerifiablePresentation`,
`Claim`, `Nonce` or any concept that each BC views differently. These are
defined inside each `BC.Domain`, even at the cost of nominal duplication.

**Rationale:** A bloated Shared Kernel is the most common failure mode of
this architecture: the moment two BCs depend on the same mutable concept,
they stop being independently deployable. Strict minimalism is cheaper than
breaking the kernel apart later.

**Alternatives considered:**

- *Single `SharedKernel.Identity` with VC/VP types* (rejected): ergonomic
  in the short term, fatal in the medium term.
- *No SharedKernel at all, duplicate value objects per BC* (rejected):
  would force three implementations of `EthereumAddress` parsing, and
  three competing definitions of a `DID`.

### D4 — Application convention: lightweight hand-rolled CQRS

**Decision:** `SharedKernel.Application` defines:

```csharp
public interface ICommand<TResult> { }
public interface IQuery<TResult> { }
public interface ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken ct);
}
public interface IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken ct);
}
```

Each BC's `Application` layer defines its commands/queries as records and
implements handlers. No dispatcher is required at this point: the
`*.Api` layer (added later per BC) injects handlers directly. A minimal
`IDispatcher` may be added in a future change if cross-cutting behaviors
(logging, validation, transactions) become repetitive.

**Rationale:** Gets the CQRS shape and naming for free; avoids the
MediatR licensing change (commercial since 2024) and the larger surface
area of Wolverine/Brighter at this stage. Easy to swap in a real mediator
later — the public types remain.

**Alternatives considered:**

- *Plain Application Services* (rejected): less consistent surface as the
  number of use cases grows; harder to layer cross-cutting behaviors.
- *MediatR* (rejected): licensing risk, external dependency for a pattern
  we can express in ~30 lines.
- *Wolverine* (deferred): excellent option once messaging between BCs
  becomes real; overkill for an empty foundation.

### D5 — Strangler fig: legacy isolation by folder + reference rules

**Decision:** Move `SovereignID.Crypto`, `SovereignID.Chain` and
`SovereignID.Demo.Phase1` into `src/legacy/` without renaming or rewriting.
The solution file is regenerated to reflect the new paths. An architecture
test forbids any non-`legacy/` project from referencing any `legacy/`
project. Their tests move to `tests/legacy/` correspondingly.

**Rationale:** The strangler-fig pattern requires the legacy zone to keep
working untouched while the new structure grows. Folder isolation plus an
enforced reference rule make "accidental coupling" impossible.

**Alternatives considered:**

- *Leave Phase 1 in place at `src/`* (rejected): blurs the boundary; new
  contributors can't visually tell what's frozen.
- *Delete Phase 1 immediately and re-implement in new structure*
  (rejected): violates the strangler-fig principle and risks regressing
  passing tests; no functional benefit at this stage.

### D6 — Architecture tests with NetArchTest.Rules

**Decision:** Add `tests/architecture/SovereignID.Architecture.Tests/`
using `NetArchTest.Rules`. The project asserts at least the rules listed
under D7 below and runs as part of the standard `dotnet test
SovereignID.sln`.

**Rationale:** Documenting reference rules in markdown is not enforcement.
A 1-hour investment up-front prevents weeks of debt later. NetArchTest is
mature, dependency-free at runtime, and works on .NET 9 assemblies.

**Alternatives considered:**

- *ArchUnitNET* (acceptable): more expressive, slightly heavier API. Either
  works; NetArchTest chosen for simplicity.
- *Roslyn analyzers* (rejected): higher implementation cost for the same
  signal.
- *No tests, document rules in design.md only* (rejected): explicitly
  excluded by the user during the proposal.

### D7 — Enforced dependency rules

The architecture tests SHALL fail the build when any of the following are
violated:

1. `SovereignID.SharedKernel.Domain` MUST NOT reference any other
   SovereignID project.
2. `SovereignID.SharedKernel.Application` MUST reference only
   `SovereignID.SharedKernel.Domain`.
3. `SovereignID.<BC>.Domain` MUST reference only
   `SovereignID.SharedKernel.Domain`.
4. `SovereignID.<BC>.Application` MUST reference only
   `SovereignID.<BC>.Domain` and `SovereignID.SharedKernel.Application`
   (which transitively pulls `SharedKernel.Domain`).
5. `SovereignID.<BC>.Infrastructure` MAY reference its own
   `Domain`/`Application` and `SharedKernel.Infrastructure`. It MUST NOT
   reference any other BC.
6. No project under `src/bc-*/` MAY reference any project under another
   `src/bc-*/`.
7. No project outside `src/legacy/` MAY reference any project under
   `src/legacy/`.
8. No project outside `*.Infrastructure` (and `legacy/`) MAY reference
   `Nethereum.*`.

### D8 — Solution layout and folder mapping

**Decision:** Solution folders mirror disk folders. Top-level solution
folders: `shared`, `bc-auth`, `bc-issuer`, `bc-verifier`, `legacy`,
`tests`. Each BC folder appears once on disk and once in the solution to
keep IDE navigation predictable.

**Rationale:** Reduces friction when navigating in Visual Studio or Rider;
matches what architecture tests scan for via path conventions.

## Risks / Trade-offs

- **[Project-count explosion]** The solution jumps from 5 to ~14 projects.
  → Mitigation: the structural cost is paid once; per-phase additions
  thereafter are 0–1 project. IDE solution folders keep navigation sane.
- **[Premature abstraction signal]** Empty Domain/Application projects can
  feel like "interface for one implementation". → Mitigation: this change
  is explicitly preparatory; the next change (`add-auth-bc-siwe`)
  populates `bc-auth/` with real logic and demonstrates the value.
- **[Shared Kernel rot]** Future temptation to push BC-specific types into
  `SharedKernel.Domain` to "share" them. → Mitigation: D3 is documented
  here; any future change touching `SharedKernel.Domain` must justify it
  in its own `design.md`.
- **[Solution file merge conflicts]** Regenerating `SovereignID.sln`
  changes many GUIDs/paths at once. → Mitigation: this is a single
  atomic commit; downstream branches rebase against it once.
- **[Strangler debris]** `legacy/` may live longer than expected if no
  feature naturally subsumes Phase 1's notarization. → Mitigation:
  acceptable; legacy has zero maintenance cost while frozen, and the
  notarization concept will resurface inside `bc-issuer` for revocation
  anchoring.
- **[Async port leakage]** Defining synchronous ports in
  `SharedKernel.Domain` would force rewrites once Nethereum's async API
  is wired in. → Mitigation: all ports are async (`Task<T>`,
  `CancellationToken`) from day 1, per `AGENTS.md`.
- **[Private key exposure surface]** `KeyPair` in legacy still stores
  the private key as a string. → Mitigation: the new `ISigner` port
  takes a `PrivateKeyHandle` opaque type defined in
  `SharedKernel.Infrastructure`; private keys never cross into
  `Application` or `Domain`. Legacy retains its current shape; it is
  frozen and not consumed by new code.
- **[Architecture test false negatives]** NetArchTest evaluates compiled
  assemblies; if a project is missing from the test's scan list it gives
  no signal. → Mitigation: the test discovers projects by loading all
  assemblies matching `SovereignID.*` from the build output, so newly
  added projects are picked up automatically.

## Migration Plan

This change is migration-only (no functional change). Sequencing:

1. Move legacy projects to `src/legacy/` with `git mv` (preserves
   history). Do not edit their contents.
2. Move legacy tests to `tests/legacy/` similarly.
3. Create the new project skeletons (empty `csproj` + a single marker
   file per project so the assembly compiles).
4. Add project references reflecting D7.
5. Regenerate `SovereignID.sln` so it includes new projects, new
   solution folders, and updated relative paths to legacy.
6. Add `SovereignID.Architecture.Tests` with the rules from D7.
7. Run `dotnet build` and `dotnet test SovereignID.sln`. All Phase 1
   tests must remain green; new architecture tests must pass.
8. Commit as a single change.

**Rollback:** revert the commit. Because no legacy code is modified, the
revert restores the original Phase 1 layout exactly.

## Open Questions

- **OQ1**: Should `SharedKernel.Application` define a base
  `Result<T>` / `Error` type now, or wait for the first BC to need it?
  Default plan: wait. Add it in `add-auth-bc-siwe` if SIWE error handling
  motivates it.
- **OQ2**: Do we want a separate `SovereignID.SharedKernel.Contracts`
  project for DTOs that `*.Api` projects expose to clients (and that
  external SDKs could reference)? Default plan: defer until a real API
  exists; revisit when designing `add-auth-bc-siwe`.
- **OQ3**: Should the architecture-tests project also assert *naming*
  conventions (e.g., command class names end with `Command`)? Default
  plan: only enforce dependency rules in this change; naming
  conventions can be added later without schema impact.
