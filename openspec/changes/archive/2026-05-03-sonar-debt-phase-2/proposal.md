# Propuesta: Deuda SonarQube y alineación con Fase 2

## Why

En el análisis del proyecto `SovereingID` en SonarQube quedan issues abiertos
(categorías csharpsquid, reglas external_roslyn) que ruidan en la puerta de
calidad y mezclan **patrones intencionales** (markers de capa, contratos CQRS) con
**deuda corregible** (micro-optimizaciones y APIs concretas en pruebas). Fase
2 (SIWE en `bc-auth`) ya añade superficie de código: cerrar o documentar
explícitamente esos casos ahora mantiene CI legible, reduce falsos positivos
repetidos y fija criterio de equipo antes de crecer `Issuer`/`Verifier`.

## What Changes

- **S2094 (clases `*Marker` vacías)**: Cada ancla de ensamblado tendrá criterio
  explícito: sustituir por un patrón aceptado por el equipo (p. ej. tipo
  mínimo no “vacío”, `file`-scoped con justificación) **o** supresión
  acotada con comentario que cite el propósito (referencia de proyecto /
  escaneo de arquitectura), para que no se confunda con deuda olvidada.
- **S2326 (`ICommand<TResult>` / `IQuery<TResult>`)**: Acordar y aplicar
  un único enfoque: se mantiene el parámetro fantasma para enlace
  command/query → tipo de resultado con los handlers, con **justificación
  y supresión** Sonar en el punto único, **o** refactor a contratos
  alternativos solo si se valida en diseño (evitar hacks solo para “tachar
  el issue”).
- **CA1859 (`ArchitectureRulesTests`)**: Ajuste mecánico de firmas y
  retornos a tipos concretos (`HashSet`, `Dictionary`) en métodos privados
  afectados.
- **CA1845 (`DocumentNotarizer` en `src/legacy/`)**: Refactor mínimo para
  evitar `Substring` en favor de API recomendada (span/concat) en el punto
  señalado, sin reabrir el alcance de Fase 1/legacy.
- **Calidad/CI**: Tras el cambio, el análisis publicado debería reflejar
  cierre o supresión documentada de los issues listados; no añadimos nuevas
  dependencias de producto.

## Capabilities

### New Capabilities

- `sonar-debt-cleanup`: Criterio y requisitos de producto-ingeniería para
  remediar o documentar hallazgos SonarQube/Roslyn identificados (S2094,
  S2326, CA1859, CA1845) sin cambiar el comportamiento observable de SIWE
  ni de la arquitectura por capas.

### Modified Capabilities

- `solution-architecture`: Añadir/ajustar requisitos que fijen el
  **comportamiento esperado** de los contratos `ICommand`/`IQuery` en
  SharedKernel y de los **anclas de ensamblado** usados en tests de
  arquitectura, de modo que la remediación Sonar esté alineada con el spec
  (y no quede en “sólo comentario en el código”).

## Impact

- Código: `SovereignID.SharedKernel.Application` (CQRS abstractions), todos
  los `*Marker.cs` de capas, `SovereignID.Architecture.Tests`,
  `SovereignID.Chain/DocumentNotarizer`.
- Fase 2: Sin nuevos endpoints; solo mantiene la barra de calidad mientras
  se implementa `phase-2-siwe-auth`.
- SonarQube: Cierre o supresión con motivo en issues existentes; posible
  ajuste de perfiles/ exclusiones **solo** si el diseño lo documenta
  (preferencia: arreglo en código primero).
