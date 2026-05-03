## Why

El puerto `ISiweMessageParser` hoy devuelve solo `SiweMessage` y el adaptador `ManualSiweMessageParser` convierte fallos en `AuthDomainException`, mientras `VerifySiweCommandHandler` atrapa cualquier `Exception` y los reexpresa como `AuthErrors.SiweParseFailed(ex.Message)`. Eso rompe el contrato: los errores ya estructurados como `AuthError` se pierden o se re-envuelven, y la interfaz del módulo (en el sentido OpenSpec: tipo + modo de error) no coincide con lo que los tests y el handler deben saber. Hace falta alinear el contrato **antes** de añadir más adaptadores de parsing o más superficie HTTP.

## What Changes

- Redefinir el puerto `ISiweMessageParser` para que el resultado del análisis sea **explícito** (éxito con `SiweMessage` o fallo con `AuthError`), alineado con el resto del flujo de verificación SIWE que ya usa `Result<..., AuthError>`.
- Actualizar `ManualSiweMessageParser` para cumplir el nuevo contrato (sin depender de excepciones para el flujo nominal de error de parseo).
- Actualizar `VerifySiweCommandHandler` para propagar el `AuthError` del parser sin un `catch (Exception)` que pise códigos estructurados.
- Actualizar pruebas unitarias del parser, del handler y dobles de prueba (`FakeParser`).
- **BREAKING** para cualquier implementación externa de `ISiweMessageParser` (en el repo solo existe `ManualSiweMessageParser` y fakes de test).

## Capabilities

### New Capabilities

- `auth-siwe-message-parsing`: Requisitos del puerto de análisis de mensajes EIP-4361 en el contexto Auth: resultado explícito, códigos de error observables y coherencia con el modelo `AuthError`.

### Modified Capabilities

- (ninguno) — No se alteran requisitos ya publicados en `openspec/specs/`; el comportamiento observable HTTP del endpoint `/auth/verify` para mensajes mal formados debe seguir siendo 400 con `siwe_parse_failed`, lo cual se mantiene al preservar el mismo `AuthError` en el camino feliz/fallido.

## Impact

- `SovereignID.Auth.Domain` (`ISiweMessageParser`, posiblemente documentación XML del puerto).
- `SovereignID.Auth.Infrastructure` (`ManualSiweMessageParser`).
- `SovereignID.Auth.Application` (`VerifySiweCommandHandler`).
- `SovereignID.Auth.Infrastructure.Tests`, `SovereignID.Auth.Application.Tests`.
- Sin cambios en `Program.cs` salvo que el registro DI requiera ajuste por firma (no se espera).
- Sin cambios en `bc-issuer` / `bc-verifier` / `legacy`.
