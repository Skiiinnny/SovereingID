## Context

Phase 2 entregó `ISiweMessageParser` con `Task<SiweMessage> ParseAsync(...)`. `ManualSiweMessageParser` atrapa errores de formato y lanza `AuthDomainException` con `AuthErrors.SiweParseFailed`. `VerifySiweCommandHandler` envuelve la llamada en `try/catch (Exception)` y vuelve a construir `SiweParseFailed` desde `ex.Message`, de modo que `AuthDomainException` (subtipo de `Exception`) nunca llega al branch que preservaría `ex.Error`. El contrato real del puerto mezcla éxito por valor, fallo por excepción de dominio y fallo por excepción genérica, lo que dispersa el conocimiento entre infraestructura y aplicación.

## Goals / Non-Goals

**Goals:**

- Un único modo de error **declarado** en el puerto, alineado con CQRS existente (`Result<..., AuthError>`).
- Que el handler de verificación SIWE no re-envuelva errores ya modelados como `AuthError`.
- Mantener códigos HTTP y extensiones actuales en `AuthEndpoints` (sigue dependiendo del `AuthError` devuelto por el handler).

**Non-Goals:**

- Sustituir `ManualSiweMessageParser` por Nethereum.Templates.Siwe u otro motor de parsing.
- Añadir nuevos códigos `AuthError` granulares (p. ej. por cada línea EIP-4361) salvo que una tarea explícita lo pida después.
- Cambiar el contrato de `ISiweSignatureVerifier`, repositorio de retos o emisión JWT.

## Decisions

1. **Puerto devuelve `Task<Result<SiweMessage, AuthError>>`**

   - **Rationale:** Misma forma que `VerifySiweCommandHandler`; la interfaz del módulo documenta el modo de error sin leer el cuerpo del adaptador; tests cruzan el mismo costado que el handler.
   - **Alternativa descartada:** Ordenar `catch (AuthDomainException)` antes de `catch (Exception)` manteniendo `Task<SiweMessage>` — sigue dependiendo de excepciones para flujo de control y deja el contrato del puerto incompleto respecto a `AuthError`.

2. **`ManualSiweMessageParser` devuelve `Failure(SiweParseFailed(...))` en lugar de lanzar para fallos de parseo**

   - **Rationale:** Las excepciones internas (`FormatException`, etc.) pueden seguir usándose dentro del adaptador y mapearse a `AuthError` en un solo lugar al borde del método.
   - **Alternativa:** Lanzar `AuthDomainException` y capturar solo en el handler — rechazada por duplicar manejo y por el riesgo de capturas amplias.

3. **Fakes de test (`FakeParser`) devuelven `Result`**

   - **Rationale:** Los tests del handler pueden simular fallo de parseo sin lanzar excepciones arbitrarias que ya no formen parte del contrato.

## Risks / Trade-offs

- **[Riesgo] Cambio de firma del puerto** → Mitigación: solo hay un adaptador productivo y fakes en tests; documentar **BREAKING** en proposal.
- **[Riesgo] Errores inesperados (bugs) en el parser** → Mitigación: el handler puede conservar un `catch` muy acotado para defectos no modelados *solo si* el equipo lo desea; el diseño preferido es que el parser no lance en el camino nominal; bugs se detectan por tests y logs sin mezclarlos con `siwe_parse_failed` si se decide propagar `500` distinto (fuera de alcance mínimo: la spec actual se centra en fallos de parseo esperados).

## Migration Plan

1. Cambiar firma del puerto e implementación.
2. Actualizar handler y tests hasta verde.
3. Ejecutar `dotnet test` en solución o al menos proyectos `Auth.*`.
4. No hay datos ni despliegue que migrar.

## Open Questions

- ¿Debe el handler tratar explícitamente excepciones no previstas del parser (defensa en profundidad) y mapearlas a un error genérico interno, o dejar que suban como 500? Resolver al implementar según política de robustez del API.
