# Design: Phase 2 — SIWE Authentication

> SovereignID · openspec/changes/phase-2-siwe-auth/design.md

## Context

The Onion foundation (`add-onion-architecture-foundation`, archived
2026-04-25) delivered empty layered projects for `bc-auth`. Phase 2
populates those projects with the first real capability: Sign-In with
Ethereum (EIP-4361).

Relevant constraints drawn from `AGENTS.md` and the architecture spec:

- Async everywhere (`Task<T>`, `CancellationToken`), no `.Result`/`.Wait()`.
- Records for immutable data (value objects, DTOs, commands, queries).
- Interfaces (ports) for every external dependency.
- `Nethereum.*` may only be referenced by `*.Infrastructure` projects.
- Shared Kernel stays minimal: concepts specific to authentication
  (`Nonce`, `AuthChallenge`, `SiweMessage`) live inside `Auth.Domain`.
- No secrets in source. JWT signing key is read from an environment
  variable at startup.

Reference scenario: `SC-01 · Decentralized Login (SIWE)` in
`openspec/specs/scenarios.md`.

## Solution Structure

```
src/
└── bc-auth/
    ├── SovereignID.Auth.Domain/
    │   ├── AuthChallenge.cs                 ← aggregate root
    │   ├── Nonce.cs                         ← value object
    │   ├── SiweMessage.cs                   ← value object (parsed form)
    │   ├── ChainId.cs                       ← value object (static Sepolia)
    │   ├── AuthErrors.cs                    ← typed domain errors
    │   ├── Ports/
    │   │   ├── IAuthChallengeRepository.cs
    │   │   ├── INonceGenerator.cs
    │   │   ├── ISiweMessageParser.cs
    │   │   ├── ISiweSignatureVerifier.cs
    │   │   └── IJwtTokenIssuer.cs
    │   └── AuthDomainMarker.cs              ← kept for assembly discovery
    ├── SovereignID.Auth.Application/
    │   ├── Nonce/
    │   │   ├── GenerateNonceQuery.cs
    │   │   ├── GenerateNonceResult.cs
    │   │   └── GenerateNonceQueryHandler.cs
    │   ├── Verify/
    │   │   ├── VerifySiweCommand.cs
    │   │   ├── VerifySiweResult.cs
    │   │   └── VerifySiweCommandHandler.cs
    │   └── AuthApplicationMarker.cs
    ├── SovereignID.Auth.Infrastructure/
    │   ├── Repositories/
    │   │   └── InMemoryAuthChallengeRepository.cs
    │   ├── Nonces/
    │   │   └── SecureRandomNonceGenerator.cs
    │   ├── Siwe/
    │   │   ├── ManualSiweMessageParser.cs
    │   │   └── NethereumSiweSignatureVerifier.cs
    │   ├── Jwt/
    │   │   └── JwtBearerTokenIssuer.cs
    │   └── AuthInfrastructureMarker.cs
    └── SovereignID.Auth.Api/                ← NEW project
        ├── Program.cs                       ← Minimal API host
        ├── Endpoints/
        │   └── AuthEndpoints.cs             ← maps /auth/nonce + /auth/verify
        ├── Contracts/
        │   ├── NonceResponse.cs
        │   ├── VerifyRequest.cs
        │   └── VerifyResponse.cs
        ├── Configuration/
        │   └── AuthOptions.cs               ← JWT key/issuer/audience
        ├── wwwroot/
        │   ├── index.html                   ← vanilla demo UI
        │   └── app.js                       ← MetaMask → sign → verify
        ├── appsettings.json
        └── appsettings.Development.example.json

tests/
└── bc-auth/
    ├── SovereignID.Auth.Domain.Tests/
    │   ├── AuthChallengeTests.cs
    │   └── SiweMessageTests.cs
    ├── SovereignID.Auth.Application.Tests/
    │   ├── GenerateNonceQueryHandlerTests.cs
    │   └── VerifySiweCommandHandlerTests.cs
    └── SovereignID.Auth.IntegrationTests/
        ├── AuthEndpointsTests.cs            ← WebApplicationFactory<Program>
        └── TestClock.cs                     ← IClock fake for deterministic tests
```

## Goals / Non-Goals

**Goals:**

- Deliver `SC-01` end-to-end: browser with MetaMask → signed message →
  JWT returned.
- Prove the Onion architecture with a real use case (handlers depending
  only on domain ports, all Nethereum code confined to `Auth.Infrastructure`).
- Ship a manual, dependency-free SIWE parser to sidestep the .NET 9
  incompatibility of `Nethereum.Templates.Siwe`.
- Keep everything testable without Sepolia: signature verification uses
  real cryptography but the test harness generates its own test keypairs
  via `SovereignID.Crypto` (legacy) or a minimal helper in the
  integration test project.
- Integration tests run in-process via `WebApplicationFactory<Program>`
  with no network, no secrets.

**Non-Goals:**

- No DID resolution in the returned JWT claims. The JWT contains the raw
  `0x…` address; mapping to `did:ethr` is a Phase 3 concern.
- No `/auth/me` endpoint. Only the two endpoints needed to exercise
  `SC-01` ship with this phase.
- No persistent challenge store (no DB, no Redis). The in-memory
  repository is explicit about its MVP scope.
- No multi-chain. `Chain ID = 11155111` (Sepolia) is enforced as a
  domain invariant. Accepting other chains becomes a later design
  decision with its own change.
- No rate-limiting, no IP throttling, no anti-bot. Out of scope for the
  MVP; added later alongside persistence.

## Decisions

### D1 — `AuthChallenge` is a true aggregate, not an anemic DTO

**Decision:** `AuthChallenge` is the consistency boundary for the flow.
It owns the invariants "nonce not expired", "nonce not consumed",
"chain is Sepolia" and exposes a single behavioral method
`Consume(EthereumAddress signer, DateTimeOffset now)` returning a
`Result<Unit, AuthError>`.

```csharp
public sealed class AuthChallenge
{
    public Nonce Nonce { get; }
    public DateTimeOffset IssuedAt { get; }
    public DateTimeOffset ExpiresAt { get; }
    public bool IsConsumed { get; private set; }

    private AuthChallenge(Nonce nonce, DateTimeOffset issuedAt, TimeSpan ttl) { ... }

    public static AuthChallenge Issue(
        INonceGenerator nonces,
        IClock clock,
        TimeSpan ttl,
        CancellationToken ct);

    public Result<Unit, AuthError> Consume(DateTimeOffset now);
}
```

**Rationale:** Keeps the domain rules in the domain. Handlers become
thin orchestrators that call `Consume` and persist the result. Matches
the Onion intent and stays unit-testable without any infrastructure.

**Alternatives considered:**

- *Anemic record + handler logic* (rejected): spreads invariants across
  Application and Domain, defeats the layering.
- *Event-sourced aggregate* (rejected): overkill for a single-use
  challenge with a 10-minute lifetime.

### D2 — `Nonce` is a value object with its own format

**Decision:** `Nonce` is a `sealed record` holding a 128-bit
cryptographically secure value encoded as 32 lowercase hex characters
(no `0x` prefix), created via `Nonce.Create(string)` which validates
length and character set. `SecureRandomNonceGenerator` uses
`RandomNumberGenerator.GetBytes(16)` and produces a `Nonce` directly.

**Rationale:** EIP-4361 recommends an alphanumeric nonce of at least 8
characters. 128 bits of entropy (32 hex chars) is the conservative
industry default and avoids any chance of birthday collisions across
the lifetime of the service. Keeping the format validated at
construction is the classic "parse, don't validate" pattern.

**Alternatives considered:**

- *Base64* (rejected): SIWE verifiers in the wild sometimes reject `/`
  or `+`; lowercase hex is safe everywhere.
- *GUID* (rejected): 122 bits of entropy, string form includes dashes
  that some SIWE parsers have historically mishandled.

### D3 — Manual SIWE parser, no `Nethereum.Templates.Siwe`

**Decision:** `ManualSiweMessageParser : ISiweMessageParser` parses the
canonical EIP-4361 format line-by-line and returns a `SiweMessage`
record containing the typed fields the domain cares about:

```csharp
public sealed record SiweMessage(
    string Domain,
    EthereumAddress Address,
    string Statement,
    Uri Uri,
    int Version,
    ChainId ChainId,
    Nonce Nonce,
    DateTimeOffset IssuedAt,
    DateTimeOffset? ExpirationTime,
    DateTimeOffset? NotBefore,
    string? RequestId,
    IReadOnlyList<Uri> Resources,
    string OriginalPayload);
```

Parsing grammar (one regex per fixed field, no full-blown parser
combinator):

```
Line 1: "{domain} wants you to sign in with your Ethereum account:"
Line 2: "{0xAddress}"
Line 3: ""  (blank)
Line 4: "{statement}" (optional but conventional)
Line 5: ""  (blank)
Line 6: "URI: {uri}"
Line 7: "Version: {1}"
Line 8: "Chain ID: {decimal}"
Line 9: "Nonce: {alphanumeric}"
Line 10: "Issued At: {ISO8601}"
Line 11+: optional "Expiration Time", "Not Before", "Request ID", "Resources:\n- …"
```

Any deviation → `SiweParseError` with a typed reason code and the
failing line number. The parser keeps the original raw payload on the
returned record because that is the exact string that was signed and
must be re-hashed for recovery.

**Rationale:** `Nethereum.Templates.Siwe` is not stable on .NET 9 (it
depends on older `Nethereum.Util` versions that pin to deprecated
BouncyCastle namespaces). Hand-rolling ~120 lines of parsing code is
cheaper than fighting that dependency chain and isolates us from its
release cadence.

**Alternatives considered:**

- *Fork `Nethereum.Templates.Siwe`* (rejected): ongoing maintenance cost
  for a file we can replace in an afternoon.
- *Regex over the whole message* (rejected): brittle; multi-line
  matchers with optional fields become unreadable. Line-by-line with
  typed errors is friendlier for debugging.

### D4 — Signature verification via Nethereum, port in `Auth.Domain`

**Decision:** `ISiweSignatureVerifier` lives in `Auth.Domain`:

```csharp
public interface ISiweSignatureVerifier
{
    Task<EthereumAddress> RecoverAddressAsync(
        string message,
        Signature signature,
        CancellationToken cancellationToken);
}
```

The adapter `NethereumSiweSignatureVerifier` (in `Auth.Infrastructure`)
wraps `Nethereum.Signer.EthereumMessageSigner.EncodeUTF8AndEcRecover`,
normalizes the recovered address with EIP-55 checksum via
`Nethereum.Util.AddressUtil`, and returns an `EthereumAddress` value
object.

Comparison with the claimed `Address` from the SIWE payload uses
case-insensitive equality after both sides are EIP-55 normalized.

**Rationale:** Recovery from `personal_sign` is SIWE-specific at this
stage and not needed by Issuer/Verifier — so the port lives in
`Auth.Domain`, not in `SharedKernel.Domain`. If a future BC needs the
same operation, it graduates to the shared kernel with a documented
justification (per §D3 of the solution-architecture spec).

**Alternatives considered:**

- *Reuse the existing `ISignatureVerifier` in `SharedKernel.Domain`*
  (rejected): its shape requires a known `PublicKey`, whereas SIWE
  recovery derives the address directly and never materializes a public
  key.
- *Add the port to `SharedKernel.Domain`* (rejected for now): violates
  the minimalism rule; revisit when `bc-issuer` or `bc-verifier` proves
  a second consumer.

### D5 — `Chain ID` is hard-coded Sepolia as a domain invariant

**Decision:** `ChainId.Sepolia` is a singleton with value `11155111`.
`SiweMessage` construction rejects any other chain id via
`ChainId.Create(int value)` → `AuthErrors.UnsupportedChain`.

**Rationale:** Accepting any chain id would let an attacker replay a
mainnet signature against the Sepolia backend (the user's intent was
different). Rather than adding a configurable allow-list we bake the
current policy into the domain and make a future multi-chain move
explicit via its own change.

**Alternatives considered:**

- *Configurable allow-list (`AUTH_ALLOWED_CHAIN_IDS`)* (rejected for
  now): adds operational knobs we can't justify without a use case.

### D6 — Repository port is domain-owned, adapter is in-memory

**Decision:** `IAuthChallengeRepository` (in `Auth.Domain`) defines:

```csharp
public interface IAuthChallengeRepository
{
    Task SaveAsync(AuthChallenge challenge, CancellationToken ct);
    Task<AuthChallenge?> FindByNonceAsync(Nonce nonce, CancellationToken ct);
    Task DeleteAsync(Nonce nonce, CancellationToken ct);
}
```

`InMemoryAuthChallengeRepository` stores entries in a
`ConcurrentDictionary<Nonce, AuthChallenge>` and launches a lightweight
`Task` via `PeriodicTimer` to evict expired entries every minute. It
is registered as `Singleton` so state survives across requests.

**Rationale:** For a single-instance MVP this is sufficient and makes
the demo self-contained. The port shape is already the right one for a
future `SqlAuthChallengeRepository` or `RedisAuthChallengeRepository`.

**Alternatives considered:**

- *`IMemoryCache`* (rejected): ties the port to a specific Microsoft
  abstraction and obscures the eviction semantics.
- *LiteDB file-backed* (rejected): adds a dependency for zero MVP
  value.

### D7 — Two use cases, two handlers, no dispatcher

**Decision:** `Application` layer defines exactly two handlers,
registered by concrete type in DI:

- `GenerateNonceQueryHandler : IQueryHandler<GenerateNonceQuery, GenerateNonceResult>`
- `VerifySiweCommandHandler : ICommandHandler<VerifySiweCommand, VerifySiweResult>`

`Auth.Api` endpoints resolve the handlers directly from the DI
container, per §D4 of the solution-architecture spec (no external
mediator).

```csharp
public sealed record GenerateNonceQuery() : IQuery<GenerateNonceResult>;
public sealed record GenerateNonceResult(string Nonce, DateTimeOffset ExpiresAt);

public sealed record VerifySiweCommand(string Message, string Signature)
    : ICommand<VerifySiweResult>;
public sealed record VerifySiweResult(string Jwt, string Address, DateTimeOffset ExpiresAt);
```

**Rationale:** Two use cases is below the threshold that justifies a
dispatcher; per-handler DI is clearer and keeps the dependency graph
visible to anyone reading `Program.cs`.

### D8 — JWT issuance is an Auth-owned port

**Decision:** `IJwtTokenIssuer` in `Auth.Domain`:

```csharp
public interface IJwtTokenIssuer
{
    Task<JwtToken> IssueAsync(
        EthereumAddress subject,
        TimeSpan lifetime,
        CancellationToken cancellationToken);
}

public sealed record JwtToken(string Value, DateTimeOffset ExpiresAt);
```

`JwtBearerTokenIssuer` (in `Auth.Infrastructure`) signs with HMAC-SHA256
using a key read from `AUTH_JWT_SIGNING_KEY`. Claims:

- `sub` = lowercase EIP-55 address
- `iss` = `AUTH_JWT_ISSUER` (default `sovereignid-auth`)
- `aud` = `AUTH_JWT_AUDIENCE` (default `sovereignid-clients`)
- `iat` / `exp` from `IClock`
- `address` (custom claim, redundant with `sub` but keeps downstream
  services explicit about what they're consuming)

Token lifetime: 24 hours (matches `SC-01`).

**Rationale:** Keeping the issuer behind a port lets Phase 3 (VC
issuance) swap to EIP-712-signed tokens without changing the
Application layer.

**Alternatives considered:**

- *RSA / ECDSA signing* (deferred): HMAC is sufficient for a
  single-service MVP. RSA becomes valuable once the token is consumed
  by independent services.

### D9 — Frontend: one HTML file, one JS file, no framework

**Decision:** `wwwroot/index.html` contains a minimal layout with a
"Sign in with Ethereum" button and a status area. `wwwroot/app.js`
encapsulates the flow:

```
1. click → fetch GET /auth/nonce          → { nonce, expiresAt }
2. window.ethereum.request({method:'eth_requestAccounts'})
3. build SIWE message client-side (use `${location.host}` as domain)
4. window.ethereum.request({ method:'personal_sign', params:[msg,acct]})
5. POST /auth/verify  body: { message, signature }
6. render { jwt, address, expiresAt }
```

Static files served by the same host (`app.UseDefaultFiles(); app.UseStaticFiles();`)
so the `domain` in the SIWE message matches the page origin (MetaMask's
phishing protection depends on this).

**Rationale:** A framework-free demo is a better teaching aid than any
Blazor/React sample and has zero build step. The same JS will be
portable later when a Blazor wallet lands.

**Alternatives considered:**

- *Blazor WebAssembly demo page* (deferred to Phase 4): pulls in an
  entire runtime for a five-line flow.
- *No frontend, curl only* (rejected): the user-facing value of SIWE is
  the MetaMask prompt; a text-only demo hides the part that matters.

### D10 — Integration tests via `WebApplicationFactory<Program>`

**Decision:** `Auth.IntegrationTests` spins up the real `Program`
via `WebApplicationFactory<Program>`, overrides `IClock` with a
deterministic `TestClock`, and overrides `INonceGenerator` with a
predictable generator so signatures can be produced inside the test.
The tests use a locally generated keypair (via
`SovereignID.Crypto.KeyPair.Generate` from the legacy project — which
is allowed for test projects; the rule forbids non-legacy **production**
code from consuming legacy, but `tests/bc-auth/` is test code, and
architecture tests already encode this exception).

**Test cases (integration):**

1. Happy path: GET /auth/nonce, sign with local key, POST /auth/verify →
   200 + JWT; `sub` claim matches the signing address.
2. Replay: call POST /auth/verify twice with the same valid payload →
   first succeeds, second returns 401 with `error=nonce_consumed`.
3. Expired nonce: advance `TestClock` past TTL → 401 with
   `error=nonce_expired`.
4. Wrong chain id in message → 400 with `error=unsupported_chain`.
5. Tampered message (signature valid for different payload) → 401 with
   `error=signature_mismatch`.
6. Unknown nonce (never issued) → 401 with `error=nonce_unknown`.
7. Malformed SIWE payload → 400 with `error=siwe_parse_failed` and the
   failing line number.

**Rationale:** `WebApplicationFactory` exercises middleware, routing,
DI wiring, and serialization — the parts unit tests cannot cover.
Deterministic clock + nonce make signature-based assertions repeatable
without a real wallet.

**Alternatives considered:**

- *Playwright-based browser tests with a real MetaMask* (deferred):
  valuable but heavy; in-process tests give us the same behavioral
  coverage at a fraction of the setup cost.

## Public HTTP Contract

```
GET /auth/nonce
  → 200 { "nonce": "abcd…", "expiresAt": "2026-04-25T18:10:00Z" }

POST /auth/verify
  body:  { "message": "<SIWE message>", "signature": "0x…" }
  → 200  { "jwt": "eyJ…", "address": "0xA0b…", "expiresAt": "…" }
  → 400  { "error": "siwe_parse_failed", "detail": "Line 7: expected 'Version: 1'" }
  → 400  { "error": "unsupported_chain",  "detail": "Got 1, expected 11155111" }
  → 401  { "error": "nonce_unknown"       | "nonce_consumed" | "nonce_expired"
                   | "signature_mismatch" }
```

Errors are JSON objects with a stable `error` code (snake_case) for
machine consumers and a human-readable `detail` where appropriate.

## NuGet Dependencies

Added only to the projects that need them (architecture rules still
apply):

```xml
<!-- Auth.Api -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.*" />

<!-- Auth.Infrastructure -->
<PackageReference Include="Nethereum.Signer" Version="4.26.*" />
<PackageReference Include="Nethereum.Util"   Version="4.26.*" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.*" />

<!-- Auth.IntegrationTests -->
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.*" />
<PackageReference Include="xunit" Version="2.9.*" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.*" />
<PackageReference Include="FluentAssertions" Version="7.*" />
<!-- A legacy ProjectReference to src/legacy/SovereignID.Crypto is permitted
     for tests; it is NOT allowed for non-legacy production code. -->
```

No new packages in `Auth.Domain` or `Auth.Application` (both stay
dependency-free, per the architecture rules).

## Demo Output (target)

Browser:

```
SovereignID · Sign-In with Ethereum (Phase 2)

[ Sign in with Ethereum ]   status: idle

→ requesting nonce … ok (expires 18:10:00Z)
→ opening MetaMask …
→ message signed by 0xA0b86991c6218b36c1d19d4a2e9eb0cE3606eB48
→ posting /auth/verify … ok
✓ session established
  address : 0xA0b86991c6218b36c1d19d4a2e9eb0cE3606eB48
  jwt     : eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIweEE…
  expires : 2026-04-26T18:05:00Z
```

## Constraints Summary

- **TTL:** 10 minutes, enforced by `AuthChallenge` against `IClock`.
- **Single-use:** repository `DeleteAsync` is called inside the same
  handler path as successful verification; `FindByNonceAsync` returning
  `null` on the second call is the replay defense.
- **Chain:** only `11155111` accepted; every other id → domain error.
- **Secrets:** `AUTH_JWT_SIGNING_KEY` is the only required secret;
  read from environment at startup; never logged.
- **Async:** every port, handler, and endpoint method is async with
  `CancellationToken`.
- **Logging:** `ILogger<T>` in handlers and adapters; log level `Info`
  for happy path (address + nonce fingerprint only, never the full
  signature or key), `Warning` for domain errors, `Error` for
  infrastructure failures.

## Risks / Trade-offs

- **[Parser drift]** SIWE spec has optional fields (`Request ID`,
  `Resources`). Strict parsers can reject valid messages from exotic
  wallets. → Mitigation: accept the EIP-4361 canonical shape used by
  MetaMask; unknown fields after `Issued At` are tolerated when they
  match the documented grammar and ignored otherwise.
- **[In-memory store for MVP]** A restart clears all outstanding
  nonces. → Mitigation: acceptable for the single-instance Sepolia
  demo; a future change introduces persistence behind the same port.
- **[No CSRF on /auth/verify]** The endpoint is safe by construction
  (replay protection comes from the nonce), but the frontend still
  posts JSON from the same origin. → Mitigation: keep the endpoint
  JSON-only and document that consumers must treat the returned JWT as
  a bearer token, not a cookie.
- **[Nethereum in Auth.Infrastructure]** Adding the package here is
  allowed by the architecture rules, but duplicates the adapter surface
  that `SharedKernel.Infrastructure` exposes. → Mitigation: when a
  second BC needs signature recovery, move `ISiweSignatureVerifier` to
  the shared kernel with a justification in that change's `design.md`.
- **[Ambient `Program` class]** `WebApplicationFactory<Program>`
  requires `Program` to be `public partial` (Minimal API top-level
  statements hide it by default). → Mitigation: add an explicit
  `public partial class Program;` at the end of `Program.cs` (standard
  ASP.NET Core integration-test pattern).

## Migration Plan

This change is additive; no code under `src/legacy/` or the other BC
folders is modified.

1. Populate `Auth.Domain` types (value objects, aggregate, ports,
   errors).
2. Add `Auth.Domain.Tests` and get full domain coverage green before
   touching any other layer.
3. Populate `Auth.Application` with the two handlers + unit tests
   using in-memory doubles for every port.
4. Populate `Auth.Infrastructure` adapters; add unit tests for the
   SIWE parser against the EIP-4361 canonical example and a handful of
   negative cases.
5. Create `Auth.Api` (new project), wire DI, map the two endpoints,
   add JWT authentication middleware (even though this phase doesn't
   gate any endpoint behind `[Authorize]` — the middleware is
   configured so Phase 3's `/auth/me` or other consumers can use it).
6. Create `Auth.IntegrationTests` and implement the seven cases listed
   in §D10.
7. Add the static frontend (`index.html`, `app.js`).
8. Update `SovereignID.sln` to include the four new projects under
   their matching solution folders.
9. Run `dotnet build` and `dotnet test` (incl. architecture tests).
   All green.

**Rollback:** revert the commit. Because the change is purely additive
and does not modify any existing assembly, the solution returns to its
exact previous state.

## Open Questions

- **OQ1:** Should the JWT include a `did` claim (`did:ethr:sepolia:0x…`)
  now, even though DID resolution is a Phase 3 concern? Default plan:
  include it as a pure string derivation (`"did:ethr:sepolia:" +
  Address.ToLowerInvariant()`) with no validation. Downstream services
  that trust it in Phase 3 will not need a JWT shape change.
- **OQ2:** Should `AUTH_NONCE_TTL` be configurable via environment
  variable? Default plan: ship with a hardcoded 10-minute constant and
  revisit only if a test or demo scenario needs a shorter value.
- **OQ3:** Should the frontend display the raw SIWE message before
  signing (as MetaMask does automatically)? Default plan: yes — shows
  the user exactly what they're signing and reinforces the teaching
  goal of the demo.
