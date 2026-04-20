# Tasks: Add Onion Architecture Foundation

> SovereignID · openspec/changes/add-onion-architecture-foundation/tasks.md

## 1. Pre-flight

- [ ] 1.1 Confirm `dotnet test SovereignID.sln` is green on `main` before starting (baseline).
- [ ] 1.2 Create working branch `feature/onion-architecture-foundation` from `main`.
- [ ] 1.3 Verify `git status` is clean — no uncommitted Phase 1 work in progress.

## 2. Isolate legacy projects

- [ ] 2.1 `git mv src/SovereignID.Crypto src/legacy/SovereignID.Crypto`.
- [ ] 2.2 `git mv src/SovereignID.Chain src/legacy/SovereignID.Chain`.
- [ ] 2.3 `git mv src/SovereignID.Demo.Phase1 src/legacy/SovereignID.Demo.Phase1`.
- [ ] 2.4 `git mv tests/SovereignID.Crypto.Tests tests/legacy/SovereignID.Crypto.Tests`.
- [ ] 2.5 `git mv tests/SovereignID.Chain.Tests tests/legacy/SovereignID.Chain.Tests`.
- [ ] 2.6 Update `<ProjectReference>` paths inside the moved `.csproj` files (e.g., `SovereignID.Chain.csproj` referencing `..\SovereignID.Crypto\SovereignID.Crypto.csproj` still resolves after the move). Adjust only relative paths; do NOT modify code.
- [ ] 2.7 Update `<ProjectReference>` paths in the legacy test `.csproj` files similarly.
- [ ] 2.8 Build the legacy projects in isolation (`dotnet build src/legacy/SovereignID.Crypto/SovereignID.Crypto.csproj` etc.) to confirm path edits are correct.

## 3. Regenerate solution file

- [ ] 3.1 Remove existing project entries from `SovereignID.sln` for the moved Phase 1 projects (`dotnet sln SovereignID.sln remove ...` for each).
- [ ] 3.2 Re-add the legacy projects from their new paths under solution folder `legacy` (`dotnet sln SovereignID.sln add --solution-folder legacy ...`).
- [ ] 3.3 Re-add the legacy test projects under solution folder `tests/legacy` similarly.
- [ ] 3.4 Run `dotnet build SovereignID.sln` and `dotnet test SovereignID.sln`. All Phase 1 tests MUST still pass.
- [ ] 3.5 Commit checkpoint: "chore(legacy): move Phase 1 projects under src/legacy and tests/legacy".

## 4. Create SharedKernel.Domain

- [ ] 4.1 Create folder `src/shared/SovereignID.SharedKernel.Domain/`.
- [ ] 4.2 Create `SovereignID.SharedKernel.Domain.csproj` targeting `net9.0` with `ImplicitUsings=enable`, `Nullable=enable`, and NO package references.
- [ ] 4.3 Add port interface stubs (signatures only, no implementations): `ISigner`, `ISignatureVerifier`, `IHasher`, `IBlockchainAnchor`, `IBlockchainQuery`, `IClock`, `IGuidGenerator`. All async methods MUST return `Task<T>` and accept `CancellationToken`.
- [ ] 4.4 Add value-object stubs (records with private constructors and static factory): `DecentralizedIdentifier`, `EthereumAddress`, `Sha256Hash`, `PublicKey`, `Signature`. Stubs are minimal — just enough to compile and to be referenced by tests later.
- [ ] 4.5 `dotnet sln SovereignID.sln add --solution-folder shared src/shared/SovereignID.SharedKernel.Domain/SovereignID.SharedKernel.Domain.csproj`.

## 5. Create SharedKernel.Application

- [ ] 5.1 Create folder `src/shared/SovereignID.SharedKernel.Application/`.
- [ ] 5.2 Create `SovereignID.SharedKernel.Application.csproj` targeting `net9.0` with `ImplicitUsings=enable`, `Nullable=enable` and a `<ProjectReference>` to `SovereignID.SharedKernel.Domain`.
- [ ] 5.3 Add CQRS abstractions: `ICommand<TResult>`, `IQuery<TResult>`, `ICommandHandler<TCommand, TResult>`, `IQueryHandler<TQuery, TResult>`. Handler methods are `Task<TResult> HandleAsync(TIn input, CancellationToken ct)`.
- [ ] 5.4 Add the project to the solution under solution folder `shared`.

## 6. Create SharedKernel.Infrastructure

- [ ] 6.1 Create folder `src/shared/SovereignID.SharedKernel.Infrastructure/`.
- [ ] 6.2 Create `SovereignID.SharedKernel.Infrastructure.csproj` targeting `net9.0` with package references to `Nethereum.Signer` (4.26.0) and `Nethereum.Web3` (4.26.0), and project references to `SovereignID.SharedKernel.Domain` and `SovereignID.SharedKernel.Application`.
- [ ] 6.3 Add a single placeholder marker class `KernelInfrastructureMarker` (empty, public, used by `Assembly.GetAssembly`-style discovery in tests) so the project compiles and is discoverable.
- [ ] 6.4 Do NOT yet implement any adapter (concrete `NethereumSigner`, etc.). Adapters arrive with the first BC change.
- [ ] 6.5 Add the project to the solution under solution folder `shared`.

## 7. Create per-BC layered scaffolding (Auth)

- [ ] 7.1 Create `src/bc-auth/SovereignID.Auth.Domain/` with empty `csproj` (net9.0, nullable, implicit usings) and one marker class `AuthDomainMarker`.
- [ ] 7.2 Add project reference: `SovereignID.Auth.Domain` → `SovereignID.SharedKernel.Domain`.
- [ ] 7.3 Create `src/bc-auth/SovereignID.Auth.Application/` similarly with marker `AuthApplicationMarker`.
- [ ] 7.4 Add project references: `SovereignID.Auth.Application` → `SovereignID.Auth.Domain`, `SovereignID.SharedKernel.Application`.
- [ ] 7.5 Create `src/bc-auth/SovereignID.Auth.Infrastructure/` similarly with marker `AuthInfrastructureMarker`.
- [ ] 7.6 Add project references: `SovereignID.Auth.Infrastructure` → `SovereignID.Auth.Domain`, `SovereignID.Auth.Application`, `SovereignID.SharedKernel.Infrastructure`.
- [ ] 7.7 Add the three new projects to the solution under solution folder `bc-auth`.

## 8. Create per-BC layered scaffolding (Issuer)

- [ ] 8.1 Repeat steps 7.1–7.2 for `SovereignID.Issuer.Domain` under `src/bc-issuer/`.
- [ ] 8.2 Repeat steps 7.3–7.4 for `SovereignID.Issuer.Application`.
- [ ] 8.3 Repeat steps 7.5–7.6 for `SovereignID.Issuer.Infrastructure`.
- [ ] 8.4 Add the three projects under solution folder `bc-issuer`.

## 9. Create per-BC layered scaffolding (Verifier)

- [ ] 9.1 Repeat steps 7.1–7.2 for `SovereignID.Verifier.Domain` under `src/bc-verifier/`.
- [ ] 9.2 Repeat steps 7.3–7.4 for `SovereignID.Verifier.Application`.
- [ ] 9.3 Repeat steps 7.5–7.6 for `SovereignID.Verifier.Infrastructure`.
- [ ] 9.4 Add the three projects under solution folder `bc-verifier`.

## 10. Build sanity check

- [ ] 10.1 Run `dotnet build SovereignID.sln`. Every project (legacy + shared + 3 BCs) MUST compile.
- [ ] 10.2 Run `dotnet test SovereignID.sln`. All pre-existing Phase 1 tests MUST still pass; no new tests yet.
- [ ] 10.3 Commit checkpoint: "feat(arch): add SharedKernel and per-BC layered scaffolding".

## 11. Architecture tests project

- [ ] 11.1 Create `tests/architecture/SovereignID.Architecture.Tests/` xUnit project targeting `net9.0`.
- [ ] 11.2 Add NuGet package `NetArchTest.Rules` (latest stable for .NET 9).
- [ ] 11.3 Add project references to every production project so their assemblies are loaded into the test AppDomain.
- [ ] 11.4 Implement an assembly discovery helper that loads all `SovereignID.*` assemblies from the test bin folder, excluding `SovereignID.Architecture.Tests` itself.
- [ ] 11.5 Test: `SharedKernel_Domain_Has_No_SovereignID_Dependencies` (rule for `SharedKernel.Domain`).
- [ ] 11.6 Test: `BC_Domain_Only_References_SharedKernel_Domain` (parameterized over `Auth`, `Issuer`, `Verifier`).
- [ ] 11.7 Test: `Application_Layer_Has_No_Reference_To_Infrastructure` (across all BCs and SharedKernel).
- [ ] 11.8 Test: `BCs_Cannot_Reference_Each_Other` (no `Auth.*` references `Issuer.*` or `Verifier.*`, etc.).
- [ ] 11.9 Test: `No_Project_Outside_Legacy_References_Legacy` (scans references; uses path/assembly-name convention).
- [ ] 11.10 Test: `Nethereum_Is_Confined_To_Infrastructure_And_Legacy` (no `Nethereum.*` reference in Domain/Application of any non-legacy project).
- [ ] 11.11 Test: `No_Project_References_MediatR_Package` (asserts no `PackageReference` whose id starts with `MediatR` exists in any non-legacy project; reads `.csproj` XML).
- [ ] 11.12 Test failure messages MUST name the offending assembly, the violated rule, and the dependency edge.
- [ ] 11.13 Add the project to the solution under solution folder `tests/architecture`.
- [ ] 11.14 Run `dotnet test SovereignID.sln`. All architecture tests MUST pass.

## 12. Negative test (manual smoke)

- [ ] 12.1 Temporarily add a forbidden `<ProjectReference>` (e.g., from `SovereignID.Auth.Domain` to `SovereignID.Auth.Infrastructure`) on a throwaway commit.
- [ ] 12.2 Run `dotnet test SovereignID.sln`. Verify the architecture-tests project FAILS with a message identifying the offending edge.
- [ ] 12.3 Revert the throwaway change.
- [ ] 12.4 Re-run `dotnet test SovereignID.sln` and confirm green.

## 13. Documentation touch-up

- [ ] 13.1 Update `README.md` "Repository layout (.NET)" section so it reflects the new `src/shared`, `src/bc-*`, `src/legacy` layout. Keep prose minimal — refer to `openspec/specs/solution-architecture/spec.md` for the rules.
- [ ] 13.2 Add a short note to `README.md` clarifying that `src/legacy/` is frozen and not consumed by new code.
- [ ] 13.3 Do NOT modify `AGENTS.md` in this change (its conventions still hold).

## 14. Final verification

- [ ] 14.1 `dotnet build SovereignID.sln` succeeds with zero warnings for new projects (legacy warnings, if any, are out of scope).
- [ ] 14.2 `dotnet test SovereignID.sln` succeeds; counts include all Phase 1 tests + every architecture test.
- [ ] 14.3 `git diff --stat main...HEAD` shows: project moves under `src/legacy/` and `tests/legacy/`, new `src/shared/`, new `src/bc-*/`, new `tests/architecture/`, regenerated `SovereignID.sln`, README touch-up. NO modification inside any moved Phase 1 source file.
- [ ] 14.4 Open PR titled "feat(arch): add Onion architecture foundation and isolate Phase 1 as legacy".

## Definition of Done

- [ ] All Phase 1 production projects live under `src/legacy/` and their tests under `tests/legacy/`, with unchanged contents.
- [ ] `SovereignID.sln` contains the 9 new BC projects, the 3 SharedKernel projects, and the architecture-tests project, organized into matching solution folders.
- [ ] All ports defined in `SharedKernel.Domain` are async with `CancellationToken`.
- [ ] Architecture-tests project enforces every rule from `specs/solution-architecture/spec.md`.
- [ ] Manual negative test confirmed that a forbidden reference fails the build.
- [ ] `dotnet test SovereignID.sln` is green end-to-end.
- [ ] No code changes inside `src/legacy/` beyond `<ProjectReference>` path adjustments required by the move.
- [ ] No `.Api`, persistence, SIWE, or VC code added.
