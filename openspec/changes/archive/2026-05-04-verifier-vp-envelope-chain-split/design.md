## Context

El verificador slice A ya delega la conformidad de documento del **VC** en `TituloGraduacionVcDocumentValidator` (`SovereignID.VcSliceA.Document`). `PresentationVerifier` sigue mezclando parseo de envoltura **VP**, cadena criptográfica del VC embebido, prueba de la **VP** y lectura repetida de `proof`. El glosario del BC (`src/bc-verifier/CONTEXT.md`) define **VP envelope** y **cadena de verificación del VC embebido** y el orden respecto a `proof` en la raíz.

## Goals / Non-Goals

**Goals:**

- Dos unidades **`internal`** con responsabilidades claras: (1) envoltura + extracción, (2) cadena embebida + cripto VP/VC.
- Un helper **`internal`** compartido para extraer `proofValue` y validar `proof.type` contra `VcSliceAEip712Constants.ProofType`.
- Misma semántica observable: mismos códigos de error en los mismos casos que hoy, incluido que **`proof` en la raíz de la VP** se valide **después** de la validación de documento del VC.
- Fachada pública estable (`PresentationVerifier.Verify`).

**Non-Goals:**

- Introducir `IPresentationVerifier` o DI en este cambio.
- Partir `PresentationVerifierTests` en varios archivos en la primera iteración.
- Mover lógica a otro ensamblado ni cambiar reglas de `VcSliceA.Document`.

## Decisions

1. **Nombres de tipos (sugeridos, ajustables en implementación):** `VpEnvelopeParser` (o `MinimalVpEnvelope`) y `TituloGraduacionEmbeddedVcPresentationChain` (o nombre más corto `EmbeddedTituloGraduacionVpChain`) como `internal static` o tipos equivalentes en `Presentation/`.

2. **Helper compartido:** clase `internal` estática p. ej. `PresentationEip712ProofValueReader` con método que reciba `JsonElement proof` y devuelva `(bool ok, string? signature, string? errorCode)` alineado con los códigos actuales (`proof_type_invalid`, `proof_value_missing`, `proof_value_empty`).

3. **Orquestación:** `PresentationVerifier.Verify` parsea JSON una vez, llama a (1), si éxito llama a (2) con `nowUtc` y raíz si el tramo VP posterior necesita re-leer `proof` — preferir pasar `JsonElement` raíz y VC ya localizados para evitar re-parse.

4. **Visibilidad:** todo lo nuevo `internal` en `SovereignID.Verifier.Application`; tests siguen en proyecto existente que ya referencia Application.

## Risks / Trade-offs

- **[Riesgo] Regresión de orden de errores** → Mitigación: ejecutar batería completa de `PresentationVerifierTests` sin cambiar nombres de casos; añadir un caso explícito si hoy falta cobertura de “VC inválido antes de comprobar `vp_proof_missing`”.
- **[Trade-off] Tipos `internal` no probables en ensamblado de tests** → Los tests siguen ejercitando solo la API pública; cobertura del helper indirectamente — aceptado hasta extraer pruebas o `InternalsVisibleTo` si hiciera falta (fuera de alcance inicial).

## Migration Plan

Ninguno: refactor interno. Rollback por revertir commit.

## Open Questions

- Nombres finales de archivos/clases si el equipo prefiere convención distinta (p. ej. sufijo `Verifier` vs `Parser`).
