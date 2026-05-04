# Shared kernel — glossary (cross bounded contexts)

## Subject identifier in Verifiable Credentials

- En el JSON-LD del VC, **`credentialSubject.id`** identifica al titular con un **DID `did:ethr`** en la red acordada para el producto (**Sepolia** en desarrollo), en la forma canónica:

  `did:ethr:sepolia:<dirección Ethereum>`

- La parte `<dirección Ethereum>` es el literal `0x` seguido de **40 dígitos hexadecimales en minúsculas**, alineado con la normalización ya usada en autenticación SIWE hacia cuentas Ethereum.
- El tipo `DecentralizedIdentifier` del shared kernel representa el string completo; la capa de dominio puede derivar un `EthereumAddress` a partir de este prefijo para verificación criptográfica (VP, SIWE, etc.).

## Issuer identifier in Verifiable Credentials

- En slice A, **`issuer`** identifica al instituto con la **misma convención DID** que el titular: `did:ethr:sepolia:<dirección Ethereum>` (hex en minúsculas, ver arriba).
- La prueba **EIP-712** del VC debe verificarse contra la **misma** cuenta Ethereum extraída del DID del `issuer`.
- Si el JSON-LD representa `issuer` como **objeto**, la propiedad **`id`** de ese objeto contiene el DID string canónico (no se usa en slice A un `https://…` aislado como único identificador del emisor).

## Verifiable Credential document `id` (slice A)

- El **`id`** del documento VC (identificador único del credencial, distinto de `credentialSubject.id`) usa el esquema **`urn:uuid:`** seguido de un UUID en representación estándar con **hexadecimales en minúsculas** (p. ej. `urn:uuid:xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`).
- No se usa un `https://…` como único identificador del documento en el primer incremento.

## Verifiable Presentation (slice A)

- **`type` (VP, nivel raíz):** el array incluye únicamente **`VerifiablePresentation`** (sin tipo de producto adicional en slice A).
- **`holder`:** **obligatorio**; debe ser el **mismo** DID string que **`credentialSubject.id`** del VC incrustado (coherencia documental además de la prueba EIP-712 del titular).
- **`verifiableCredential`:** exactamente **un** elemento en el array: el **objeto VC completo** incrustado (no solo referencias por `id`, ni varios VCs en este hito).
- **Prueba del titular:** la prueba de que el titular controla la cuenta asociada al VC se hace con **EIP-712** (dominio y tipos dedicados a la presentación), no con `personal_sign` / EIP-191.
- Objetivo cripto: misma familia que la prueba del emisor sobre el VC (coherencia de implementación, tests con vectores, mensajes tipados en wallet).

## Credential profile (Phase 3 slice A, first increment)

- Hasta nuevo acuerdo OpenSpec, el único tipo de credencial académica en alcance es **`TituloGraduacion`** (título de graduación), coherente con el enum `CredentialType` descrito en `openspec/specs/master-design.md`.
- En código .NET, **`CredentialType`** (y equivalentes cerrados compartidos por emisor y verificador) viven en **`SovereignID.SharedKernel.Domain`**, no solo en `bc-issuer`.
- **Claims obligatorios** bajo `credentialSubject` para `TituloGraduacion` (primer incremento, sin nombre legal del graduado en claro): **`degreeTitle`**, **`programName`**, **`awardDate`**. El emisor del VC se expresa en **`issuer`** del documento W3C; no se exige `legalName` ni otros PII adicionales en slice A.
- **`awardDate`:** solo **fecha civil** en formato **`YYYY-MM-DD`** (sin componente hora ni zona horaria en el string almacenado en el VC).
- **`issuanceDate` (VC):** obligatoria; instante en **RFC 3339 con desplazamiento UTC** (sufijo **`Z`**), p. ej. `2026-05-03T14:30:00Z`.
- **`expirationDate` (VC):** **opcional**. Si está ausente, el credencial no se considera caducado por fecha en slice A. Si está presente, mismo formato que `issuanceDate` (instante UTC con `Z`); el verificador rechaza credenciales expiradas cuando el campo existe.
- **`@context` (VC y VC embebido en VP):** array con **(1)** la URL del contexto W3C VC v1 `https://www.w3.org/2018/credentials/v1` y **(2)** un **objeto inline** que define los términos de dominio propios (claims de `TituloGraduacion`, tipo de credencial, etc.). No se exige en slice A un contexto hospedado solo bajo `https://` del producto.
- **`type` (VC, nivel raíz):** incluye **`VerifiableCredential`** y **`TituloGraduacionCredential`** (string canónico para el perfil `TituloGraduacion` / enum en código).

## Document conformance module (slice A, agreed architecture)

- Además del kernel y de **`SovereignID.VcSliceA.Eip712`** (Nethereum), el slice puede incluir un ensamblado **solo de política de documento** (sin Nethereum), p. ej. **`SovereignID.VcSliceA.Document`**, colocado junto al proyecto EIP-712 bajo `src/shared/`.
- Ese **módulo** valida la forma del **VC** `TituloGraduacion` tal como circula en JSON (entrada orientada a **`JsonElement`** / documento parseado), con **paridad literal** con lo que se firma y transporta: incluye **`credentialSubject`** estricto (solo claves permitidas, mismas reglas de claims que emisión) y el **segundo `@context` inline** con el **mismo mapa de términos** (IRIs) que la construcción de documento del emisor — producto cerrado frente a variantes JSON-LD permisibles pero no emitidas.
- **Issuer** y **Verifier** deben ejecutar esa validación en sus flujos (emisor antes de firmar; verificador al aceptar el VC embebido). El **módulo** devuelve **códigos de error canónicos internos** (sin prefijo de bounded context); cada contexto **mapea** a sus propios prefijos (`issuer_*`, `vc_*`, etc.) en el borde de aplicación o API.

## Specification workflow (Phase 3, slice A)

- La implementación del primer incremento **no arranca** hasta tener un cambio OpenSpec en `openspec/changes/<cambio>/` con `proposal.md`, `design.md`, `tasks.md` y las especificaciones acordadas; el código sigue esas tareas.
- **Nombre de carpeta acordado:** `openspec/changes/phase-3-vc-slice-a/`.

Further cross-context terms will be added here when agreed in `/grill-with-docs` or implementation.
