# Tasks: phase-3-vc-slice-a

> Checklist de implementación. Spec: `specs/vc-slice-a/spec.md` · Diseño:
> `design.md`

## 1. Shared kernel

- [x] 1.1 Añadir `CredentialType` (mínimo `TituloGraduacion`) en
  `SovereignID.SharedKernel.Domain` y referencias de proyecto necesarias.
- [x] 1.2 Implementar parseo/validación de DID `did:ethr:sepolia:0x` + 40 hex
  minúsculas hacia `EthereumAddress` (sin I/O), con tests unitarios en el
  proyecto de tests del shared kernel si existe, o en el BC que lo consuma
  primero si aún no hay test project dedicado.

## 2. Issuer — dominio y aplicación

- [x] 2.1 Modelar errores de dominio y validación de allowlist de claims
  (`degreeTitle`, `programName`, `awardDate` formato fecha) para
  `TituloGraduacion`.
- [x] 2.2 Definir puerto de firma EIP-712 del VC (emisor) y VO/DTO internos
  necesarios para el payload firmable acordado en `design.md`.
- [x] 2.3 Implementar builder/fábrica de documento JSON-LD VC cumpliendo
  `type`, `@context`, `id` urn:uuid, `issuer`, `issuanceDate` / opcional
  `expirationDate`, `credentialSubject`.
- [x] 2.4 Implementar comando + `ICommandHandler` de emisión en Application
  (orquestación: validar entrada → construir VC → firmar vía puerto).

## 3. Issuer — pruebas

- [x] 3.1 Tests unitarios del builder: JSON requerido y rechazo de claims
  extra o DID inválido.
- [x] 3.2 Tests con firmante fake: VC contiene `proof` coherente y pasa
  validación estructural cruzada mínima (sin red).

## 4. Verifier — dominio y aplicación

- [x] 4.1 Validar forma de VP (`type`, `holder`, un solo
  `verifiableCredential` embebido) según spec.
- [x] 4.2 Validar VC embebido (tipos, contexto, fechas con `IClock` o
  abstracción equivalente).
- [x] 4.3 Verificar firma EIP-712 del emisor contra `issuer` y firma EIP-712
  del titular contra `credentialSubject.id`.
- [x] 4.4 Comando + handler de verificación de VP (nombre alineado con el
  código) devolviendo resultado explícito (éxito / código error).

## 5. Verifier — pruebas

- [x] 5.1 Tests: VP válida pasa; firma emisor incorrecta falla; firma holder
  incorrecta falla; `holder` ≠ subject falla; dos VCs en array falla;
  `issuanceDate` futura falla; `expirationDate` pasada falla.

## 6. Cierre del cambio OpenSpec

- [x] 6.1 Actualizar `AGENTS.md` para apuntar el **Current change** a
  `openspec/changes/phase-3-vc-slice-a/tasks.md` mientras este cambio esté
  activo.
- [x] 6.2 Tras tocar código de producción, ejecutar desde la raíz del repo:
  `python -m graphify update .` (AST-only).
