## 1. Helper compartido

- [x] 1.1 Crear tipo `internal` estático (p. ej. `PresentationEip712ProofValueReader`) en `Presentation/` que centralice lectura de `proof.type` y `proofValue` con los mismos códigos que `TryGetProofSignature` hoy.
- [x] 1.2 Sustituir usos duplicados en el flujo actual por el helper (antes o durante el corte de clases).

## 2. Módulo VP envelope

- [x] 2.1 Añadir módulo `internal` que, desde `JsonElement` raíz (o `JsonDocument`), valide envoltura según `src/bc-verifier/CONTEXT.md` (**VP envelope**) y devuelva `holderDid`, `holderAddr` y `JsonElement` del VC único, o código `vp_*`.
- [x] 2.2 Asegurar que **no** llama a `TituloGraduacionVcDocumentValidator` ni exige `proof` en la raíz.

## 3. Módulo cadena VC embebido + VP crypto

- [x] 3.1 Extraer lógica de `VerifyEmbeddedVc` y posteriores firmas VP a un módulo `internal` que implemente el orden: documento VC → enlaces holder → VC EIP-712 → `proof` VP → VP EIP-712.
- [ ] 3.2 Usar el helper de 1.x para pruebas VC y VP; mantener mapeo de errores de documento con `VerifierTituloGraduacionVcDocumentErrorMapper`.

## 4. Fachada y limpieza

- [x] 4.1 Reducir `PresentationVerifier` a orquestación (parse JSON → envelope → cadena) manteniendo firma pública `Verify`.
- [x] 4.2 Eliminar métodos privados obsoletos o dejar solo reenvíos mínimos; revisar `using` y nulos.

## 5. Pruebas y verificación

- [x] 5.1 Ejecutar `PresentationVerifierTests` completo sin partir archivo; corregir fallos hasta paridad de códigos de error.
- [x] 5.2 Si falta cobertura explícita de orden “documento VC inválido antes que `vp_proof_missing`”, añadir un caso en el mismo archivo de tests.
- [x] 5.3 `dotnet test` en proyectos afectados.

## 6. Documentación del cambio

- [x] 6.1 Tras implementar, marcar tareas `[x]` y archivar/promover spec según flujo del repo si aplica.
