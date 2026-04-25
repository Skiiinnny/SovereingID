# Tasks: Phase 2 — SIWE Authentication

> SovereignID · openspec/changes/phase-2-siwe-auth/tasks.md
> Estimated: 3 weeks · 5-10 hrs/week

---

## Week 1 — Domain + Application

### 1.1 Pre-flight
- [x] Confirm `dotnet test SovereignID.sln` is green on `main` (baseline).
- [ ] Create working branch `feature/phase-2-siwe-auth` from `main`.
- [x] Verify architecture tests still pass (Phase 2 starts from a clean slate).

### 1.2 Auth.Domain — value objects
- [x] Implement `Nonce` as `sealed record` with `Create(string)` factory;
  validate 32 lowercase hex chars.
- [x] Unit test: `Nonce.Create` accepts valid 32-hex strings.
- [x] Unit test: `Nonce.Create` throws on length ≠ 32 and on non-hex chars.
- [x] Implement `ChainId` as `sealed record` with `Create(int)`; expose
  static `ChainId.Sepolia` (= 11155111); reject any other value with a
  typed domain error.
- [x] Unit test: `ChainId.Create(11155111)` returns `Sepolia`.
- [x] Unit test: `ChainId.Create(1)` throws `UnsupportedChain` domain error.
- [x] Implement `SiweMessage` record with every field listed in design §D3
  plus `OriginalPayload`.

### 1.3 Auth.Domain — errors
- [x] Create `AuthErrors` static class with typed error codes:
  `NonceUnknown`, `NonceConsumed`, `NonceExpired`, `UnsupportedChain`,
  `SignatureMismatch`, `SiweParseFailed`.
- [x] Create `Result<TValue, TError>` helper (or use the one already
  provided by `SharedKernel.Application` if it exists — otherwise
  local to `Auth.Domain`).

### 1.4 Auth.Domain — aggregate
- [x] Implement `AuthChallenge` with static `Issue(...)` factory and
  `Consume(DateTimeOffset now)` behavioral method.
- [x] `Consume` enforces: not already consumed → else `NonceConsumed`;
  `now < ExpiresAt` → else `NonceExpired`.
- [x] Unit test: fresh challenge → `Consume` returns success and flips
  `IsConsumed`.
- [x] Unit test: second `Consume` on the same challenge → `NonceConsumed`.
- [x] Unit test: `Consume` past expiration → `NonceExpired`.
- [x] Unit test: factory uses `IClock.GetUtcNowAsync` and honors TTL
  parameter (deterministic test via fake clock).

### 1.5 Auth.Domain — ports
- [x] Define `IAuthChallengeRepository` (`SaveAsync`, `FindByNonceAsync`,
  `DeleteAsync`); all async with `CancellationToken`.
- [x] Define `INonceGenerator` (`Task<Nonce> NewAsync(CancellationToken)`).
- [x] Define `ISiweMessageParser` (`Task<SiweMessage> ParseAsync(string payload, CancellationToken)`).
- [x] Define `ISiweSignatureVerifier` (`Task<EthereumAddress> RecoverAddressAsync(string message, Signature signature, CancellationToken)`).
- [x] Define `IJwtTokenIssuer` (`Task<JwtToken> IssueAsync(EthereumAddress subject, TimeSpan lifetime, CancellationToken)`).
- [x] Every port has XML doc comments on its interface and each method.

### 1.6 Auth.Application — `GenerateNonceQuery`
- [x] Create records: `GenerateNonceQuery : IQuery<GenerateNonceResult>` and
  `GenerateNonceResult(string Nonce, DateTimeOffset ExpiresAt)`.
- [x] Implement `GenerateNonceQueryHandler`: call `AuthChallenge.Issue(...)`
  with `INonceGenerator`, `IClock`, TTL = 10 min; save via
  `IAuthChallengeRepository`; return result.
- [x] Unit test: handler returns a fresh nonce and the repository
  receives exactly one `SaveAsync`.
- [x] Unit test: returned `ExpiresAt` = `IClock.GetUtcNowAsync()` + 10 min.

### 1.7 Auth.Application — `VerifySiweCommand`
- [x] Create records: `VerifySiweCommand(string Message, string Signature) : ICommand<VerifySiweResult>`
  and `VerifySiweResult(string Jwt, string Address, DateTimeOffset ExpiresAt)`.
- [x] Implement `VerifySiweCommandHandler` orchestrating:
  1. `ISiweMessageParser.ParseAsync` → `SiweMessage`.
  2. `IAuthChallengeRepository.FindByNonceAsync(siwe.Nonce)` → `AuthChallenge` or `NonceUnknown`.
  3. `AuthChallenge.Consume(IClock.Now)` → success or typed error.
  4. `ISiweSignatureVerifier.RecoverAddressAsync(siwe.OriginalPayload, signature)` → `EthereumAddress`.
  5. Compare recovered address with `siwe.Address` (EIP-55 normalized) → else `SignatureMismatch`.
  6. `IAuthChallengeRepository.DeleteAsync(siwe.Nonce)` (single-use).
  7. `IJwtTokenIssuer.IssueAsync(recovered, lifetime: 24h)` → return result.
- [x] Every handler step is `await`-ed; no blocking calls.
- [x] Unit test (happy path): all ports return happy values → result
  contains expected JWT + address.
- [x] Unit test: `ParseAsync` throws → handler maps to `SiweParseFailed`.
- [x] Unit test: repository returns null → `NonceUnknown`.
- [x] Unit test: stale challenge → `NonceExpired`.
- [x] Unit test: recovered address ≠ claimed address → `SignatureMismatch`.
- [x] Unit test: on success, `DeleteAsync` is called exactly once with the
  same nonce.

### 1.8 Week 1 deliverable
- [x] `dotnet test` is green for `Auth.Domain.Tests` and `Auth.Application.Tests`.
- [x] Zero Nethereum references in `Auth.Domain` or `Auth.Application`
  (verified by existing architecture tests).
- [ ] Commit checkpoint: "feat(auth): domain + application for SIWE flow".

---

## Week 2 — Infrastructure

### 2.1 Auth.Infrastructure — project plumbing
- [x] Add NuGet packages to `SovereignID.Auth.Infrastructure.csproj`:
  `Nethereum.Signer` (4.26.x), `Nethereum.Util` (4.26.x),
  `System.IdentityModel.Tokens.Jwt` (8.x), `Microsoft.Extensions.Logging.Abstractions`.
- [x] Do NOT add any package to `Auth.Domain` or `Auth.Application`.

### 2.2 InMemoryAuthChallengeRepository
- [x] Implement `InMemoryAuthChallengeRepository : IAuthChallengeRepository`
  backed by `ConcurrentDictionary<Nonce, AuthChallenge>`.
- [ ] Register as `Singleton` in DI (done in `Auth.Api` wiring later).
- [x] Implement periodic eviction using `PeriodicTimer` (1-min tick);
  evict challenges whose `ExpiresAt` ≤ current `IClock` time.
- [ ] Eviction loop is started via `IHostedService` registered alongside
  the repository so tests can opt out when needed.
- [x] Unit test: `SaveAsync` + `FindByNonceAsync` round-trip.
- [x] Unit test: `DeleteAsync` makes `FindByNonceAsync` return null.

### 2.3 SecureRandomNonceGenerator
- [x] Implement `SecureRandomNonceGenerator : INonceGenerator` using
  `RandomNumberGenerator.GetBytes(16)` converted to lowercase hex.
- [x] Unit test: two consecutive calls produce distinct nonces (probabilistic,
  run 1000 iterations).
- [x] Unit test: output always matches `Nonce.Create` constraints.

### 2.4 ManualSiweMessageParser
- [x] Implement `ManualSiweMessageParser : ISiweMessageParser` with
  line-by-line grammar per design §D3.
- [x] Unit test: parse the canonical EIP-4361 example from the spec;
  every field matches expectations.
- [x] Unit test: missing `Chain ID` line → `SiweParseFailed` with line number.
- [x] Unit test: malformed address (not 0x + 40 hex) → `SiweParseFailed`.
- [x] Unit test: `Version` ≠ 1 → `SiweParseFailed`.
- [x] Unit test: optional `Resources` block with 0 / 1 / 2 entries parses correctly.
- [x] Unit test: unknown chain id (42161) → parser returns `SiweMessage`,
  but `AuthChallenge` / Application rejects via `UnsupportedChain`
  (keeps parser policy-free).

### 2.5 NethereumSiweSignatureVerifier
- [x] Implement `NethereumSiweSignatureVerifier : ISiweSignatureVerifier`
  using `Nethereum.Signer.EthereumMessageSigner.EncodeUTF8AndEcRecover`.
- [x] Recovered address is normalized via `Nethereum.Util.AddressUtil`
  (EIP-55 checksum).
- [x] Unit test: sign a known message with a known private key
  (generated in-test, never committed) → recovered address equals the
  key's address.
- [x] Unit test: tampered message → recovered address differs.
- [x] Unit test: malformed signature (wrong length / not hex) → throws
  with a meaningful message (wrapped so callers see a consistent
  exception type).

### 2.6 JwtBearerTokenIssuer
- [x] Implement `JwtBearerTokenIssuer : IJwtTokenIssuer` using HMAC-SHA256
  and `JwtSecurityTokenHandler`.
- [x] Read signing key from `IOptions<AuthOptions>`; key itself comes
  from environment variable `AUTH_JWT_SIGNING_KEY` (validated at
  startup; missing → fail fast with a clear error).
- [x] Claims: `sub` (lowercase EIP-55 address), `iss`, `aud`, `iat`, `exp`,
  custom `address` claim, custom `did` claim
  (`did:ethr:sepolia:{lowercase-address}`).
- [x] Unit test: issued JWT parses back with the same claims.
- [x] Unit test: issued JWT `exp` = `IClock.Now` + lifetime.
- [x] Unit test: signing key validation fails fast when missing / too short
  (< 32 bytes).

### 2.7 Week 2 deliverable
- [x] Every adapter has unit-test coverage ≥ 1 happy + 1 negative case.
- [x] `dotnet test` is green across `Auth.*.Tests` projects.
- [x] Architecture tests confirm Nethereum is only referenced by
  `Auth.Infrastructure` (and legacy).
- [ ] Commit checkpoint: "feat(auth): infrastructure adapters + JWT issuer".

---

## Week 3 — API, Frontend, Integration Tests

### 3.1 Create `SovereignID.Auth.Api`
- [ ] Create folder `src/bc-auth/SovereignID.Auth.Api/`.
- [ ] `SovereignID.Auth.Api.csproj` targets `net9.0`, `<Sdk>` =
  `Microsoft.NET.Sdk.Web`, `ImplicitUsings` + `Nullable` enabled.
- [ ] Project references: `SovereignID.Auth.Application`,
  `SovereignID.Auth.Infrastructure`. NO reference to any other BC.
- [ ] Add NuGet: `Microsoft.AspNetCore.Authentication.JwtBearer` (9.0.*).
- [ ] Add `AuthOptions` record bound to configuration section `"Auth"`
  with fields: `JwtSigningKey`, `JwtIssuer`, `JwtAudience`,
  `NonceTtlSeconds` (default 600).
- [ ] `Program.cs`:
  - Register all Auth handlers, ports, and adapters in DI.
  - Register `InMemoryAuthChallengeRepository` as `Singleton`.
  - Register `IClock` + `IGuidGenerator` adapters (system clock / `Guid.NewGuid`).
  - Register the hosted eviction service for the repository.
  - `AddAuthentication().AddJwtBearer(...)` using the same options as
    the issuer so Phase 3 services can validate tokens without rewiring.
  - `app.UseDefaultFiles(); app.UseStaticFiles();` for the frontend.
  - Map endpoints defined in `AuthEndpoints`.
  - End file with `public partial class Program;` for
    `WebApplicationFactory<Program>` support.

### 3.2 Endpoints
- [ ] `AuthEndpoints.MapAuth(WebApplication app)` registers:
  - `app.MapGet("/auth/nonce", ...)` resolving
    `IQueryHandler<GenerateNonceQuery, GenerateNonceResult>`,
    returning `200 NonceResponse`.
  - `app.MapPost("/auth/verify", async (VerifyRequest body, ...) => ...)`
    resolving `ICommandHandler<VerifySiweCommand, VerifySiweResult>`
    and mapping domain errors to HTTP status codes per design §"Public
    HTTP Contract".
- [ ] Both endpoints accept `CancellationToken` and forward it to handlers.
- [ ] Error mapping is centralized (single helper `ToProblem(AuthError)`).
- [ ] XML doc comments on the public `AuthEndpoints.MapAuth` method and
  on the request/response records.

### 3.3 Configuration & secrets hygiene
- [ ] `appsettings.json` contains non-secret defaults only
  (`JwtIssuer`, `JwtAudience`, `NonceTtlSeconds`), and `JwtSigningKey`
  as an empty string.
- [ ] `appsettings.Development.example.json` shipped in the repo with a
  placeholder signing key and a note instructing to copy it to
  `appsettings.Development.json` (gitignored) — matches the pattern
  already used by the Phase 1 demo.
- [ ] `Program.cs` fails fast if `AUTH_JWT_SIGNING_KEY` is missing in
  non-Development environments.
- [ ] Confirm `.gitignore` excludes `appsettings.Development.json` (it
  already does for the legacy demo; verify for the new project too).

### 3.4 Frontend (wwwroot)
- [ ] Create `wwwroot/index.html` with a header, a "Sign in with
  Ethereum" button, a `<pre>` status area, and a `<pre>` "signed
  message" area.
- [ ] Create `wwwroot/app.js` implementing the six-step flow from
  design §D9. No external JS dependencies. Use `window.ethereum` only.
- [ ] The SIWE message built on the client matches the grammar
  `ManualSiweMessageParser` expects (include `Issued At` in ISO-8601
  UTC, omit optional fields for simplicity).
- [ ] Defensive checks: `window.ethereum` missing → show installation
  hint; user rejects signature → render error without a stack trace.
- [ ] Display the raw SIWE payload before sending (transparency for the
  user; reinforces the teaching value).

### 3.5 Integration tests
- [ ] Create `tests/bc-auth/SovereignID.Auth.IntegrationTests/` xUnit
  project targeting `net9.0`.
- [ ] NuGet: `Microsoft.AspNetCore.Mvc.Testing` (9.0.*), `xunit`, `xunit.runner.visualstudio`,
  `FluentAssertions`.
- [ ] Project references: `SovereignID.Auth.Api` (under test),
  `SovereignID.Auth.Domain`, `SovereignID.Auth.Application`,
  `SovereignID.Auth.Infrastructure`, and (test-only) the legacy
  `SovereignID.Crypto` project for keypair generation.
- [ ] Implement `TestClock : IClock` — mutable `DateTimeOffset` with
  `Advance(TimeSpan)`.
- [ ] Implement `DeterministicNonceGenerator : INonceGenerator` for
  reproducibility.
- [ ] Custom `WebApplicationFactory<Program>` overrides `IClock`,
  `INonceGenerator`, and injects a test `AUTH_JWT_SIGNING_KEY` via
  `builder.UseSetting` or environment variable per the test run.
- [ ] Test 1 — happy path: nonce → sign → verify → 200; JWT `sub` =
  signer address; response contains the same address.
- [ ] Test 2 — replay: second `POST /auth/verify` with identical payload
  → 401 `nonce_consumed`.
- [ ] Test 3 — expired: advance `TestClock` past TTL → 401 `nonce_expired`.
- [ ] Test 4 — wrong chain id: build SIWE message with `Chain ID: 1` →
  400 `unsupported_chain`.
- [ ] Test 5 — tampered message: sign one payload, submit a different
  one with the same signature → 401 `signature_mismatch`.
- [ ] Test 6 — unknown nonce: submit a SIWE message whose nonce was
  never issued → 401 `nonce_unknown`.
- [ ] Test 7 — malformed payload: missing `Version` line → 400
  `siwe_parse_failed`, `detail` mentions the failing line.

### 3.6 Solution + build integration
- [ ] Add all four new projects to `SovereignID.sln` under matching
  solution folders (`bc-auth`, `tests/bc-auth`).
- [ ] Run `dotnet build SovereignID.sln`. Zero warnings in Phase 2 code.
- [ ] Run `dotnet test SovereignID.sln --filter "Category!=Integration"`.
  All unit + integration (WebApplicationFactory) tests pass without
  any network access.
- [ ] Run `dotnet test SovereignID.sln` (including all architecture
  tests). All green. Confirm no new architecture rule violations.

### 3.7 Manual demo walkthrough (local)
- [ ] Set `AUTH_JWT_SIGNING_KEY` to a 32-byte random secret in the
  local shell.
- [ ] `dotnet run --project src/bc-auth/SovereignID.Auth.Api`.
- [ ] Open `http://localhost:<port>/` in a browser with MetaMask (any
  testnet or even mainnet account works — no gas is spent).
- [ ] Click "Sign in with Ethereum" → MetaMask opens → confirm → page
  shows JWT, recovered address, and expiry.
- [ ] Decode the JWT at [jwt.io](https://jwt.io) and confirm the
  claims match the signer's address.

### 3.8 Documentation touch-up
- [ ] Update `README.md`:
  - Add "Phase 2" entry to the status table (Done/Demo).
  - Add a "Run the Auth demo" section pointing to the API project and
    the `AUTH_JWT_SIGNING_KEY` env var.
- [ ] Do NOT modify `AGENTS.md` (conventions unchanged).
- [ ] Update `AGENTS.md` Active Phase pointer ONLY after tasks are
  archived (per experimental workflow rules); out of scope for this
  implementation change.

### 3.9 Week 3 deliverable
- [ ] Browser demo renders a JWT after a real MetaMask signature.
- [ ] All seven integration tests pass in-process.
- [ ] All architecture tests still pass.
- [ ] Commit checkpoint: "feat(auth): Auth.Api + frontend + integration tests".
- [ ] Open PR "feat(auth): Phase 2 — SIWE authentication (endpoints,
  JWT, demo page)".

---

## Definition of Done

- [ ] `GET /auth/nonce` returns a fresh nonce with a 10-minute expiry.
- [ ] `POST /auth/verify` accepts a SIWE payload + signature and returns
  a signed JWT on success, or a typed error code on failure.
- [ ] Nonce is single-use: the second `POST /auth/verify` with the same
  payload fails with `nonce_consumed`.
- [ ] Chain ID other than `11155111` is rejected with `unsupported_chain`.
- [ ] All Auth code follows `AGENTS.md` conventions: async everywhere,
  records for immutable data, interfaces for external dependencies,
  XML doc comments on public APIs, no secrets in source.
- [ ] No Nethereum reference in `Auth.Domain` or `Auth.Application`
  (enforced by architecture tests).
- [ ] Manual SIWE parser passes the canonical EIP-4361 example plus the
  negative cases listed in 2.4.
- [ ] Integration tests run in-process via `WebApplicationFactory<Program>`
  with zero network calls, zero secrets, and a deterministic clock.
- [ ] Browser demo (vanilla JS + MetaMask) successfully completes the
  full flow against a locally running API.
- [ ] `dotnet test SovereignID.sln` is green end-to-end.
