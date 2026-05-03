## 1. Domain port

- [x] 1.1 Cambiar `ISiweMessageParser.ParseAsync` para que devuelva `Task<Result<SiweMessage, AuthError>>` y actualizar comentarios XML del puerto (contrato de éxito/fallo explícito).

## 2. Infrastructure adapter

- [x] 2.1 Refactorizar `ManualSiweMessageParser` para devolver `Result.Failure(AuthErrors.SiweParseFailed(...))` en todos los caminos de error de parseo esperados, sin lanzar `AuthDomainException` por esos casos.
- [x] 2.2 Asegurar que los caminos de éxito devuelven `Result.Success` con `SiweMessage` completo (incl. `OriginalPayload`).

## 3. Application handler

- [x] 3.1 Actualizar `VerifySiweCommandHandler` para consumir el `Result` del parser: en fallo, devolver `Result.Failure` con el mismo `AuthError` del parser; eliminar el patrón `try/catch (Exception)` que re-envuelve el fallo de parseo.

## 4. Tests

- [x] 4.1 Actualizar `ManualSiweMessageParserTests` para afirmar sobre `Result` (códigos `siwe_parse_failed`, detalles), no sobre `ThrowsAsync<AuthDomainException>` salvo donde aún aplique un error verdaderamente excepcional no cubierto por la spec.
- [x] 4.2 Actualizar `FakeParser` y todos los tests en `VerifySiweCommandHandlerTests` que dependan de la firma anterior o de `Throws` en el parser.
- [x] 4.3 Añadir o ajustar un test que demuestre que un fallo de parseo con `siwe_parse_failed` atraviesa el handler sin cambiar `Code` ni `Detail`.

## 5. Verificación

- [x] 5.1 Ejecutar `dotnet test` en los proyectos de test de Auth afectados y confirmar verde.
