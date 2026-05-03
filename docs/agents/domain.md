# Domain docs

How the engineering skills should consume this repo's domain documentation when exploring the codebase.

## Before exploring, read these

- **`CONTEXT-MAP.md`** at the repo root — maps each bounded context to a `CONTEXT.md`.
- For the area you are working in, read the **`CONTEXT.md`** linked from that map (Shared kernel, Auth, Issuer, Verifier, or Legacy as appropriate).
- **`docs/adr/`** — read ADRs that touch the area you are about to work in. If a bounded context has its own `docs/adr/` under `src/`, read those for context-scoped decisions.

If any of these files do not exist yet, **proceed silently**. Do not flag their absence or suggest creating them upfront. The producer skill (`/grill-with-docs`) creates them lazily when terms or decisions actually get resolved.

## File structure (this repo)

Multi-context layout — one glossary per bounded context:

```
/
├── CONTEXT-MAP.md
├── docs/adr/                              ← system-wide decisions
├── docs/agents/
└── src/
    ├── shared/
    │   ├── CONTEXT.md
    │   └── docs/adr/                      ← optional, context-scoped ADRs
    ├── bc-auth/
    │   ├── CONTEXT.md
    │   └── docs/adr/
    ├── bc-issuer/
    │   ├── CONTEXT.md
    │   └── docs/adr/
    ├── bc-verifier/
    │   ├── CONTEXT.md
    │   └── docs/adr/
    └── legacy/
        ├── CONTEXT.md
        └── docs/adr/
```

## Use the glossary's vocabulary

When your output names a domain concept (in an issue title, a refactor proposal, a hypothesis, a test name), use the term as defined in the **`CONTEXT.md` for that bounded context** (and shared kernel terms where they apply). Do not drift to synonyms the glossary explicitly avoids.

If the concept you need is not in the glossary yet, that is a signal — either you are inventing language the project does not use (reconsider) or there is a real gap (note it for `/grill-with-docs`).

## Flag ADR conflicts

If your output contradicts an existing ADR, surface it explicitly rather than silently overriding:

> _Contradicts ADR-0007 — but worth reopening because…_
