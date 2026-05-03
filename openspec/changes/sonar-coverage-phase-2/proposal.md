# Propuesta: Ingestión de cobertura y Quality Gate de SonarQube (Fase 2)

## Why

SonarCloud reporta hoy **0.0 % de cobertura** sobre `Skiiinnny_SovereingID`
(sólo 114 líneas "cubribles" indexadas, todas en `src/legacy/`), aun cuando
el repositorio ya contiene más de una decena de proyectos de test con casos
de unidad e integración in-process (WebApplicationFactory). El motivo es
mecánico: el workflow `sonarqube.yml` construye y analiza, pero **no
ejecuta `dotnet test` ni ingiere un reporte de cobertura** en el scanner,
y sólo `SovereignID.Architecture.Tests` tiene `coverlet.collector`
referenciado. El Quality Gate por defecto (`Sonar way`) evalúa
`new_coverage < 80` como error, pero hoy pasa "OK" de forma vacía porque
no hay new lines to cover detectadas — es un verde falso que se romperá
en cuanto haya un PR de negocio real. Además, los tres workflows
(`ci.yml`, `dotnet.yml`, `sonarqube.yml`) se solapan y reconstruyen la
solución hasta 3 veces por PR sin compartir resultados. Antes de
continuar con `bc-issuer`/`bc-verifier` conviene fijar la tubería de
cobertura, la política de exclusión (legacy, markers, hosting trivial) y
un Quality Gate alineado con la arquitectura por bounded context.

## What Changes

- **Quality Gate propio** `SovereignID default` (clonado de `Sonar way`)
  con condiciones: `new_coverage ≥ 80 %`, `coverage ≥ 70 %` overall, más
  las heredadas de `Sonar way` (ratings A, duplicación ≤ 3 %, hotspots
  revisados 100 %). Se asigna al proyecto `Skiiinnny_SovereingID`.
- **Enforcement por BC** (lectura estricta del 70 %): un paso de CI
  procesa el reporte Cobertura y falla el job si **cualquier** bounded
  context (`bc-auth`, `bc-issuer`, `bc-verifier`, `shared`) queda por
  debajo del 70 % line coverage, independientemente del promedio
  proyecto-nivel que mira Sonar.
- **Ingestión de cobertura en Sonar**: el scanner recibe
  `sonar.cs.opencover.reportsPaths` apuntando a los XML OpenCover
  generados por Coverlet en todos los proyectos de test. Se habilita
  `sonar.qualitygate.wait=true` para bloquear el PR si el gate falla.
- **Generación de reportes duales** (OpenCover + Cobertura) mediante un
  `coverlet.runsettings` único en la raíz, consumido por todos los
  `dotnet test`. OpenCover alimenta Sonar (ramas precisas), Cobertura
  alimenta artifacts/badges/GitHub UI.
- **Coverlet uniforme**: `Directory.Build.props` en `tests/` inyecta
  `coverlet.collector` a todos los proyectos de test; se elimina la
  referencia duplicada en `SovereignID.Architecture.Tests.csproj`.
- **Política de exclusión de cobertura**:
  - `src/legacy/**` excluido como *congelado* (política de Fase 1).
  - `src/bc-auth/SovereignID.Auth.Api/wwwroot/**` excluido del análisis
    completo (frontend JS fuera de scope por ahora).
  - Hosting trivial y DTOs marcados con `[ExcludeFromCodeCoverage]`:
    `Program.cs`, `SystemClock`, `GuidGenerator`, `AuthOptions`, records
    en `Contracts/`, `*Marker.cs`, `CqrsAbstractions.cs`.
- **Consolidación de workflows**: `dotnet.yml` se elimina. `ci.yml`
  queda como job `build-test` (ubuntu-latest) que produce reportes de
  cobertura como artifact. `sonarqube.yml` queda como job `sonar-scan`
  (windows-latest) que consume el artifact y corre el scanner begin→end
  sin reconstruir tests.
- **Convención explícita de `[Trait("Category","Integration")]`**: se
  documenta que el trait aplica **sólo** a tests que tocan red/I/O
  externo real (Sepolia RPC, contratos on-chain). Los 7 casos de
  `SovereignID.Auth.IntegrationTests` (WebApplicationFactory in-process,
  sin red) **no** llevan el trait y cuentan para cobertura en CI. El
  nombre del proyecto se mantiene por conveniencia histórica, con nota
  aclaratoria en README.
- **Umbrales locales para desarrolladores**: `coverlet.runsettings`
  incluye `Threshold=70`, `ThresholdType=line,branch,method`,
  `ThresholdStat=total` para que `dotnet test --settings
  coverlet.runsettings` avise en local antes de subir a CI.

## Capabilities

### New Capabilities

- `test-coverage-ingestion`: Requisitos de producto-ingeniería para que
  las pruebas automatizadas de SovereignID alimenten métricas de
  cobertura reales a SonarCloud, con exclusiones documentadas,
  enforcement por bounded context y un Quality Gate propio. Cubre la
  generación de reportes duales (OpenCover + Cobertura), la política de
  exclusión de legacy/wwwroot/hosting, la consolidación de los
  workflows CI/Sonar y el criterio explícito `Integration = red/I/O
  externo` para futuros tests.

### Modified Capabilities

<!-- Ninguna. El cambio no modifica requisitos de `solution-architecture`
     ni de `sonar-debt-cleanup`; se añade ortogonalmente. -->

## Impact

- **Código de producto** (sólo atributos `[ExcludeFromCodeCoverage]`, sin
  cambio de comportamiento): `SovereignID.Auth.Api` (`Program.cs`,
  `Infrastructure/SystemClock.cs`, `Infrastructure/GuidGenerator.cs`,
  `Configuration/AuthOptions.cs`, `Contracts/*.cs`), los `*Marker.cs`
  de `bc-auth`, `bc-issuer`, `bc-verifier`, `shared`,
  `CqrsAbstractions.cs`.
- **Proyectos de test**: `Directory.Build.props` nuevo en `tests/`
  inyecta `coverlet.collector`. Se elimina la referencia duplicada en
  `SovereignID.Architecture.Tests.csproj`.
- **Raíz del repo**: nuevo `coverlet.runsettings`.
- **CI/CD**: `ci.yml` reescrito, `sonarqube.yml` reescrito,
  `dotnet.yml` eliminado.
- **SonarCloud**: nuevo Quality Gate `SovereignID default` asignado al
  proyecto. La asignación y creación del gate es **manual vía UI de
  SonarCloud** (o API); se documenta en `design.md` y `tasks.md` pero
  no se automatiza en este change.
- **Documentación**: `README.md` añade sección "Run tests with
  coverage" y aclara la convención de `[Trait("Category","Integration")]`.
- **Out of scope**: tests nuevos para cubrir gaps reales sólo si el
  primer reporte real muestra que algún BC queda < 70 %; en ese caso
  se crea un task adicional dentro de este change, pero no se
  anticipan en la propuesta para evitar inventar casos sin datos.
- **Phase 2 SIWE (`phase-2-siwe-auth`)**: sin impacto funcional.
  Se apoya al cierre de esa fase proporcionando cobertura medible para
  los endpoints y handlers ya entregados.
- **Sonar debt (`sonar-debt-phase-2`)**: complementa pero no duplica.
  Ese change cierra reglas de estilo (S2094, S2326, CA1859, CA1845);
  éste habilita la métrica de cobertura.
