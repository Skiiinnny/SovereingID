# Design: Phase 3 VC — slice A

> Técnica · `openspec/changes/phase-3-vc-slice-a/design.md`  
> Motivación: `proposal.md` · Requisitos: `specs/vc-slice-a/spec.md`

## Context

- **Estado actual:** `bc-issuer` y `bc-verifier` existen como capas Onion con
  *markers*; `SharedKernel.Domain` ya incluye `DecentralizedIdentifier`,
  `EthereumAddress`, etc. `bc-auth` demuestra SIWE, recuperación de firma y
  abstracción de reloj vía tests.
- **Dominio acordado** (sesión grill + `src/shared/CONTEXT.md`): VC 1.1
  perfil único **TituloGraduacion**, VP mínima con un VC incrustado,
  identidades **`did:ethr:sepolia:0x…`**, pruebas **EIP-712** separadas para
  emisor (VC) y titular (VP), sin IPFS ni registro on-chain en este cambio.
- **Restricción:** sin secretos en repo; claves de test solo en proyectos de
  prueba o variables locales no versionadas.

## Goals / Non-Goals

**Goals**

- Modelar en **Domain** la emisión lógica de un VC válido según la spec y
  la verificación de una VP que lo contiene.
- Fijar **EIP-712** (dominios y tipos) suficientemente para generar y
  verificar vectores en tests; documentar el `proof` JSON-LD paralelo al
  modelo W3C (tipo de prueba de producto, p. ej. string estable acordado en
  implementación y reflejado en spec).
- Permitir **tests deterministas** (sin red) para parseo DID, allowlist de
  claims, fechas y verificación cripto.

**Non-Goals**

- Contratos Solidity, RPC Sepolia en CI obligatorio, web3.storage, APIs HTTP
  de Issuer/Verifier expuestas (pueden quedar preparadas en cambios
  posteriores).
- Resolución DID genérica, JSON-LD full framing con descarga remota de
  contextos más allá de la URL W3C fija ya acordada.
- Selective disclosure, BBS+, cifrado PII (P1.2).

## Decisions

1. **`CredentialType` en SharedKernel**  
   - **Qué:** enum (o tipo cerrado equivalente) con al menos `TituloGraduacion`,
     referenciado por Issuer y Verifier.  
   - **Por qué:** una sola fuente de verdad para el perfil admitido en slice A.

2. **Parsing `did:ethr:sepolia:` → `EthereumAddress`**  
   - **Qué:** helper puro en Domain o SharedKernel (sin I/O) que valide prefijo
     red Sepolia y 40 hex minúsculas.  
   - **Alternativa descartada:** aceptar cualquier string como `issuer` —
     rompe la verificación cripto sin reglas claras.

3. **Dos familias EIP-712 explícitas**  
   - **VC (emisor):** dominio de producto dedicado (nombre/versión/chainId
     Sepolia para alinear con wallets) y tipo primario que cubra los campos
     necesarios para integridad del VC (ver spec: al menos vínculo con
     `credential` `id`, `issuer` y digest de claims normalizados, o lista
     cerrada de campos tipados).  
   - **VP (titular):** dominio/tipos **distintos** del VC para evitar replay
     entre “firmar credencial” y “firmar presentación”.  
   - **Alternativa descartada:** reutilizar el mismo struct EIP-712 para VC y
     VP — riesgo de confundir intención de firma.

4. **Representación `proof` en JSON-LD**  
   - **Qué:** objeto(s) bajo `proof` con `type` de producto (string estable),
     `proofPurpose` acorde a uso, y campos necesarios para verificar con
     Nethereum (`proofValue` / `jws` / componentes `eip712` — detalle en
     implementación alineado con la spec `vc-slice-a`).  
   - **Por qué:** VC Data Model 1.1 admite extensiones; DataIntegrity 1.0
     completo puede ser pesado para slice A; se documenta el mínimo
     interoperable internamente.

5. **Reloj para `issuanceDate` / `expirationDate`**  
   - **Qué:** puerto `IClock` o reutilizar el patrón ya usado en tests de
     Auth (`IClock` / `TestClock`) inyectado en handlers de verificación.  
   - **Por qué:** cumplir “no futuro” / “no caducado” sin `DateTime.UtcNow`
     directo en dominio puro (facilita tests).

6. **Capas**  
   - **Issuer:** comando `IssueTituloGraduacionCredential` (nombre exacto
     TBD en código) + handler que orquesta validación de entrada, construcción
     JSON, llamada al puerto firmante.  
   - **Verifier:** comando `VerifyPresentation` + handler que valida forma
     VP, extrae VC, valida VC, verifica dos pruebas EIP-712.  
   - **Infra:** implementaciones concretas EIP-712 y (opcional) noop de
     registro — solo si hace falta compilar; slice A puede vivir en Domain
     + tests con fakes.

## Risks / Trade-offs

| Riesgo | Mitigación |
|--------|------------|
| Canonicalización JSON distinta entre emisor y verificador | Fijar en design implementación: orden de claves UTF-8, reglas explícitas en spec para el digest firmado. |
| Wallets no muestran friendly name del tipo EIP-712 VP | Aceptado en slice A; documentar en README de dev. |
| `https://www.w3.org/2018/credentials/v1` no resuelve en entorno air-gapped | Tests unitarios no dependen de fetch; opcional cache local en infra futura. |

## Migration Plan

- No hay datos productivos ni contratos: **aplicar** = merge del cambio de
  código cuando `tasks.md` esté completo; sin migración de BD.
- Cuando llegue IPFS/cadena: sustituir puertos por adaptadores reales sin
  cambiar los requisitos de la VP/VC ya cubiertos aquí.

## Open Questions

- Nombre final exacto del **tipo** EIP-712 y del string `proof.type` (deben
  alinearse entre spec, código y vectores en el PR de implementación).
- Si el **holder** firma un digest del VP completo o un subconjunto de
  campos — cerrar en el primer PR con el mismo criterio en emisor y
  verificador.
