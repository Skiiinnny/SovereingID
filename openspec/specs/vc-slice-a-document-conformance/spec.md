# Spec: vc-slice-a-document-conformance

> Requisitos del **módulo** compartido de conformidad de documento VC y su uso en Issuer y Verifier.
> Requisitos de producto generales de slice A siguen en `openspec/specs/vc-slice-a/spec.md`.

## ADDED Requirements

### Requirement: Non-cryptographic shared assembly

The system SHALL provide a .NET 9 class library project
`SovereignID.VerifiableCredential.Document` under `src/shared/` that SHALL NOT reference
Nethereum packages or `SovereignID.VerifiableCredential.Eip712`. The library MAY reference
`SovereignID.SharedKernel.Domain` for canonical DID parsing rules.

#### Scenario: Architecture confinement preserved

- **WHEN** a maintainer inspects `SovereignID.VerifiableCredential.Document.csproj`
- **THEN** no `PackageReference` to `Nethereum.*` exists and no project reference
  to `SovereignID.VerifiableCredential.Eip712` exists

### Requirement: Canonical proof type string ownership

The literal value used for `proof.type` on issued VCs (`SovereignIDEip712Signature2026`)
SHALL be defined exactly once in `SovereignID.VerifiableCredential.Document` as the
single source of truth for JSON documents. `SovereignID.VerifiableCredential.Eip712`
SHALL consume that definition (by project reference to `VerifiableCredential.Document`)
so EIP-712 helpers and JSON `proof.type` cannot drift.

#### Scenario: Eip712 project references Document

- **WHEN** a maintainer inspects `SovereignID.VerifiableCredential.Eip712.csproj`
- **THEN** a project reference to `SovereignID.VerifiableCredential.Document` exists and
  `VerifiableCredentialEip712Constants.ProofType` resolves to the same literal as the
  document module’s public constant

### Requirement: Strict second @context vocabulary

For `CredentialType.TituloGraduacion`, when validating a VC JSON document, the
validator SHALL require `@context` to be a JSON array whose first element is
exactly `https://www.w3.org/2018/credentials/v1` and whose second element is a
JSON object that matches, key-for-key and value-for-value (string equality,
ordinal), the inline context object produced for this profile by the issuer’s
document builder (same IRIs under `https://sovereignid.local/vocab/titulo-graduacion/v1#`).

#### Scenario: Wrong vocabulary IRI fails

- **WHEN** the second `@context` object maps `degreeTitle` to a string other
  than `https://sovereignid.local/vocab/titulo-graduacion/v1#degreeTitle`
- **THEN** validation returns a canonical error code (not BC-prefixed)

#### Scenario: Extra term in second context fails

- **WHEN** the second `@context` object contains a key not present in the
  canonical issuer context map
- **THEN** validation returns a canonical error code

### Requirement: credentialSubject strict allowlist and formats

The validator SHALL require `credentialSubject` to be a JSON object containing
only the properties `id`, `degreeTitle`, `programName`, and `awardDate`.
`id`, `degreeTitle`, `programName`, and `awardDate` MUST be non-empty strings.
`id` and the VC’s top-level `issuer` string MUST parse as canonical
`did:ethr:sepolia:0x` + 40 lowercase hex per `EthrSepoliaDidParser` rules.
`awardDate` MUST match `YYYY-MM-DD` and be a valid calendar date.

#### Scenario: Extra subject property fails

- **WHEN** `credentialSubject` contains `legalName`
- **THEN** validation returns a canonical error code

#### Scenario: Invalid subject DID fails

- **WHEN** `credentialSubject.id` is not a canonical Sepolia ethr DID
- **THEN** validation returns a canonical error code

### Requirement: VC shape and temporal rules with injected clock

The validator SHALL enforce, on the VC JSON root: `type` is a JSON array whose
set of string entries is exactly `VerifiableCredential` and
`TituloGraduacionCredential`; `id` matches `urn:uuid:` + lowercase UUID pattern;
`issuanceDate` and optional `expirationDate` match RFC 3339 UTC with `Z`
suffix; `issuanceDate` MUST NOT be strictly after `nowUtc`; when
`expirationDate` is present it MUST NOT be strictly before `nowUtc`. The
validator SHALL require `issuer` as a JSON string (slice A implementation
shape). Validation of `proof` presence and minimal proof shape (for signed
documents) SHALL be supported as specified by the library’s public API so the
Verifier path can reject missing or malformed proofs before EIP-712 recovery.

#### Scenario: Future issuance fails

- **WHEN** `nowUtc` is `2026-05-03T12:00:00Z` and `issuanceDate` is
  `2026-05-04T00:00:00Z`
- **THEN** validation returns a canonical error code

### Requirement: Canonical error codes and BC mapping

The document validator SHALL return stable machine-oriented error identifiers
without bounded-context prefixes (e.g. `document_context_second_mismatch`).
Issuer and Verifier code SHALL map those identifiers to their existing
prefixed or legacy user-facing codes at their application/domain edges without
requiring the shared library to know those prefixes.

#### Scenario: Verifier maps canonical code

- **WHEN** the document module returns `document_subject_extra_property`
- **THEN** the Verifier MAY surface `vc_subject_extra_property` (or the
  project’s chosen prefixed string) at its API boundary

### Requirement: Issuer invokes validator before integrity sign

The issuer flow that builds a `TituloGraduacion` VC SHALL invoke
`SovereignID.VerifiableCredential.Document` validation on the VC JSON (unsigned or per the
library’s unsigned mode) and SHALL NOT proceed to EIP-712 signing when
validation fails.

#### Scenario: Invalid claims block signing

- **WHEN** `awardDate` is invalid and the issuer reaches the signing step
- **THEN** issuance fails without calling `IIssuerVcIntegritySigner`

### Requirement: Verifier delegates VC document shape to the module

The Verifier’s presentation verification path SHALL delegate VC document-shape
and subject rules to `SovereignID.VerifiableCredential.Document` instead of duplicating
equivalent checks in application-local static validators. Cryptographic
EIP-712 recovery, VP-level checks, and holder/issuer address equality against
recovered signatures remain in the Verifier assembly.

#### Scenario: Duplicate subject validator removed

- **WHEN** the change is complete
- **THEN** `EmbeddedTituloGraduacionSubjectValidator` is removed or reduced to
  a thin adapter that only forwards to the shared module

### Requirement: Automated tests for the document module

The system SHALL include automated tests (project `SovereignID.VerifiableCredential.Document.Tests`
or equivalent) that cover valid golden JSON, invalid `@context`, invalid
subject keys, invalid `awardDate`, invalid DID, and temporal failures without
network access.

#### Scenario: CI runs document tests without RPC

- **WHEN** the test project runs in CI with no outbound blockchain access
- **THEN** all tests in `SovereignID.VerifiableCredential.Document.Tests` pass
