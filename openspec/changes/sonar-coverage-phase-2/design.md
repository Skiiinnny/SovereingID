# Diseño: sonar-coverage-phase-2

## Context

**Estado actual observado (scan 2026-04-20, Skiiinnny_SovereingID en
SonarCloud)**:

| Métrica                        | Valor                  |
|--------------------------------|------------------------|
| `coverage`                     | **0.0 %**              |
| `line_coverage`                | 0.0 %                  |
| `ncloc`                        | 456                    |
| `uncovered_lines`              | 114 (sólo legacy)      |
| `tests` (métrica)              | no reportada           |
| Quality Gate aplicado          | `Sonar way` (default)  |
| Estado del gate                | `OK` (pero `new_coverage` N/A) |
| `code_smells` abiertos         | 16                     |
| `bugs` / `vulnerabilities`     | 0 / 0                  |

Sólo 7 archivos tienen `linesToCover > 0` y todos viven en
`src/legacy/`. Esto ocurre porque `sonarqube.yml` ejecuta
`dotnet-sonarscanner begin → dotnet build → end` pero **no corre
`dotnet test`** y **no pasa `sonar.cs.*.reportsPaths`**. El único
proyecto de test con `coverlet.collector` referenciado es
`SovereignID.Architecture.Tests`, lo cual no ayuda porque los tests no
se ejecutan en el pipeline Sonar.

**Tests existentes (fuente de cobertura futura, una vez se ingiera
reporte)**:

- `SovereignID.Auth.Domain.Tests` — `Nonce`, `ChainId`,
  `AuthChallenge`.
- `SovereignID.Auth.Application.Tests` —
  `GenerateNonceQueryHandler`, `VerifySiweCommandHandler` (happy +
  todos los códigos de error).
- `SovereignID.Auth.Infrastructure.Tests` — parser manual EIP-4361,
  generador de nonce seguro, emisor JWT, repositorio in-memory,
  verificador de firma Nethereum.
- `SovereignID.Auth.IntegrationTests` — 7 casos end-to-end
  in-process vía `WebApplicationFactory<Program>`, sin red.
- `SovereignID.Architecture.Tests` — reglas de arquitectura.
- `SovereignID.Crypto.Tests` / `SovereignID.Chain.Tests` — legacy;
  algunos `[Integration]` para Sepolia.

**Workflows actuales (duplicación)**: `ci.yml`, `dotnet.yml`,
`sonarqube.yml`. Los tres hacen `dotnet restore + build` y dos
ejecutan tests sin coverage. Sonar reconstruye aparte.

**Change relacionado en curso**: `sonar-debt-phase-2` cierra reglas
de estilo (S2094, S2326, CA1859, CA1845). Este change es ortogonal y
complementa: habilita la métrica de **cobertura** sin tocar esas
reglas.

## Goals / Non-Goals

**Goals**

- Que Sonar publique una métrica de cobertura real (≠ 0) en el próximo
  análisis publicado tras aplicar este change.
- Cumplir el gate `SovereignID default` (custom): `new_coverage ≥ 80 %`
  y `coverage ≥ 70 %` a nivel proyecto.
- Garantizar que **cada** bounded context no-legacy
  (`shared`, `bc-auth`, `bc-issuer`, `bc-verifier`) quede ≥ 70 % line
  coverage mediante verificación en CI (no sólo el promedio
  proyecto).
- Mantener CI rápida: un solo build por PR, tests y coverage en el
  mismo job, scanner reutilizando el resultado (artifact) sin
  recompilar.
- Política de exclusión **trazable** por archivo/atributo, no
  "coverage off" global.
- Convención `[Trait("Category","Integration")]` documentada y
  estable (I/O externo real = sí; WebApplicationFactory in-process =
  no).

**Non-Goals**

- Escribir tests nuevos para legacy (`SovereignID.Chain`,
  `SovereignID.Crypto`, `SovereignID.Demo.Phase1`). Queda excluido
  por política de Fase 1 congelada.
- Cobertura del frontend `wwwroot/*.js`. Queda fuera del análisis;
  entraría con tests Playwright en un change futuro.
- Renombrar `SovereignID.Auth.IntegrationTests` → otro nombre. El
  nombre queda; la convención se aclara en README.
- Tocar la selección de runner de Sonar (windows-latest) o mover el
  scanner a Linux. Se mantiene la topología actual.
- Reescribir el spec `solution-architecture` ni `sonar-debt-cleanup`.
  Son requisitos estables de otros changes.
- Métricas de calidad distintas a cobertura (mutation testing,
  complexity thresholds). Fuera de scope.

## Decisions

### 1. Gate propio `SovereignID default` — proyecto-wide + per-BC

**Decisión:** Crear un Quality Gate custom en SonarCloud llamado
`SovereignID default`, clonado de `Sonar way`, con **una condición
adicional** sobre coverage overall (`coverage ≥ 70 %`) además de la
existente `new_coverage ≥ 80 %`. El gate se asigna al proyecto
`Skiiinnny_SovereingID`.

Sobre el **enforcement por bounded context** (el 70 % "por BC" que
pidió el producto): Sonar no permite condiciones por directorio en un
gate. Se implementa **fuera de Sonar** con un paso de CI que parsea
el reporte Cobertura consolidado y falla si algún BC queda bajo
70 %. Este paso vive en `ci.yml` **antes** de publicar el artifact
para Sonar, de modo que si un BC falla, el pipeline corta y Sonar
ni siquiera se ejecuta.

**Alternativas descartadas:**

- *Sólo gate proyecto-wide (70 % overall)*: no detecta un BC rezagado
  cuando otro compensa. Inaceptable dada la arquitectura por BCs
  independientes que va a crecer con `issuer` y `verifier`.
- *Proyectos Sonar separados por BC*: fragmenta historial, dificulta
  la vista global, y SonarCloud cobra por proyecto en algunos tiers.
  Overkill para un monorepo con convención clara.
- *Portfolios/Applications de Sonar*: requieren tier empresarial; el
  proyecto usa SonarCloud free.
- *Check por BC en Sonar vía `component` queries en el scanner*:
  tecnicamente posible pero no evalua gate. Pierde la semántica de
  "bloquear PR".

### 2. Formato de reporte: OpenCover + Cobertura duales

**Decisión:** Generar ambos formatos en cada ejecución de
`dotnet test`. OpenCover va al scanner Sonar
(`sonar.cs.opencover.reportsPaths`); Cobertura va al artifact del PR,
al script de enforcement per-BC, y queda disponible para badges/UI
futuros.

**Rationale:**

- OpenCover distingue `Branch Coverage` vs `Sequence Coverage` con
  fidelidad en C#; Sonar lo mapea a `branch_coverage` y
  `line_coverage` sin pérdida.
- Cobertura es el formato universal que consumen herramientas como
  ReportGenerator, `danger-cobertura`, y la UI nativa de Azure
  DevOps/GitHub (vía extensiones). Evita lock-in si cambiamos de
  Sonar.
- El costo marginal de generar los dos es despreciable (mismo
  tracing, dos serializadores).

**Alternativas descartadas:**

- *Sólo OpenCover*: rompe el step de per-BC si se cambia a otra
  librería de parsing; Cobertura tiene soporte más amplio.
- *Sólo Cobertura*: Sonar soporta Cobertura vía
  `sonar.cs.cobertura.reportsPaths` (SonarScanner 6.0+) pero
  históricamente la granularidad de branch coverage es menor que
  OpenCover para C#.
- *dotCover / AltCover*: herramientas excelentes pero cambian el
  stack de Microsoft recomendado (Coverlet) sin beneficio claro.

### 3. Configuración de Coverlet centralizada

**Decisión:**

1. Crear `coverlet.runsettings` en la raíz del repo con
   `Format=opencover,cobertura`, `DeterministicReport=false`,
   `SingleHit=false`, `Threshold=70`, `ThresholdType=line,branch,method`,
   `ThresholdStat=total`, y las listas de `Exclude`, `ExcludeByFile`,
   `ExcludeByAttribute` detalladas en §5.
2. Crear `Directory.Build.props` en `tests/` que inyecte
   `<PackageReference Include="coverlet.collector" Version="6.0.2"
   PrivateAssets="all" />` a todos los proyectos de test.
3. Eliminar la referencia explícita duplicada a `coverlet.collector`
   en `SovereignID.Architecture.Tests.csproj`.

**Rationale:** un solo lugar de verdad. Cualquier nuevo proyecto de
test en `tests/bc-issuer/` o `tests/bc-verifier/` hereda el setup sin
copy-paste. El `Threshold=70` actúa como *guardrail local*: si un
desarrollador corre `dotnet test --settings coverlet.runsettings` y un
proyecto baja de 70 %, falla localmente antes de llegar a CI.
`DeterministicReport=false` evita `NotSupportedException` en el reporter
OpenCover de Coverlet; si en el futuro se exige determinismo en rutas,
habría que migrar la ingesta de cobertura en Sonar a Cobertura u otro
formato compatible.

**Alternativas descartadas:**

- *Configurar Coverlet por proyecto*: viola DRY, propenso a drift
  entre proyectos.
- *MSBuild integration de Coverlet* (`coverlet.msbuild` con
  `<PackageReference>` + target hook): más invasivo, requiere pasar
  props en la línea de comandos, y no compone bien con
  `--collect:"XPlat Code Coverage"` que necesita
  `coverlet.collector`.

### 4. Política de exclusión (tres niveles, cada uno con su razón)

**Decisión:** aplicar exclusiones en tres capas ortogonales, cada una
con semántica distinta:

| Capa                              | Qué excluye                                          | Scope                |
|-----------------------------------|------------------------------------------------------|----------------------|
| `coverlet.runsettings`            | legacy assemblies, `*Marker`, `Program.cs`, DTOs     | cálculo en el report |
| `sonar.exclusions` (scanner)      | `wwwroot/**`, `src/legacy/**`                         | análisis completo    |
| `sonar.coverage.exclusions`       | `src/legacy/**`, `**/*Marker.cs`, `**/Program.cs`     | coverage en Sonar    |
| `[ExcludeFromCodeCoverage]`       | hosting trivial, composition root, DTOs concretos     | Roslyn + Coverlet    |

La combinación asegura que:

- Los **archivos legacy** quedan fuera de coverage (nivel Sonar +
  Coverlet) pero **siguen siendo analizados** por Sonar para reglas
  de calidad ya documentadas en `sonar-debt-phase-2`.
- El **frontend JS** (`wwwroot/`) se saca del análisis **completo**
  de Sonar — no queremos el plugin JavaScript activo hasta tener
  tests Playwright, y así no contaminamos issues.
- Los **markers y composition root** quedan fuera del denominador de
  coverage por ser código estructural no testeable útilmente.

**Archivos marcados `[ExcludeFromCodeCoverage]` (listado cerrado
para este change)**:

- `src/bc-auth/SovereignID.Auth.Api/Program.cs`
- `src/bc-auth/SovereignID.Auth.Api/Infrastructure/SystemClock.cs`
- `src/bc-auth/SovereignID.Auth.Api/Infrastructure/GuidGenerator.cs`
- `src/bc-auth/SovereignID.Auth.Api/Configuration/AuthOptions.cs`
- `src/bc-auth/SovereignID.Auth.Api/Contracts/VerifyRequest.cs`
- `src/bc-auth/SovereignID.Auth.Api/Contracts/VerifyResponse.cs`
- `src/bc-auth/SovereignID.Auth.Api/Contracts/NonceResponse.cs`
- `src/bc-auth/SovereignID.Auth.Infrastructure/Configuration/AuthOptions.cs`
- `src/shared/SovereignID.SharedKernel.Application/CqrsAbstractions.cs`
- Todos los `*Marker.cs` de `bc-auth`, `bc-issuer`, `bc-verifier`,
  `shared` (coherente con `sonar-debt-phase-2`).

**Alternativas descartadas:**

- *Sólo `sonar.coverage.exclusions` sin `[ExcludeFromCodeCoverage]`*:
  el reporte Cobertura local sigue contando esos archivos como 0 % y
  el threshold local de Coverlet (70 %) fallaría. Desalinea CI y
  local.
- *Sólo `[ExcludeFromCodeCoverage]`*: Sonar lo respeta sólo si el
  reporte ingerido ya lo excluyó (lo hace Coverlet). Pero el atributo
  no cubre legacy — necesitamos `sonar.coverage.exclusions` para
  `src/legacy/**` porque no vamos a tocar código legacy para añadir
  atributos.
- *Desactivar el plugin JS de Sonar*: no es deseable para cuando
  volvamos al frontend. Mejor `sonar.exclusions` puntual.

### 5. Enforcement por BC (lectura estricta del 70 %)

**Decisión:** en `ci.yml`, después de `dotnet test` y antes de subir
el artifact para Sonar, ejecutar un script que:

1. Parsea el XML Cobertura consolidado.
2. Agrupa las clases por BC usando el prefijo del assembly
   (`SovereignID.Auth.*` → `bc-auth`, `SovereignID.SharedKernel.*` →
   `shared`, etc.) **excluyendo** `SovereignID.Crypto.*`,
   `SovereignID.Chain.*`, `SovereignID.Demo.Phase1` (legacy).
3. Calcula line coverage por BC.
4. Falla el step con un mensaje legible si algún BC < 70 %.

**Implementación preferida:** PowerShell inline en el workflow (ya se
usa PowerShell en `sonarqube.yml`, no añadimos nueva dependencia).
El script vive en `ci/Check-BcCoverage.ps1` para ser testeable
localmente.

**Rationale:**

- Usa directamente el reporte ya generado; no requiere tooling
  extra.
- El umbral (70) y la lista de BCs se leen de variables/constantes
  al principio del script, fácil de evolucionar.
- Cortar antes del job Sonar evita consumir el minuto de analyzer
  cuando ya sabemos que falló.

**Alternativas descartadas:**

- *ReportGenerator + assertion*: herramienta excelente pero pesada
  (NuGet tool + dotnet tool install en CI). Justificable si
  queremos HTML reports bonitos; no lo es si sólo queremos un
  threshold.
- *`dotnet-coverage` de Microsoft*: tool oficial pero el enforcement
  por assembly requiere el mismo parseo.
- *Hacer el check en post-scan de Sonar*: Sonar no expone coverage
  por directorio en el gate; tendríamos que polling con API. Menos
  limpio.

### 6. Consolidación de workflows

**Decisión:** reducir a dos workflows con responsabilidades claras y
un artifact compartido.

```
ci.yml  (ubuntu-latest)                sonarqube.yml (windows-latest)
────────────────────────               ───────────────────────────────
restore                                download artifact (coverage)
build                                  setup JDK 17 (Sonar scanner)
test --filter "Category!=Integration"  scanner begin
  --settings coverlet.runsettings        /d:sonar.cs.opencover.reportsPaths
per-BC coverage check                    /d:sonar.exclusions
upload artifact (opencover +             /d:sonar.coverage.exclusions
                 cobertura)              /d:sonar.qualitygate.wait=true
                                       dotnet build (sin restore)
                                       scanner end
```

`dotnet.yml` se **elimina** (redundante con `ci.yml`).

**Rationale:**

- Separación SO → tests en Linux (más rápido, más cercano a
  producción si se despliega en Linux), Sonar en Windows
  (dotnet-sonarscanner mejor soportado).
- `needs:` encadena: Sonar corre sólo si `ci.yml` pasó (incluyendo el
  check per-BC). Ahorra minutos y evita verde-rojo confuso.
- `sonar.qualitygate.wait=true` bloquea el PR si el gate falla;
  añade ~30s al PR pero es el patrón CaYC recomendado.
- Artifacts persisten 7 días (default GH Actions) — suficiente para
  triage post-merge.

**Alternativas descartadas:**

- *Workflow único con dos jobs*: válido, pero acopla triggers. Tener
  dos archivos independientes permite desactivar Sonar (p.ej. si
  SONAR_TOKEN vence) sin romper CI.
- *Correr Sonar en Linux con `dotnet-sonarscanner`*: funciona pero
  los docs de SonarSource aún recomiendan Windows para cobertura
  OpenCover "oficial". No vale la pena arriesgar tiempo de CI.

### 7. Convención `[Trait("Category","Integration")]`

**Decisión:** el trait aplica **si y sólo si** el test toca red
o I/O externo real (endpoint RPC de Sepolia, contratos on-chain,
fileystem fuera de `TestResults/`, base de datos externa). Los tests
que usan `WebApplicationFactory<Program>` in-process, con
dependencias reemplazadas (fake clock, fake nonce generator, fake
RPC si aplicara), **no** llevan el trait.

Bajo esta definición:

| Test                                             | Trait         | Corre en CI |
|--------------------------------------------------|---------------|-------------|
| `AuthEndpointsTests` (7 casos, WAF)              | —             | Sí          |
| `GenerateNonceQueryHandlerTests`                 | —             | Sí          |
| `VerifySiweCommandHandlerTests`                  | —             | Sí          |
| `ManualSiweMessageParserTests`                   | —             | Sí          |
| `NethereumSiweSignatureVerifierTests`            | —             | Sí          |
| `JwtBearerTokenIssuerTests`                      | —             | Sí          |
| `InMemoryAuthChallengeRepositoryTests`           | —             | Sí          |
| `SecureRandomNonceGeneratorTests`                | —             | Sí          |
| `Nonce/ChainId/AuthChallengeTests`               | —             | Sí          |
| `ArchitectureRulesTests`                         | —             | Sí          |
| `Crypto.Tests/*` (sin red)                       | —             | Sí (legacy tests, no coverage destino) |
| `SepoliaClientIntegrationTests`                  | `Integration` | No (skip)   |
| `DocumentNotarizerIntegrationTests`              | `Integration` | No (skip)   |

**Notas de tracing futuras:**

- El **nombre** `SovereignID.Auth.IntegrationTests` se mantiene.
  El README añade una nota explicando la convención para evitar
  ambigüedad con lectores nuevos.
- Futuros tests WAF en `bc-issuer` / `bc-verifier` siguen esta
  misma regla.

## Risks / Trade-offs

- **[Gate propio requiere acción manual en SonarCloud UI]**
  La creación del Quality Gate y su asignación se hacen vía UI o API
  una sola vez; no se automatizan en este change.
  **Mitigación:** `tasks.md` documenta los pasos exactos (clonar,
  añadir condición, asignar), y el spec requiere evidencia
  (screenshot o referencia al analysis de CI) de que el gate activo
  sobre el proyecto tiene las condiciones esperadas.

- **[Cambio de gate rompe PRs pendientes]**
  Si hay PRs abiertos al hacer el switch, podrían pasar de "OK
  vacío" a "ERROR coverage < 80 %" sin cambio propio.
  **Mitigación:** aplicar el gate custom **después** de mergear este
  change (el scan del PR que lo introduce ya tendrá el reporte y
  cumplirá el gate). Documentar en tasks §6.

- **[Per-BC check falla en master tras un drop inesperado]**
  Un BC nuevo (p.ej. `bc-issuer` en próximas fases) puede no tener
  tests al principio y romper el check.
  **Mitigación:** el script ignora BCs con **0 líneas testeables** o
  no presentes en el reporte — un BC vacío no bloquea. Sólo bloquea
  cuando hay código y cobertura < 70 %.

- **[Sonar no respeta `sonar.coverage.exclusions` si el report ya
  las incluye]**
  Si Coverlet incluye `src/legacy/**` en el XML, Sonar cuenta esas
  líneas aunque el scanner las excluya.
  **Mitigación:** las exclusiones de Coverlet (§4) son la
  **primera** defensa. `sonar.coverage.exclusions` es redundancia
  defensiva — si Coverlet lo hace bien, Sonar ni las ve.

- **[`[ExcludeFromCodeCoverage]` olvidado en archivos nuevos]**
  Un composition root futuro (p.ej. `bc-issuer/Api/Program.cs`)
  podría olvidar el atributo y contar 0 % injustamente.
  **Mitigación:** task en el DoD del futuro change
  `phase-3-vc-issuance` para añadir el atributo, y una línea en
  `AGENTS.md` que lo indique como convención de layer Api.

- **[`sonar.qualitygate.wait=true` añade latencia a PRs]**
  ~30s por PR según carga del scanner.
  **Mitigación:** aceptable para un proyecto en el tamaño actual;
  revisitar sólo si el tiempo total de CI sobrepasa 5 min.

- **[Script PowerShell de per-BC frágil a renombres]**
  Si alguien renombra `SovereignID.Auth.*` → otra cosa, el script
  silenciosamente dejaría de ver el BC.
  **Mitigación:** el script **falla** si la lista de BCs esperados
  (`bc-auth`, `shared`) no aparece en absoluto en el reporte — better
  loud-failure que un falso verde.

- **[OpenCover format puede cambiar entre versiones de Coverlet]**
  Ruptura menor rara pero posible.
  **Mitigación:** pin de versión en `Directory.Build.props`
  (`6.0.2` concreto, no floating). Renovación es un task
  consciente en futuras fases.

## Migration Plan

1. **Crear el gate en SonarCloud** (manual, UI o API): duplicar
   `Sonar way` → `SovereignID default` → añadir `coverage ≥ 70 %` →
   asignar al proyecto `Skiiinnny_SovereingID`. Screenshot en PR.
2. **Introducir `coverlet.runsettings` + `Directory.Build.props`**:
   PR separable aunque se mergee en el mismo branch. Verificar
   `dotnet test --settings coverlet.runsettings` local antes de
   empujar.
3. **Añadir `[ExcludeFromCodeCoverage]`** en los archivos listados
   en §4. Ningún cambio de comportamiento.
4. **Reescribir `ci.yml`** con el job `build-test` que incluye
   `dotnet test` con el runsettings, el check per-BC
   (`ci/Check-BcCoverage.ps1`) y el upload del artifact.
5. **Reescribir `sonarqube.yml`** para descargar el artifact y
   pasar `sonar.cs.opencover.reportsPaths` + exclusiones +
   `sonar.qualitygate.wait=true` al scanner.
6. **Eliminar `dotnet.yml`** del repo.
7. **Empujar el PR**. Verificar en el run:
   - `ci.yml` verde, per-BC check mostrando cobertura real.
   - `sonarqube.yml` verde, coverage reportada ≠ 0.
   - Quality Gate `SovereignID default` aplicado y pasado.
8. **Rollback**: si cualquier paso rompe PRs open, revertir el
   commit que tocó workflows (`ci.yml`, `sonarqube.yml`,
   `dotnet.yml`) deja el estado previo operativo en < 5 min.
9. **Documentar en README** la convención `Integration` y el
   comando local de tests con cobertura.

## Open Questions

- Ninguna asumida. Si en revisión se decide mover al 75 % el
  umbral per-BC (más estricto) o bajarlo al 65 % temporalmente
  (más laxo), se ajusta en una sola constante dentro del script
  PowerShell sin cambiar el spec — o, si el cambio es permanente,
  se enmienda el spec con un MODIFIED delta en el próximo change.
