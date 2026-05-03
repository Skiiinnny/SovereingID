# Proposal: Phase 2 — SIWE Authentication

> SovereignID · openspec/changes/phase-2-siwe-auth/proposal.md

## What & Why

Phase 1 delivered the cryptographic primitives (keys, signing, hashing,
notarization) and the `add-onion-architecture-foundation` change laid down
the bounded-context scaffolding for `bc-auth`, `bc-issuer` and `bc-verifier`.
`bc-auth` today is three empty marker projects.

Phase 2 makes that scaffolding real. It delivers the first
business-meaningful capability of SovereignID: **Sign-In with Ethereum
(EIP-4361)** — authenticating a user by having them sign a nonce-bound
challenge with their wallet and exchanging that signature for a JWT session.
This is the gateway scenario (`SC-01` in `openspec/specs/scenarios.md`) and
the prerequisite for every later phase, since VC issuance and verification
both assume an authenticated holder.

The phase also proves the Onion architecture end-to-end for the first time:
a Domain that models the challenge as a first-class aggregate, an
Application layer with two CQRS use cases, an Infrastructure layer with
adapters (manual SIWE parser, in-memory repository, signature recovery via
Nethereum, JWT issuer), and an `Api` project exposing two HTTP endpoints
plus a minimal vanilla-JS frontend so the flow is demonstrable in a browser
with MetaMask.

## What's Changing

- New project: `SovereignID.Auth.Domain` gets populated with the
  `AuthChallenge` aggregate, the `Nonce` value object, domain errors, and
  ports (`IAuthChallengeRepository`, `INonceGenerator`,
  `ISiweMessageParser`, `ISiweSignatureVerifier`, `IJwtTokenIssuer`).
- New project: `SovereignID.Auth.Application` gets populated with two CQRS
  handlers: `GenerateNonceQueryHandler` and `VerifySiweCommandHandler`.
- New project: `SovereignID.Auth.Infrastructure` gets the concrete adapters:
  `InMemoryAuthChallengeRepository`, `SecureRandomNonceGenerator`,
  `ManualSiweMessageParser` (hand-rolled EIP-4361 parser, no
  `Nethereum.Templates.Siwe`), `NethereumSiweSignatureVerifier`, and
  `JwtBearerTokenIssuer`.
- **New project:** `SovereignID.Auth.Api` (ASP.NET Core Minimal API, .NET 9).
  Exposes `GET /auth/nonce` and `POST /auth/verify`. Wires all DI. Hosts
  the static frontend.
- **New project:** `SovereignID.Auth.IntegrationTests` under
  `tests/bc-auth/`. Uses `WebApplicationFactory<Program>` to exercise the
  full request pipeline without a network or real wallet.
- **New project:** `SovereignID.Auth.Domain.Tests` and
  `SovereignID.Auth.Application.Tests` for unit coverage of the aggregate,
  the SIWE parser, and the two handlers.
- New frontend: `SovereignID.Auth.Api/wwwroot/index.html` + `app.js` —
  vanilla JS demo page that calls `GET /auth/nonce`, builds the SIWE
  message, asks MetaMask to sign it, posts to `POST /auth/verify`, and
  displays the returned JWT + recovered address. No frameworks, no build
  step.
- Solution file updated: new projects added under the matching solution
  folders (`bc-auth`, `tests/bc-auth`).
- README: short note pointing to the Phase 2 demo endpoint; no other
  documentation rewrite.

## What's NOT Changing

- No changes inside `src/legacy/` (Phase 1 code remains frozen).
- No changes inside `src/bc-issuer/` or `src/bc-verifier/` scaffolding.
- No changes to `SharedKernel.Domain` (no new cross-context types — the
  ports introduced are all Auth-specific, living in `Auth.Domain`).
- No database: the `InMemoryAuthChallengeRepository` is sufficient for the
  MVP. Persistence arrives in a later change if a real deployment demands
  it.
- No DID resolution yet (Phase 3). Authentication returns the raw
  `EthereumAddress` inside the JWT; the `did:ethr` mapping is deferred.
- No `/auth/me` endpoint yet. Only the two endpoints required by `SC-01`
  are shipped; `/auth/me` is trivial to add later once a consumer needs it.
- No UI framework. The frontend is a single HTML page + one JS file,
  strictly to prove the flow in a browser.

## Capabilities

### New Capabilities

- `siwe-authentication` — exposes the two public endpoints
  (`GET /auth/nonce`, `POST /auth/verify`) and the behavioral contract
  around them: nonce freshness (TTL 10 min), single-use semantics,
  mandatory `ChainId = 11155111` (Sepolia), address recovery via
  `personal_sign`, and JWT issuance on successful verification.

### Modified Capabilities

<!-- None. The project-level specs (architecture.md, scenarios.md) and the
     solution-architecture capability remain untouched. -->

## Risks

- **Manual SIWE parser correctness.** Hand-rolling EIP-4361 is the main
  source of subtle bugs (missing `Chain ID`, wrong line order,
  whitespace). → Mitigation: unit tests cover the canonical example from
  the EIP, plus negative cases (missing fields, wrong order, malformed
  addresses). Parser is isolated behind `ISiweMessageParser` so a future
  swap to a supported library is a one-adapter change.
- **Nonce collision / replay.** A predictable or reusable nonce breaks the
  entire authentication guarantee. → Mitigation:
  `SecureRandomNonceGenerator` uses `RandomNumberGenerator.GetBytes(16)`;
  repository enforces single-use by deleting on verification; TTL
  enforced by `AuthChallenge` itself (domain-level invariant, not an
  application-layer check).
- **Chain ID spoofing.** A signer on another chain (mainnet, Polygon)
  could replay a signature if we accepted any `Chain ID`. → Mitigation:
  hard-coded Sepolia (`11155111`) at domain validation; any other value
  fails verification with a typed domain error.
- **Clock skew.** Nonce TTL and SIWE `Not Before` / `Expiration Time`
  depend on agreement with the client. → Mitigation: `IClock` port
  (already in SharedKernel.Domain) is the single source of truth; tests
  inject a fake clock; TTL is generous (10 min) so 1–2 min client skew
  is tolerated.
- **JWT signing key leakage.** A leaked HMAC/RSA key invalidates every
  issued token. → Mitigation: signing key loaded exclusively from
  environment variable (`AUTH_JWT_SIGNING_KEY`), never hardcoded, never
  logged. Integration tests use an ephemeral in-memory key.
- **Frontend CORS / phishing confusion.** The demo frontend is served by
  the same origin as the API to avoid CORS complexity and to make the
  SIWE `domain` field honest. → Mitigation: `app.UseStaticFiles()` +
  `app.MapGet("/")` returns `index.html`; no cross-origin config.
- **Premature coupling to Nethereum in Auth.Infrastructure.** We could
  have routed signature recovery through `SharedKernel.Infrastructure`.
  → Mitigation: `ISiweSignatureVerifier` is an Auth-owned port. The
  Infrastructure adapter uses Nethereum directly; if later reused by
  another BC, it moves to the shared kernel under its own change with a
  documented justification (per solution-architecture spec §D3).

## Impact

- **Code:** ~4 new projects (`Auth.Api`, `Auth.Domain.Tests`,
  `Auth.Application.Tests`, `Auth.IntegrationTests`) plus substantial
  content added to the three existing but currently empty `Auth.*`
  projects.
- **Dependencies:** `Nethereum.Signer` is added to
  `SovereignID.Auth.Infrastructure` only (architecture rules permit
  Nethereum in `.Infrastructure`). `Microsoft.AspNetCore.Authentication.JwtBearer`
  and `System.IdentityModel.Tokens.Jwt` added to `Auth.Api` /
  `Auth.Infrastructure`. `Microsoft.AspNetCore.Mvc.Testing` added to
  `Auth.IntegrationTests`.
- **Configuration:** one new environment variable,
  `AUTH_JWT_SIGNING_KEY`. Optional `AUTH_JWT_ISSUER` and
  `AUTH_JWT_AUDIENCE` with sensible defaults. Never committed.
- **CI:** no changes. `dotnet build` and `dotnet test
  "SovereignID.sln" --filter "Category!=Integration"` keep working; the
  new integration tests run in-process via `WebApplicationFactory` so
  they are safe to leave in the default test run (no Sepolia calls
  needed for Phase 2).
- **Architecture tests:** existing rules apply automatically to the new
  projects via the dynamic assembly discovery already in place. The new
  `Auth.Api` project must satisfy the allowed-edges row (`Auth.Api →
  Auth.Application, Auth.Infrastructure`), which the existing tests
  enforce without modification.
- **Out of scope:** DID resolution, `/auth/me`, persistent challenge
  store, multi-chain support, account abstraction, UI framework, CSS
  polish beyond a minimal readable layout.
