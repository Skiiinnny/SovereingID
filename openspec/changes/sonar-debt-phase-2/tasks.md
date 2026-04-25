## 1. SharedKernel: CQRS markers (S2326)

- [x] 1.1 Añadir comentario XML a `ICommand<TResult>` e `IQuery<TResult>` explicando el enlace con los handlers, en línea con el delta de `solution-architecture`.
- [x] 1.2 Aplicar supresión explícita para `csharpsquid:S2326` (o equivalente) **solo** en `src/shared/SovereignID.SharedKernel.Application/CqrsAbstractions.cs`, con comentario que cita el requisito.
- [x] 1.3 Compilar y ejecutar pruebas que toquen handlers CQRS (Application tests existentes) para asegurar cero regresión.

## 2. Anclas de ensamblado `*Marker` (S2094)

- [x] 2.1 Añadir documentación XML a cada `*Marker` en `src/bc-auth/`, `src/bc-issuer/`, `src/bc-verifier/` y `src/shared/SovereignID.SharedKernel.Infrastructure/`, descrita en el spec “assembly anchor”.
- [x] 2.2 Añadir supresión o pragma acotada para `csharpsquid:S2094` en esos archivos, con el mismo criterio que 2.1.
- [x] 2.3 Revisar que el texto de supresión sea local (por tipo o archivo), sin desactivar la regla en todo el proyecto.

## 3. Pruebas de arquitectura (CA1859)

- [x] 3.1 Actualizar `tests/architecture/SovereignID.Architecture.Tests/ArchitectureRulesTests.cs`: parámetros y retornos afectados por `CA1859` — usar `HashSet<string>` y `Dictionary<...>` concretos en métodos privados, preservando intención de inmutabilidad hacia el exterior.
- [x] 3.2 Ejecutar `dotnet test` solo del proyecto de arquitectura y luego de la solución.

## 4. Legacy: `DocumentNotarizer` (CA1845)

- [x] 4.1 Refactorizar en `src/legacy/SovereignID.Chain/DocumentNotarizer.cs` la ruta señalada: sustituir `Substring` por construcción compatible con `CA1845` (p. ej. `ReadOnlySpan` / APIs recomendadas) sin alterar el comportamiento observable ni la firma pública.
- [x] 4.2 Ejecutar pruebas bajo `tests/legacy/` que cubran o dependan de `SovereignID.Chain`.

## 5. Verificación y cierre

- [x] 5.1 `dotnet test SovereignID.sln` y corregir cualquier regresión.
- [ ] 5.2 Tras el siguiente análisis publicado en SonarQube, comprobar que los issues afectados figuran resueltos o como supresión aceptada según el equipo, y alinear nombres de reglas si el servidor difiere (Community vs. Enterprise). *(Seguimiento manual tras CI; criterio: csharpsquid S2094/S2326, external_roslyn CA1859/CA1845 en el proyecto `Skiiinnny_SovereingID`.)*
