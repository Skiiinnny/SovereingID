# Tasks: sonar-coverage-phase-2

> SovereignID · openspec/changes/archive/2026-05-04-sonar-coverage-phase-2/tasks.md
> Estimated: 1 week · 4-8 hrs total
> Order: gate → coverage config → exclusions → CI → Sonar → docs → verify

---

## 1. Quality Gate en SonarCloud (plan gratuito → `Sonar way`)

> En SonarCloud **Free** no se pueden crear ni clonar Quality Gates
> personalizados (p. ej. aparece **Upgrade** junto a gates no built-in).
> Este proyecto usa el gate built-in **`Sonar way`**, que incluye cobertura
> suficiente en **código nuevo** (típicamente **≥ 80 %** en new code). El
> piso **≥ 70 % por bounded context** y el umbral Coverlet local siguen
> aplicándose **solo en CI**, no como condición extra en Sonar.
>
> Checklist: [`docs/onboarding.md`](../../../docs/onboarding.md) → **SonarCloud quality gate (`Sonar way`, free plan)**.

- [x] 1.1 En SonarCloud → organización `skiiinnny` → **Quality Gates**,
  revisar que **`Sonar way`** es el gate **Built-in** / **Default** y
  anotar la condición de **Coverage on new code** (p. ej. ≥ 80 %).
- [x] 1.2 Proyecto **`Skiiinnny_SovereingID`** → **Project settings** →
  **Quality Gate**: confirmar que el proyecto está en **`Sonar way`**
  (o equivalente “default / no custom” permitido por el plan).
- [x] 1.3 (Recomendado) Captura de la asignación del proyecto o de las
  condiciones visibles de **`Sonar way`**, en el PR de verificación (p. ej.
  `<details>` en la descripción).

## 2. Coverlet uniforme

- [x] 2.1 Crear `coverlet.runsettings` en la raíz del repo con:
  - `Format=opencover,cobertura`
  - `DeterministicReport=false` (OpenCover + `true` is unsupported by Coverlet)
  - `SingleHit=false`
  - `Threshold=70`, `ThresholdType=line,branch,method`, `ThresholdStat=total`
  - `ExcludeByAttribute`: `ExcludeFromCodeCoverageAttribute`,
    `GeneratedCodeAttribute`, `CompilerGeneratedAttribute`, `Obsolete`
  - `ExcludeByFile`: `**/Program.cs`, `**/*.g.cs`, `**/obj/**`, `**/bin/**`
  - `Exclude` (assemblies): `[*SovereignID.Crypto*]*`,
    `[*SovereignID.Chain*]*`, `[*SovereignID.Demo.Phase1*]*`,
    `[*]*Marker`
- [x] 2.2 Crear `tests/Directory.Build.props` con
  `<PackageReference Include="coverlet.collector" Version="6.0.2"
  PrivateAssets="all" />` dentro de un `<ItemGroup
  Condition="'$(IsTestProject)' == 'true'">` (los `.csproj` de test ya
  activan `IsTestProject` vía `Microsoft.NET.Test.Sdk`).
- [x] 2.3 Eliminar la referencia explícita a `coverlet.collector` en
  `tests/architecture/SovereignID.Architecture.Tests/SovereignID.Architecture.Tests.csproj`
  (queda heredada del Directory.Build.props).
- [x] 2.4 Ejecutar en local:
  `dotnet test SovereignID.sln --filter "Category!=Integration" --settings coverlet.runsettings --results-directory coverage`.
- [x] 2.5 Verificar que aparecen `coverage.opencover.xml` y
  `coverage.cobertura.xml` bajo `coverage/**` por cada proyecto de test
  ejecutado.

## 3. Exclusiones en código (`[ExcludeFromCodeCoverage]`)

- [x] 3.1 Añadir `[ExcludeFromCodeCoverage]` a nivel de tipo en
  `src/bc-auth/SovereignID.Auth.Api/Program.cs`
  (en `public partial class Program`).
- [x] 3.2 Añadir `[ExcludeFromCodeCoverage]` a
  `src/bc-auth/SovereignID.Auth.Api/Infrastructure/SystemClock.cs`.
- [x] 3.3 Añadir `[ExcludeFromCodeCoverage]` a
  `src/bc-auth/SovereignID.Auth.Api/Infrastructure/GuidGenerator.cs`.
- [x] 3.4 Añadir `[ExcludeFromCodeCoverage]` a
  `src/bc-auth/SovereignID.Auth.Api/Configuration/AuthOptions.cs`.
- [x] 3.5 Añadir `[ExcludeFromCodeCoverage]` a cada record en
  `src/bc-auth/SovereignID.Auth.Api/Contracts/`
  (`VerifyRequest`, `VerifyResponse`, `NonceResponse`).
- [x] 3.6 Añadir `[ExcludeFromCodeCoverage]` a
  `src/bc-auth/SovereignID.Auth.Infrastructure/Configuration/AuthOptions.cs`.
- [x] 3.7 Añadir `[ExcludeFromCodeCoverage]` a
  `src/shared/SovereignID.SharedKernel.Application/CqrsAbstractions.cs`
  (compatible con la supresión ya aplicada en `sonar-debt-phase-2`).
- [x] 3.8 Confirmar que los `*Marker.cs` de `bc-auth`, `bc-issuer`,
  `bc-verifier`, `shared` ya quedan excluidos por el filtro
  `[*]*Marker` del runsettings. Si alguno no empata el patrón, añadir
  el atributo a mano.
- [x] 3.9 Compilar solución completa (`dotnet build SovereignID.sln`)
  sin warnings nuevos.
- [x] 3.10 Re-ejecutar el `dotnet test` de §2.4 y validar que
  `linesToCover` sobre los archivos de §3.1–3.7 es **0** en el reporte
  Cobertura.

## 4. Script de enforcement per-BC

- [x] 4.1 Crear carpeta `ci/` en la raíz si no existe.
- [x] 4.2 Crear `ci/Check-BcCoverage.ps1` con la lógica descrita en
  spec `test-coverage-ingestion` requirement *"Per-bounded-context
  coverage floor is enforced in CI"*.
  - Parámetros: `-CoberturaPath <string>`, `-Threshold <int = 70>`.
  - Mapea prefijos de assembly → BCs (`SovereignID.Auth.*` → `bc-auth`,
    `SovereignID.Issuer.*` → `bc-issuer`,
    `SovereignID.Verifier.*` → `bc-verifier`,
    `SovereignID.SharedKernel.*` → `shared`).
  - Ignora `SovereignID.Crypto.*`, `SovereignID.Chain.*`,
    `SovereignID.Demo.Phase1`.
  - Imprime una tabla `BC | covered / coverable | % | status` al final.
  - Exit code != 0 si algún BC con líneas cubribles > 0 queda <
    `Threshold`.
- [x] 4.3 Probar el script en local contra el reporte Cobertura
  generado en §2.4. Debe listar `bc-auth` y `shared` con sus
  porcentajes reales.
- [x] 4.4 Probar el branch "BC vacío": simular un reporte sin clases
  `SovereignID.Issuer.*` y verificar que el script imprime un warning
  ("no coverable lines") y sale con exit 0.

## 5. Reescribir `ci.yml`

- [x] 5.1 Reemplazar el contenido de `.github/workflows/ci.yml` con un
  job `build-test` (runs-on: `ubuntu-latest`):
  - `actions/checkout@v4`
  - `actions/setup-dotnet@v4` (`dotnet-version: 9.0.x`)
  - `dotnet restore SovereignID.sln`
  - `dotnet build SovereignID.sln -c Release --no-restore`
  - `dotnet test SovereignID.sln -c Release --no-build --filter "Category!=Integration" --settings coverlet.runsettings --results-directory coverage --logger trx`
  - Paso PowerShell (`shell: pwsh`) que invoca
    `./ci/Check-BcCoverage.ps1 -CoberturaPath "coverage/**/coverage.cobertura.xml" -Threshold 70`.
  - `actions/upload-artifact@v4` con `name: coverage`, `path: coverage/`,
    `retention-days: 7`.
- [x] 5.2 Comprobar que `ci.yml` se dispara en `push` a `main` y
  `pull_request` → `main`.

## 6. Reescribir `sonarqube.yml`

- [x] 6.1 Añadir `needs: build-test` **no**: mantener workflow
  separado porque son triggers distintos, pero hacer que el job
  principal descargue el artifact `coverage` del mismo SHA mediante
  `actions/download-artifact@v4` con `github-token` o vía
  `dawidd6/action-download-artifact@v3` (permite `run_id` del
  workflow `ci.yml` correspondiente al mismo commit).
- [x] 6.2 Ajustar el paso "Build and analyze" de `sonarqube.yml`:
  - Tras descargar el artifact, pasar al `scanner begin`:
    - `/d:sonar.cs.opencover.reportsPaths="coverage/**/coverage.opencover.xml"`
    - `/d:sonar.exclusions="src/bc-auth/SovereignID.Auth.Api/wwwroot/**,src/legacy/**"`
    - `/d:sonar.coverage.exclusions="src/legacy/**,**/*Marker.cs,**/Program.cs"`
  - En el `scanner begin`, añadir `/d:sonar.qualitygate.wait=true`
    (el plugin C# no admite esta propiedad en `scanner end`).
- [x] 6.3 Mantener el paso *"Build only (fork PR / no token)"* sin
  cambios para PRs desde forks.
- [x] 6.4 Quitar cualquier otra invocación a `dotnet test` dentro de
  este workflow (el job sólo construye y analiza).

## 7. Eliminar workflow redundante

- [x] 7.1 Borrar `.github/workflows/dotnet.yml`.
- [x] 7.2 Verificar que ningún status check requerido en
  branch-protection apunta a `dotnet.yml`. Si lo hace, actualizar la
  protección para requerir `ci.yml` en su lugar (paso manual en GitHub
  settings; documentar en PR).

## 8. Documentación

- [x] 8.1 Añadir sección `### Run tests with coverage` al `README.md`
  con:
  - Comando exacto (§2.4).
  - Dónde se generan los reportes (`coverage/**`).
  - Mención a que Coverlet falla localmente si algún proyecto queda <
    70 %.
- [x] 8.2 Añadir sección `### Integration test convention` al
  `README.md` explicando que:
  - `[Trait("Category","Integration")]` cubre sólo red / I/O externo.
  - Los 7 `AuthEndpointsTests` in-process **no** llevan el trait,
    aunque el proyecto se llame `SovereignID.Auth.IntegrationTests`.
- [x] 8.3 Añadir sección `### CI / SonarQube pipeline` al `README.md`
  describiendo los dos workflows (`ci.yml`, `sonarqube.yml`) y la
  existencia del script `ci/Check-BcCoverage.ps1`.
- [x] 8.4 No modificar `AGENTS.md` en este change. La convención
  Integration puede entrar en `AGENTS.md` sólo cuando el change esté
  archivado, por la regla del workflow experimental.

## 9. Verificación end-to-end

- [x] 9.1 Crear rama `feature/sonar-coverage-phase-2` y empujar los
  cambios de §2 a §8.
- [x] 9.2 Abrir PR a `main`. Confirmar en el run:
  - `ci.yml` verde, con tabla per-BC en el log mostrando
    `bc-auth ≥ 70 %` y `shared ≥ 70 %` (o warnings para BCs vacíos).
  - Artifact `coverage` adjuntado al workflow run.
- [x] 9.3 Confirmar en el mismo PR que `sonarqube.yml`:
  - Descarga el artifact correctamente.
  - Pasa al scanner `begin` las 3 propiedades Sonar (paths,
    exclusions, coverage.exclusions) y `sonar.qualitygate.wait=true`.
  - El paso "Build and analyze" devuelve exit 0.
- [x] 9.4 Verificar en SonarCloud que tras el merge el proyecto
  reporta:
  - `coverage` > 0 %.
  - `tests` con un número > 0.
  - Quality Gate **`Sonar way`** (o el default del plan) asignado y estado `OK`.
- [x] 9.5 Abrir un PR de prueba sintético que elimine un test
  significativo (p. ej. `HappyPath_ReturnsJwtAndAddress`). Verificar
  que `ci.yml` o el gate Sonar lo rechazan. Cerrar el PR sin mergear.

## 10. Cierre

- [x] 10.1 Marcar como completas todas las tasks anteriores.
- [x] 10.2 Confirmar que `openspec validate sonar-coverage-phase-2 --strict --type change` pasa (sin errores de schema).
- [x] 10.3 Cuando CI y Sonar publiquen métricas estables durante al
  menos un ciclo de PR → main, ejecutar el workflow de archivo del
  change (ver `.cursor/skills/openspec-archive-change/SKILL.md`).
