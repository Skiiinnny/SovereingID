# Verifier bounded context — glossary

## Agreed scope (Phase 3, first milestone)

- **Slice A (Opción A):** verificar **VP mínima** (W3C): `type` = `VerifiablePresentation` solamente; **`holder`** obligatorio e igual a `credentialSubject.id` del VC; **un solo VC incrustado** en `verifiableCredential`; validar VC 1.1 + prueba **EIP-712** del emisor y **EIP-712** del **titular** sobre la presentación, sin IPFS ni cadena real en este hito.
- **Primer incremento:** políticas de claims y `@type` para **un solo** perfil: **`TituloGraduacion`** (ver [`src/shared/CONTEXT.md`](../shared/CONTEXT.md)); ampliar a más perfiles en un cambio posterior.
- **Out of scope for this milestone:** descarga por CID, comparación de hash on-chain, lista de revocación on-chain; se sustituyen por **puertos** falsos o en memoria cuando el flujo los requiera.

## Glossary

| Term | Meaning |
|------|--------|
| **Verifier** | La parte que comprueba que una **VP** y sus VCs cumplen políticas de formato, firma del emisor y vínculo con el titular. |
| **VC** | Verifiable Credential — documento JSON-LD con prueba verificable (contenido o referencia dentro de la VP). |
| **VP** | Verifiable Presentation — artefacto de entrada principal del verificador en slice A (Opción 2); `type` solo `VerifiablePresentation`; **`holder`** obligatorio = `credentialSubject.id` del VC (ver [`src/shared/CONTEXT.md`](../shared/CONTEXT.md)). |
| **Subject DID (en VC)** | El verificador interpreta `credentialSubject.id` según la convención en [`src/shared/CONTEXT.md`](../shared/CONTEXT.md) para enlazar la prueba del titular en la VP con la cuenta Ethereum esperada. |
| **Issuer DID** | El verificador comprueba que `issuer` sigue la convención en [`src/shared/CONTEXT.md`](../shared/CONTEXT.md) y que la firma EIP-712 del VC corresponde a esa cuenta. |
| **Prueba del titular (VP)** | **EIP-712** sobre la presentación; convención en [`src/shared/CONTEXT.md`](../shared/CONTEXT.md). |
| **TituloGraduacion** | Único perfil: el VC debe declarar `TituloGraduacionCredential` en `type`; allowlist en [`src/shared/CONTEXT.md`](../shared/CONTEXT.md). |

Further terms will be added as they are agreed in `/grill-with-docs` or implementation.
