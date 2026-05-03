# Context map — SovereignID

Each bounded context has its own `CONTEXT.md` (glossary and domain language for that area). Read the entries relevant to the code you are changing.

The table below is the **intended location** for each glossary. Files appear when domain terms are captured (for example in a `/grill-with-docs` session); until then, follow `docs/agents/domain.md` and do not assume a file exists just because it is linked.

| Bounded context | `CONTEXT.md` |
|-----------------|--------------|
| Shared kernel | [`src/shared/CONTEXT.md`](src/shared/CONTEXT.md) |
| Auth (SIWE, sessions) | [`src/bc-auth/CONTEXT.md`](src/bc-auth/CONTEXT.md) |
| Issuer (VC issuance) | [`src/bc-issuer/CONTEXT.md`](src/bc-issuer/CONTEXT.md) |
| Verifier (VC verification) | [`src/bc-verifier/CONTEXT.md`](src/bc-verifier/CONTEXT.md) |
| Legacy (Phase 1 crypto / chain, frozen) | [`src/legacy/CONTEXT.md`](src/legacy/CONTEXT.md) |

System-wide architecture decisions live under [`docs/adr/`](docs/adr/). Context-specific ADRs may live under `src/<bc-folder>/docs/adr/` when added.
