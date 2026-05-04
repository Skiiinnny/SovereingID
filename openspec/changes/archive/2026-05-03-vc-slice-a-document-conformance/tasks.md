## 1. Proyecto y solución

- [x] 1.1 Crear `src/shared/SovereignID.VcSliceA.Document/SovereignID.VcSliceA.Document.csproj` (net9.0), sin Nethereum, con `ProjectReference` a `SovereignID.SharedKernel.Domain` si aplica para DIDs.
- [x] 1.2 Registrar el proyecto en `SovereignID.sln` y crear `tests/shared/SovereignID.VcSliceA.Document.Tests/` (o ruta acordada bajo `tests/`) con referencia al nuevo ensamblado.

## 2. Módulo de conformidad (núcleo)

- [x] 2.1 Exponer constantes canónicas: literal `ProofType`, base `VocabBase` y mapa del segundo `@context` (paridad exacta con el builder actual del emisor).
- [x] 2.2 Implementar validación sobre `JsonElement` (y/o `JsonDocument`) según `design.md`: `@context`, `type`, `id`, `issuer`, fechas con `nowUtc`, `credentialSubject` estricto, modo firmado con `proof` mínimo.
- [x] 2.3 Definir enum o catálogo de **códigos canónicos** devueltos por el módulo (strings estables documentados en XML doc o tipo dedicado).

## 3. Integración Eip712 y emisor

- [x] 3.1 Añadir en `SovereignID.VcSliceA.Eip712` referencia a `VcSliceA.Document` y hacer que `VcSliceAEip712Constants.ProofType` delegue en la constante del módulo Document.
- [x] 3.2 Actualizar `VerifiableCredentialJsonBuilder` para usar las mismas constantes de `VcSliceA.Document` (eliminar duplicación local de `ProofType` y del mapa `@context` donde proceda).
- [x] 3.3 Invocar el validador desde el flujo de emisión **antes** de firmar; mapear códigos canónicos a los códigos con prefijo/naming del emisor existentes.
- [x] 3.4 Eliminar o reducir `TituloGraduacionClaimsValidator` según quede cubierto por validación JSON estricta (sin regresión en tests del emisor).

## 4. Integración verificador

- [x] 4.1 Sustituir en `PresentationVerifier` las comprobaciones duplicadas de forma VC / subject por llamadas al módulo `VcSliceA.Document`, pasando `nowUtc` y modo adecuado (VC embebido con `proof`).
- [x] 4.2 Mapear códigos canónicos a los códigos `vc_*` / existentes del verificador en el borde.
- [x] 4.3 Eliminar `EmbeddedTituloGraduacionSubjectValidator` o dejarlo solo como reenvío mínimo al módulo compartido si temporalmente hace falta.

## 5. Pruebas y arquitectura

- [x] 5.1 Añadir tests unitarios en `SovereignID.VcSliceA.Document.Tests` para escenarios del spec (contexto, subject, fechas, DIDs, proof).
- [x] 5.2 Actualizar `PresentationVerifierTests` y tests del emisor para reflejar códigos o rutas nuevas sin perder cobertura de regresión.
- [x] 5.3 Revisar `ArchitectureRulesTests` / reglas de referencia a Nethereum: confirmar que `VcSliceA.Document` no viola confinamiento.
- [x] 5.4 Ejecutar `dotnet test` en la solución (o subset relevante) y corregir fallos.

## 6. Cierre de especificación

- [x] 6.1 Tras implementar, sincronizar el spec canónico si el flujo del repo lo exige (p. ej. promover delta a `openspec/specs/` según convención del proyecto) y marcar tareas `[x]` en este archivo.
