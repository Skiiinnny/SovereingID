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
| **VP envelope (slice A)** | La parte mínima del JSON raíz de la VP que el producto exige antes de profundizar en el VC: `type`, `holder` (DID Sepolia canónico) y `verifiableCredential` con exactamente un objeto VC incrustado. **No** incluye la política de documento del perfil **TituloGraduacion** (eso queda en `SovereignID.VcSliceA.Document`), la verificación criptográfica ni la comprobación de que exista `proof` en la raíz de la VP: en slice A esa comprobación sigue **después** de la conformidad de documento del VC incrustado (orden de verificación acordado para el producto). |
| **Subject DID (en VC)** | El verificador interpreta `credentialSubject.id` según la convención en [`src/shared/CONTEXT.md`](../shared/CONTEXT.md) para enlazar la prueba del titular en la VP con la cuenta Ethereum esperada. |
| **Issuer DID** | El verificador comprueba que `issuer` sigue la convención en [`src/shared/CONTEXT.md`](../shared/CONTEXT.md) y que la firma EIP-712 del VC corresponde a esa cuenta. |
| **Prueba del titular (VP)** | **EIP-712** sobre la presentación; convención en [`src/shared/CONTEXT.md`](../shared/CONTEXT.md). |
| **Cadena de verificación del VC embebido (slice A)** | Fase posterior a la **VP envelope**: conformidad de documento del VC incrustado (`SovereignID.VcSliceA.Document`), coherencia **`holder`** ↔ **`credentialSubject.id`**, verificación **EIP-712** del emisor sobre el VC, y por último presencia/forma mínima de **`proof`** en la raíz de la VP + **EIP-712** del titular. |
| **TituloGraduacion** | Único perfil: el VC debe declarar `TituloGraduacionCredential` en `type`; allowlist en [`src/shared/CONTEXT.md`](../shared/CONTEXT.md). |

Further terms will be added as they are agreed in `/grill-with-docs` or implementation.
