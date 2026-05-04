## Why

`PresentationVerifier` concentraba envoltura W3C de la **VP**, extracción del **VC** incrustado, política de documento (ahora delegada en `VcSliceA.Document`), enlaces **holder** ↔ **subject**, y dos recuperaciones **EIP-712**, con poca **localidad** y un único tipo difícil de evolucionar. Se acordó en diseño (glosario en `src/bc-verifier/CONTEXT.md`: **VP envelope**, **cadena de verificación del VC embebido**) partir por **responsabilidad** antes de introducir DI, manteniendo **orden de errores** y comportamiento respecto a `proof` en la raíz de la VP.

## What Changes

- Extraer un módulo **`internal`** para la **VP envelope** (JSON raíz: `type`, `holder`, `verifiableCredential` con un solo objeto VC) hasta obtener `holderDid`, dirección parseada y `JsonElement` del VC — **sin** invocar `TituloGraduacionVcDocumentValidator` y **sin** exigir `proof` en la raíz de la VP (orden actual del producto).
- Extraer un segundo módulo **`internal`** para la **cadena de verificación del VC embebido**: validación de documento con `TituloGraduacionVcDocumentValidator`, coherencia holder/subject, prueba y firma EIP-712 del VC, luego comprobación de `proof` de la VP y EIP-712 del titular.
- Introducir un **helper interno compartido** para leer `proof.type` / `proofValue` (misma regla para prueba del VC y de la VP).
- Mantener **`PresentationVerifier.Verify`** (o el punto de entrada público actual) como fachada fina; nuevos tipos **`internal`** en `Verifier.Application` hasta existir un segundo consumidor.
- Tests: **mantener un solo archivo** `PresentationVerifierTests` en la primera iteración del refactor; partir tests solo cuando el código estabilice.

## Capabilities

### New Capabilities

- `vp-presentation-split`: Requisitos de estructura interna del flujo de verificación de **VP** slice A (envoltura vs cadena embebida, helper de prueba, orden de verificación, visibilidad y tests).

### Modified Capabilities

- _(ninguno: no se cambian requisitos normativos de `openspec/specs/vc-slice-a/spec.md` salvo empaquetado del verificador; el comportamiento observable acordado es el mismo.)_

## Impact

- **Código:** `src/bc-verifier/SovereignID.Verifier.Application/Presentation/` (nuevos archivos `internal`, `PresentationVerifier.cs` más delgado).
- **Tests:** `tests/bc-verifier/.../PresentationVerifierTests.cs` sin división inicial; mismas aserciones de códigos de error y orden donde aplique.
- **APIs públicas:** sin cambio de firma pública previsto en `PresentationVerifier.Verify`.
- **Dependencias:** sin nuevos paquetes; mismas referencias a `VcSliceA.Document` y `VcSliceA.Eip712`.
