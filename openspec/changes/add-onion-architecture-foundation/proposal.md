# Proposal: Add Onion Architecture Foundation

> SovereignID · openspec/changes/add-onion-architecture-foundation/proposal.md

## Why

Phase 1 delivered a working crypto + chain demo, but the code mixes domain
concepts with Nethereum infrastructure (`KeyPair` instantiates `EthECKey`,
`DocumentNotarizer` handles policy + transport in the same method). Phases 2–5
introduce three separately deployable services (Auth / Issuer / Verifier),
persistence, multiple blockchain adapters and external integrations — exactly
the scenario where layered architecture pays off. Doing this refactor *before*
those services exist is an order of magnitude cheaper than after. The change
prepares the codebase for greater scalability, enforces independence of
deployment from day 0, and isolates Phase 1 code as legacy via a strangler-fig
migration so no existing functionality breaks.

## What Changes

- New shared kernel: `SovereignID.SharedKernel.Domain` (DID, EthereumAddress,
  Sha256Hash, Signature value objects; `ISigner`, `ISignatureVerifier`,
  `IHasher`, `IBlockchainAnchor`, `IBlockchainQuery`, `IClock`, `IGuidGenerator`
  ports).
- New shared kernel: `SovereignID.SharedKernel.Infrastructure` (Nethereum
  adapters implementing the kernel ports).
- Empty layered scaffolding for three bounded contexts under `src/bc-auth/`,
  `src/bc-issuer/`, `src/bc-verifier/`. Each BC gets four projects:
  `Domain`, `Application`, `Infrastructure`, `Api`.
- Application convention fixed: lightweight hand-rolled CQRS
  (`ICommand<TResult>` / `IQuery<TResult>` + `IHandler<,>` interfaces in
  `SharedKernel.Application`), no external mediator dependency.
- Architecture tests project (`SovereignID.Architecture.Tests`) using
  `NetArchTest.Rules` that fails the build when dependency rules are violated:
  no Domain → Infrastructure references, no cross-BC references, no references
  to legacy from new code.
- Phase 1 projects move under `src/legacy/` (path-only move, project names and
  contents unchanged) and are explicitly frozen. Solution file is updated to
  reflect the new folder layout. Phase 1 tests stay green.
- Documentation: `openspec/specs/solution-architecture/spec.md` codifies the
  layer rules, BC isolation rules, dependency direction and shared-kernel
  scope as enforceable requirements.

## Capabilities

### New Capabilities
- `solution-architecture`: Codifies the structural rules of the codebase —
  layer boundaries, dependency direction, bounded-context isolation, shared
  kernel scope, legacy isolation strategy and required architecture-test
  enforcement. This is a structural capability whose conformance is verified
  by the architecture-tests build step.

### Modified Capabilities
<!-- None: the existing architecture.md and scenarios.md are project-level
     descriptive specs, not capability specs. They remain untouched. -->

## Impact

- **Code**: 11 new empty project skeletons (2 SharedKernel + 3×3 BC layers
  + 1 architecture-tests project; the 3 `.Api` projects are out of scope for
  *this* change and arrive with their respective BC change). Phase 1 projects
  are physically moved into `src/legacy/` but retain their names and content.
- **Solution file**: `SovereignID.sln` is regenerated so paths match the new
  folder layout; project GUIDs are preserved where possible.
- **Dependencies**: adds `NetArchTest.Rules` to the new architecture-tests
  project. Adds `Nethereum.Signer` and `Nethereum.Web3` to
  `SharedKernel.Infrastructure`. No new dependencies in Domain/Application
  layers.
- **Build / CI**: `dotnet test SovereignID.sln` must continue to pass,
  including the new architecture tests. No behavioral change to Phase 1
  binaries.
- **Future changes**: the next change (`add-auth-bc-siwe`, Phase 2) can
  populate `bc-auth/` with real logic without further structural decisions.
  Same template applies later for Issuer and Verifier.
- **Out of scope**: no business logic in any BC, no `.Api` projects yet, no
  persistence layer, no SIWE / VC code, no removal of legacy projects (only
  isolation), no migration of Phase 1 functionality into the new structure.
