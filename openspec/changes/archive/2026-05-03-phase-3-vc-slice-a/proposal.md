# Proposal: Phase 3 VC — slice A (domain + contracts, no chain/IPFS)

> SovereignID · `openspec/changes/phase-3-vc-slice-a/proposal.md`

## Why

Phase 2 entregó SIWE y JWT; el triángulo emisor → titular → verificador del
`master-design` exige **W3C Verifiable Credentials** y **Verifiable
Presentations** verificables criptográficamente. Este cambio aborda el
**primer incremento acotado (slice A)**: modelo de datos, JSON-LD conforme,
firmas **EIP-712** en VC y VP, y reglas de verificación **sin** IPFS,
**sin** contratos en Sepolia y **sin** APIs HTTP obligatorias en el mismo
cambio (puertos con fakes/in-memory). Desbloquea tests deterministas y
alinea emisor y verificador con el lenguaje de dominio ya fijado en
`src/*/CONTEXT.md` y `src/shared/CONTEXT.md`.

## What Changes

- **Shared kernel:** introduce **`CredentialType`** (incluye al menos
  `TituloGraduacion`) y otros value objects / helpers de identidad VC/VP
  acordados en diseño, viviendo en `SovereignID.SharedKernel.Domain`.
- **`bc-issuer` Domain (y tests):** construcción y validación de un VC
  **TituloGraduacion** (JSON-LD 1.1), allowlist de claims, puerto de firma
  EIP-712 del emisor; comando/caso de uso de emisión en Application cuando
  el diseño lo fije.
- **`bc-verifier` Domain (y tests):** validación de **VP mínima** (un VC
  incrustado, `holder` obligatorio, `type` solo `VerifiablePresentation`),
  verificación EIP-712 del emisor sobre el VC y EIP-712 del titular sobre la
  VP; políticas de fechas (`issuanceDate`, `expirationDate` opcional).
- **Especificación normativa** del artefacto JSON (contextos, `type`,
  DIDs `did:ethr:sepolia:…`, `urn:uuid:` para `id` del VC, pruebas).
- **No** en este cambio: adaptadores IPFS/web3.storage, escritura/lectura de
  registro on-chain, endpoints públicos Issuer/Verifier API (pueden
  prepararse puertos y quedar sin host), wallet Blazor, resolución DID
  genérica externa.

## Capabilities

### New Capabilities

- **`vc-slice-a`** — Requisitos de **emisión** (forma VC, claims
  `TituloGraduacion`, firma EIP-712 del emisor) y **verificación** (VP
  mínima, coherencia `holder` / `credentialSubject.id`, firma EIP-712 del
  titular, validación temporal y de tipos JSON-LD) para el primer incremento
  sin infraestructura de almacenamiento anclado en cadena.

### Modified Capabilities

- _(ninguno — los requisitos nuevos viven en `vc-slice-a`; el
  `solution-architecture` existente ya permite SharedKernel.Domain como
  hogar de value objects transversales.)_

## Impact

- **Código:** `src/shared/SovereignID.SharedKernel.Domain`,
  `src/bc-issuer/SovereignID.Issuer.*`,
  `src/bc-verifier/SovereignID.Verifier.*`, proyectos de prueba bajo
  `tests/` que correspondan.
- **Dependencias:** probablemente Nethereum / utilidades EIP-712 ya usadas
  o alineadas con `bc-auth`; sin nuevas cadenas RPC obligatorias para slice A.
- **Documentación de dominio:** ya actualizada en `CONTEXT.md`; este
  cambio formaliza el contrato en OpenSpec.
- **Riesgo:** definición exacta de tipos EIP-712 y de objeto `proof` en
  JSON-LD — se cierra en `design.md` y en la spec.
