# AGENTS.md — AI Assistant Instructions

> SovereignID project · OpenSpec configuration

## Project Context

You are helping build **SovereignID**, a .NET platform for decentralized
self-sovereign digital identity. The stack is C# / .NET 9, Nethereum,
SIWE (EIP-4361), W3C Verifiable Credentials, and Ethereum Sepolia testnet.

The developer is a Senior .NET engineer learning blockchain/identity concepts
while building a portfolio product. Favor correctness and clean architecture
over brevity. Always explain crypto concepts when they appear in code.

## Spec-Driven Rules

1. **Read the spec before writing code.** Always check `openspec/specs/` and
   the active change folder before implementing anything.

2. **One change at a time.** Work on the current phase tasks in `tasks.md`.
   Do not skip ahead to future phases.

3. **Update tasks as you go.** Mark tasks `[x]` when complete.

4. **Never hardcode secrets.** No private keys, RPC URLs, or API keys in source.
   Always use environment variables or `appsettings.Development.json` (gitignored).

5. **Always write tests.** Every public method in `SovereignID.Crypto` needs a
   unit test. Integration tests (hitting Sepolia) must be marked
   `[Trait("Category","Integration")]` so they can be skipped in CI.

## Code Conventions

- Async everywhere: `Task<T>`, never `.Result` or `.Wait()`
- Records for immutable data (KeyPair, VC, DID Document)
- Interfaces for all external dependencies (blockchain, storage)
- XML doc comments on all public APIs
- No `Console.WriteLine` in library code — use `ILogger<T>`

## Naming

| Concept | C# Type Name |
|---------|-------------|
| Key pair | `KeyPair` |
| Verifiable Credential | `VerifiableCredential` |
| Verifiable Presentation | `VerifiablePresentation` |
| Decentralized Identifier | `DecentralizedIdentifier` (or `Did`) |
| SIWE message | `SiweMessage` (use Nethereum type) |

## What NOT to do

- Do not implement ZK proofs — out of scope for v1
- Do not deploy to mainnet — Sepolia only
- Do not add UI until Phase 4
- Do not use `.Wait()` or block on async
- Do not log private keys, even partially

## Active Phase

Currently in: **Phase 1 — Crypto Foundations**
Active tasks: `openspec/changes/phase-1-crypto-foundations/tasks.md`
