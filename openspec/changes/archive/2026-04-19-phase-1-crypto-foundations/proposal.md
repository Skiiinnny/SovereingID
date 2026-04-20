# Proposal: Phase 1 — Crypto Foundations

> SovereignID · openspec/changes/phase-1-crypto-foundations/proposal.md

## What & Why

Before any identity logic, we need solid command of the cryptographic
primitives that underpin everything else. This phase builds the foundation:
key pairs, message signing, signature verification, and blockchain connectivity —
all from C# using Nethereum.

This is not throwaway work. The `SovereignID.Crypto` library produced here
will be imported by every other service in the project.

## What's Changing

- New solution: `SovereignID.sln`
- New project: `SovereignID.Crypto` (class library)
- New project: `SovereignID.Crypto.Tests` (xUnit)
- New project: `SovereignID.Chain` (blockchain connectivity)
- Console demo: `SovereignID.Demo.Phase1`

## What's NOT Changing

Nothing — this is a greenfield start.

## Risks

- Nethereum API surface is large; wrong abstraction choices now = refactor later
- Sepolia RPC rate limits on free tier (Infura/Alchemy) — mitigated by caching
