## Why

La forma del **VC TituloGraduacion** (JSON-LD, `@context`, `credentialSubject`) y las reglas de claims están repartidas entre el **Issuer** (`VerifiableCredentialJsonBuilder`, `TituloGraduacionClaimsValidator`) y el **Verifier** (`PresentationVerifier`, `EmbeddedTituloGraduacionSubjectValidator`). Eso genera riesgo de deriva: un cambio de producto puede quedar coherente en un bounded context y roto en el otro, y el verificador no exigía aún el mismo vocabulario inline del segundo `@context` que el emisor materializa. Se acordó centralizar la política en un **módulo** compartido sin Nethereum, validando el documento tal como circula en JSON, con validación estricta en **emisor y verificador** y prefijos de error propios de cada contexto en el borde.

## What Changes

- Nuevo ensamblado **`SovereignID.VcSliceA.Document`** (o nombre equivalente acordado en implementación) bajo `src/shared/`, **sin** referencia a Nethereum: validación de forma del **VC** slice A para el perfil **TituloGraduacion** sobre **`JsonElement`** (o documento parseado equivalente).
- Reglas unificadas: `@context` (primer elemento W3C VC v1; segundo objeto con **mapa de términos e IRIs idénticos** al usado en construcción de documento de emisión), `type`, `id` `urn:uuid:`, `issuer` / `issuanceDate` / `expirationDate` opcional según spec existente, **`credentialSubject`** con allowlist estricta y formatos de claims (incl. `awardDate` civil).
- **Issuer:** validar el VC (sin `proof` o con `proof`, según se decida en diseño) **antes** de firmar EIP-712; fallos mapeados a códigos con prefijo de contexto emisor.
- **Verifier:** delegar la comprobación de forma de documento VC embebido al mismo **módulo**; conservar fuera cripto (EIP-712), reloj inyectado y reglas de **VP** (`holder`, conteo de VCs, prueba del titular).
- El **módulo** devuelve **códigos canónicos internos** (sin prefijo de BC); Issuer y Verifier aplican **mapeo** a sus cadenas actuales (`degreeTitle_required`, `vc_subject_*`, etc.) donde aplique.
- Tests dedicados del nuevo ensamblado con matrices JSON; ajuste de tests existentes que hoy codifican duplicados.
- Actualización de referencias de proyecto (`*.csproj`, solución) y, si aplica, reglas de arquitectura o documentación en `src/shared/CONTEXT.md` ya alineada con este seam.

## Capabilities

### New Capabilities

- `vc-slice-a-document-conformance`: Contrato normativo del **módulo** compartido de conformidad de documento VC (entradas, reglas de `@context` estricto, subject, códigos canónicos, obligación de uso en Issuer y Verifier). Complementa `vc-slice-a` sin sustituir sus requisitos de producto ya publicados.

### Modified Capabilities

- _(ninguno en `openspec/specs/` a nivel de delta separado: los requisitos de producto siguen en `vc-slice-a`; este cambio añade la capacidad de implementación centralizada.)_

## Impact

- **Código:** `src/bc-issuer` (dominio/aplicación de emisión), `src/bc-verifier` (`PresentationVerifier` y eliminación o reducción de validadores duplicados), nuevo `src/shared/SovereignID.VcSliceA.Document`, tests nuevos y existentes (`PresentationVerifierTests`, tests del emisor).
- **Dependencias:** `SovereignID.SharedKernel.Domain` para parsing/normalización **DID** `did:ethr:sepolia:` en la política de documento si se centraliza aquí (recomendado para una sola verdad).
- **APIs:** sin cambio obligatorio de contratos HTTP públicos; posible alineación de textos de error solo si el mapeo expone los mismos códigos que hoy.
- **Sistemas:** ninguno externo; CI sigue sin red para pruebas unitarias de slice A.
