# Diseño: sonar-debt-phase-2

## Context

SonarQube reporta issues abiertos que mezclan: (1) reglas de estilo sobre
clases ancla vacías (`S2094`); (2) parámetros genéricos “no usados” en
`ICommand<TResult>` / `IQuery<TResult>` (`S2326`); (3) sugerencias Roslyn
`CA1859` en pruebas de arquitectura; (4) `CA1845` en `DocumentNotarizer`
(legacy). La Fase 2 (SIWE) avanza en paralelo; la remediación no debe
alterar contratos HTTP ni el modelo de dominio de `bc-auth`.

**Estado actual (referencia):** `CqrsAbstractions.cs` declara interfaces
de marcador genéricas sin miembros; múltiples `*Marker.cs` son
`sealed class` mínimas; `ArchitectureRulesTests` expone `ISet` / diccionarios
de solo lectura donde Roslyn sugiere tipos concretos; `DocumentNotarizer`
usa `Substring` en una ruta de validación on-chain.

## Goals / Non-Goals

**Goals**

- Cerrar o documentar **de forma trazable** cada issue alineado con un
  requisito de spec (nada de supresión “global” opaca).
- Mantener **cero cambios** en el comportamiento observable: mismos
  resultados de tests, mismas firmas públicas de producto (incl. ensamblados
  legacy salvo acuerdo explícito en spec).
- Dejar criterio repetible para **nuevas** capas (`Issuer`/`Verifier`)
  y futuros PR bajo análisis estático.

**Non-Goals**

- Sustituir el estilo CQRS “marcador + handler” por MediatR u otro bus.
- Reescribir módulos legacy fuera de lo mínimo para `CA1845` en el punto
  señalado.
- Tocar la pipeline de Sonar (tokens, nombres de proyecto) salvo ajuste
  documentado si el análisis remoto deja de coincidir con nombres locales.

## Decisions

1. **S2094 (`*Marker` vacías)**  
   - **Decisión:** Mantener el patrón de ancla y aplicar en cada archivo
     un comentario de intención + supresión **a nivel de tipo o archivo**
     para `csharpsquid:S2094` (o equivalente Sonar/IDE), con texto que
     enlace a `solution-architecture` (ancla de ensamblado).  
   - **Alternativas descartadas:** Sustituir por `interface` vacía
     (rompe el nombre convencionado y no siempre mejora el análisis);
     eliminar markers (rompe referencias de proyecto o tests de
     arquitectura); añadir miembros “placeholder” (ruido y confusión).

2. **S2326 (genérico `TResult` en `ICommand` / `IQuery`)**  
   - **Decisión:** Tratarlo como enlace de tipos **a nivel de constraints**
     en `ICommandHandler<,>` / `IQueryHandler<,>`. Añadir en el spec la
     norma explícita; en código, una supresión **focal** en
     `CqrsAbstractions.cs` con comentario que cita el requisito modificado.  
   - **Alternativas descartadas:** Eliminar `TResult` de los markers
     (pierde correlación común de CQRS con genéricos); añadir miembros
     ficticios solo para el analizador (deuda tácita).

3. **CA1859 (tests de arquitectura)**  
   - **Decisión:** Ajustar tipos a `HashSet<string>` y
     `Dictionary<...>` donde hoy se usan abstracciones de solo lectura
     en métodos privados, previa verificación de que no se expone
     mutabilidad indebida al resto de tests.  
   - **Alternativas descartadas:** Desactivar CA1859 a nivel de proyecto
     (demasiado amplio).

4. **CA1845 (`DocumentNotarizer`)**  
   - **Decisión:** Sustituir el troceo por `Substring` con construcción
     basada en `ReadOnlySpan<char>` o API recomendada, preservando el
     contenido lógico del `string` y la comparación con el hash esperado.
   - **Rationale:** Cumple Roslyn sin alterar API pública; el spec de
     “legacy aislado” alude a firmas públicas estables, no a
     congelar implementación interna sin motivo.

## Risks / Trade-offs

- [Supresión mal usada] → Otras violaciones reales se ocultan. **Mitigación:**
  comentario + regla/ID explícito; code review; listado de issues Sonar
  resueltos en `tasks.md`.
- [Cambio sutil en hex / encoding] al tocar `DocumentNotarizer` → fallo
  silencioso. **Mitigación:** correr pruebas legacy afectadas y, si
  existen, tests de integración que toquen notarización.
- [Desalineación con analysis remoto] (branch/caché). **Mitigación:** no
  usar `search_sonar_issues` como verificación inmediata post-cambio;
  confiar en análisis local/CI y documentar el cierre en la siguiente
  publicación al servidor.

## Migration Plan

1. Implementar según `tasks.md` (orden: SharedKernel, markers, tests,
   legacy, verificación).  
2. `dotnet test` en la solución.  
3. Revisar dashboard SonarQube en la siguiente pasada de CI; cerrar
   issues o actualizar supresiones según constancia en el repositorio.

## Open Questions

- Ninguna asumida. Si en revisión se prefiere **cero** supresiones
  Sonar, habría que reabrir el diseño con un patrón alternativo para
  `ICommand`/`IQuery` (impacto en todos los BC) antes de implementar.
