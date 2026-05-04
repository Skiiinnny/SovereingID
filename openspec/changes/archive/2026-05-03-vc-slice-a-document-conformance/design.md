## Context

Hoy la forma del **VC** `TituloGraduacion` se construye en **`VerifiableCredentialJsonBuilder`** (Issuer Domain) y se valida de forma procedural en **`PresentationVerifier`** + **`EmbeddedTituloGraduacionSubjectValidator`** (Verifier Application). Los claims también se validan en **`TituloGraduacionClaimsValidator`** sobre el record de dominio, sin la misma severidad que sobre JSON (p. ej. claves extra en `credentialSubject`). El glosario en `src/shared/CONTEXT.md` ya describe el seam **Document conformance module** y el ensamblado de trabajo **`SovereignID.VcSliceA.Document`**.

## Goals / Non-Goals

**Goals:**

- Un único **módulo** .NET compartido, sin Nethereum, que valide el **VC** slice A `TituloGraduacion` sobre datos JSON ya materializados (`JsonElement` del nodo raíz del VC).
- Segundo elemento de `@context`: objeto con **exactamente** el mapa de términos e IRIs que usa el builder de emisión (producto cerrado).
- **Issuer** y **Verifier** ejecutan la misma validación; códigos canónicos en el módulo; mapeo a prefijos de BC en el borde.
- Tabla de constantes de vocabulario y de `ProofType` donde corresponda para una sola fuente de verdad respecto al literal en JSON y al mensaje EIP-712.

**Non-Goals:**

- Cambiar el formato de **VP**, tipos EIP-712, ni añadir IPFS/cadena.
- Sustituir o mover implementación EIP-712 fuera de **`SovereignID.VcSliceA.Eip712`**.
- Unificar textos de error expuestos al cliente final (solo el módulo interno + mapeo).

## Decisions

1. **Nombre y ubicación del proyecto:** `SovereignID.VcSliceA.Document` en `src/shared/SovereignID.VcSliceA.Document/`, referenciado por `Issuer.Domain`, `Verifier.Application`, y tests; registrado en `SovereignID.sln`.

2. **Dependencia con `SharedKernel.Domain`:** el módulo **SHALL** usar **`EthrSepoliaDidParser`** (o equivalente acordado) para exigir `credentialSubject.id` e `issuer` como **DID** canónicos `did:ethr:sepolia:0x` + 40 hex minúsculas, alineado con `openspec/specs/vc-slice-a/spec.md`.

3. **Literal `proof.type`:** la constante de tipo de prueba (**`SovereignIDEip712Signature2026`**) vive en **`VcSliceA.Document`** como fuente única para el documento JSON. **`SovereignID.VcSliceA.Eip712`** referencia **`VcSliceA.Document`** solo para reexportar o delegar `ProofType` en `VcSliceAEip712Constants`, evitando cadena **Document → Eip712** (que introduciría Nethereum transitivo al grafo de referencia del proyecto Document). Concretamente: **Eip712 → Document** (Document sin referencia a Eip712).

4. **Momento de validación en el Issuer:** después de construir el JSON del VC sin prueba o con prueba adjunta — el spec debe fijar si la validación exige `proof` presente; recomendación: validar el documento **tal como se firmará** (campos de `proof` con forma mínima: `type`, `proofValue` presentes según política slice A) **o** dos entradas explícitas `ValidateUnsignedCredential` / `ValidateSignedCredentialShape`. Resolver en implementación con preferencia por **una** ruta JSON para reducir duplicación; si el emisor valida antes de `AttachIssuerProof`, el VC no tiene `proof` aún: el módulo debe soportar VC **sin** `proof` para el emisor y VC **con** `proof` para el verificador, **o** el emisor valida dos veces (costoso). **Decisión:** el módulo expone validación de **cuerpo de credencial** (todo excepto prueba opcional) y validación opcional / separada de bloque `proof` (tipo + `proofValue` no vacío) para el flujo verificador; el emisor llama la parte “sin proof” antes de firmar y opcionalmente la de proof después de adjuntar. Documentar ambas entradas en API pública del módulo.

   _Simplificación práctica:_ un método `ValidateCredentialDocument(JsonElement vc, CredentialValidationMode mode)` con `Unsigned` | `Signed` donde `Signed` exige objeto `proof` con reglas actuales de `TryGetProofSignature`.

5. **Fechas `issuanceDate` / `expirationDate`:** la validación **sintáctica** RFC 3339 UTC `Z` y comparación temporal (futuro / caducado) puede permanecer en el Verifier con reloj inyectado **o** entrar parcialmente al módulo con `DateTimeOffset nowUtc` pasado como parámetro. **Decisión:** pasar **`nowUtc`** al módulo para reglas temporales del VC en un solo lugar, de modo que Issuer pueda usar `DateTimeOffset.UtcNow` (o reloj inyectado si existe) y el Verifier su `IClock` — **misma lógica**, distinta fuente de tiempo.

6. **Eliminación de duplicados:** retirar **`EmbeddedTituloGraduacionSubjectValidator`** y **`TituloGraduacionClaimsValidator`** cuando el módulo cubra el mismo comportamiento; el handler del emisor puede validar claims vía JSON generado desde el builder o invocar solo el módulo sobre el `JsonObject` serializado.

## Risks / Trade-offs

- **[Riesgo] API del módulo demasiado grande** → Mitigación: API mínima (pocos métodos estáticos o un tipo `TituloGraduacionVcDocumentValidator` con métodos claros) y tests como contrato.
- **[Riesgo] Regresión en tests de integración cruzada** → Mitigación: conservar `PresentationVerifierTests` como oro hasta migrar; añadir tests unitarios densos en `VcSliceA.Document.Tests`.
- **[Trade-off] Producto cerrado en `@context`** → Emisores terceros no podrán usar sinónimos JSON-LD; aceptado por decisión de producto slice A.

## Migration Plan

No hay despliegue ni datos migrables. Rollback: revertir referencias de proyecto y restaurar validadores previos desde control de versiones.

## Open Questions

- Si el **Issuer** valida solo modo `Unsigned`, confirmar que ningún campo requerido por EIP-712 quede fuera de esa validación (debe alinearse con los campos que `TituloGraduacionCredential712` firma).
- Si `issuer` en JSON puede ser objeto en una futura versión: hoy string; el módulo sigue la spec `vc-slice-a` actual (string en slice A implementado).
