# Spec: vp-presentation-split (delta del cambio)

> Requisitos de **estructura interna** del flujo de verificación de presentación
> slice A en `bc-verifier`. Comportamiento normativo de VP/VC sigue en
> `openspec/specs/vc-slice-a/spec.md` y en `src/shared/CONTEXT.md` / `src/bc-verifier/CONTEXT.md`.

## ADDED Requirements

### Requirement: VP envelope parsing is isolated from document and root proof

The Verifier Application SHALL provide an `internal` module whose sole
responsibility is validating the minimal VP JSON envelope and extracting the
embedded VC: root object, `type` array with only `VerifiablePresentation`,
`holder` as a canonical `did:ethr:sepolia:` string parseable by
`EthrSepoliaDidParser`, and `verifiableCredential` as an array with exactly one
JSON object element. That module SHALL NOT invoke
`TituloGraduacionVcDocumentValidator` (or any `SovereignID.VcSliceA.Document`
credential-shape validator). It SHALL NOT require a `proof` property on the VP
root.

#### Scenario: Malformed envelope fails without document validation

- **WHEN** the VP root is missing `verifiableCredential`
- **THEN** verification fails with a `vp_*` error code and the system does not
  invoke `TituloGraduacionVcDocumentValidator`

#### Scenario: Missing VP root proof does not fail at envelope stage

- **WHEN** the envelope is otherwise valid but the VP root omits `proof`
- **THEN** the envelope module succeeds and failure for missing `proof`, if
  any, occurs only after embedded VC document validation in the chain module
  (preserving slice A ordering)

### Requirement: Embedded VC chain module owns document through VP holder signature

The Verifier Application SHALL provide an `internal` module that, given the
envelope extraction result, runs in order: (1) `TituloGraduacionVcDocumentValidator`
on the embedded VC with `CredentialValidationMode.Signed` and maps document
errors through the existing Verifier mapper; (2) holder vs `credentialSubject.id`
string and address equality checks; (3) VC `proof` extraction via the shared
helper and VC issuer EIP-712 recovery; (4) VP root `proof` presence and VP
holder EIP-712 recovery. Cryptographic dependencies remain `SovereignID.VcSliceA.Eip712`.

#### Scenario: Invalid VC document blocks VP proof check

- **WHEN** the embedded VC fails `TituloGraduacionVcDocumentValidator`
- **THEN** verification returns a mapped document error and does not return
  `vp_proof_missing` for that same request

### Requirement: Shared internal EIP-712 proof value reader

Duplicated logic for reading `proof.type` and `proofValue` from a JSON `proof`
object for both VC and VP proofs SHALL be consolidated into a single
`internal` helper used by the embedded VC chain module (and any envelope-adjacent
code if needed). The helper SHALL use the same `ProofType` constant as today
(`VcSliceAEip712Constants.ProofType`) and SHALL preserve existing error codes
`proof_type_invalid`, `proof_value_missing`, and `proof_value_empty`.

#### Scenario: VC and VP proofs use the same extraction rules

- **WHEN** a VC `proof` and a VP `proof` both omit `proofValue`
- **THEN** both paths surface `proof_value_missing` from the shared helper

### Requirement: Public entry point remains the facade

The public static API used by callers (e.g. `PresentationVerifier.Verify`) SHALL
remain the supported entry point for presentation verification in this change.
New types SHALL be `internal` to `SovereignID.Verifier.Application` unless a
second consumer is introduced in a follow-up change.

#### Scenario: Handlers do not reference new internal types

- **WHEN** `VerifyPresentationCommandHandler` (or equivalent) is inspected
- **THEN** it depends only on the existing public verification surface

### Requirement: Tests stay in a single file for the first iteration

Automated tests for presentation verification SHALL remain in the existing
`PresentationVerifierTests` file for the initial refactor commit series. Splitting
test files MAY occur only after the production split is stable.

#### Scenario: CI still runs one primary presentation test class

- **WHEN** the change is merged
- **THEN** the primary presentation verification tests still live in one test
  source file as agreed
