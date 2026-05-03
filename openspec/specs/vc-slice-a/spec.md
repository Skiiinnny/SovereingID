# Spec: vc-slice-a (Phase 3 — primer incremento)

> Requisitos normativos para VC **TituloGraduacion** y VP mínima verificable
> (slice A). Convenciones de dominio: `src/shared/CONTEXT.md`.
> Especificación canónica en `openspec/specs/` (sincronizada desde el delta del cambio `phase-3-vc-slice-a`).

## ADDED Requirements

### Requirement: Shared CredentialType for slice A

The system SHALL expose a closed set `CredentialType` in
`SovereignID.SharedKernel.Domain` that includes at least `TituloGraduacion`
for this increment. Issuer and Verifier domains SHALL reference this type
when validating or emitting the single supported academic profile.

#### Scenario: Enum is shared across bounded contexts

- **WHEN** a developer inspects `SovereignID.SharedKernel.Domain`
- **THEN** `CredentialType` exists and contains `TituloGraduacion`

### Requirement: Canonical did:ethr Sepolia subject and issuer strings

The system SHALL represent both `credentialSubject.id` and `issuer` (or
`issuer.id` when `issuer` is an object) as case-normalized Ethereum-backed
DIDs using the literal prefix `did:ethr:sepolia:` followed by `0x` and
exactly 40 lowercase hexadecimal characters.

#### Scenario: Parser rejects wrong network or casing

- **WHEN** a verifier or issuer domain component parses a DID string that is
  not `did:ethr:sepolia:0x` + 40 lowercase hex digits
- **THEN** validation fails before cryptographic verification

### Requirement: Verifiable Credential document identifiers and types

Every Verifiable Credential issued under this change SHALL have:

- `id` equal to `urn:uuid:` plus a lowercase UUID string in the standard
  8-4-4-4-12 hex pattern.
- `type` array containing exactly the strings `VerifiableCredential` and
  `TituloGraduacionCredential` for this profile (no additional product-specific
  VC types in slice A).
- `@context` as a JSON array whose first element is the string
  `https://www.w3.org/2018/credentials/v1` and whose second element is a
  JSON object defining domain-specific terms needed for
  `TituloGraduacionCredential` and its claims.

#### Scenario: Missing TituloGraduacionCredential type is rejected

- **WHEN** a verifier validates a VC document missing `TituloGraduacionCredential`
  in `type`
- **THEN** validation fails

### Requirement: TituloGraduacion credentialSubject claims allowlist

For `CredentialType.TituloGraduacion`, the `credentialSubject` object SHALL
contain only the following additional properties besides `id`:
`degreeTitle`, `programName`, and `awardDate`. The value of `awardDate` MUST
be a calendar date string `YYYY-MM-DD` with no time or timezone component.
No `legalName` or other PII fields are permitted in slice A.

#### Scenario: Extra claim property is rejected

- **WHEN** a verifier or issuer validates a VC whose `credentialSubject`
  contains `legalName` or any key not in the allowlist
- **THEN** validation fails

### Requirement: VC issuance and expiration instants

Every VC SHALL include `issuanceDate` as an RFC 3339 timestamp in UTC with a
`Z` suffix. A VC MAY omit `expirationDate`; when present, `expirationDate`
MUST use the same RFC 3339 UTC `Z` format. The verifier SHALL reject VCs
whose `issuanceDate` is strictly in the future relative to an injected clock,
and SHALL reject VCs whose `expirationDate` is present and strictly before the
clock instant.

#### Scenario: Future issuanceDate fails

- **WHEN** the clock returns `2026-05-03T12:00:00Z` and `issuanceDate` is
  `2026-05-04T00:00:00Z`
- **THEN** verification fails

#### Scenario: Absent expirationDate is acceptable

- **WHEN** `expirationDate` is absent and all other checks pass
- **THEN** verification does not fail solely for missing expiration

### Requirement: Issuer EIP-712 binding to issuer DID

The issuer’s EIP-712 signature over the VC MUST be verifiable such that the
recovered Ethereum address equals the address parsed from the VC’s `issuer`
DID (per the canonical `did:ethr:sepolia:` rules).

#### Scenario: Signature from wrong key fails

- **WHEN** the EIP-712 signature recovers to an address different from the
  issuer DID address
- **THEN** verification fails

### Requirement: Minimal Verifiable Presentation shape

A Verifiable Presentation accepted in slice A SHALL:

- Include `type` as an array whose sole string entry is
  `VerifiablePresentation`.
- Include `holder` as a string exactly equal to `credentialSubject.id` of
  the embedded VC.
- Include `verifiableCredential` as an array with exactly one element, and
  that element MUST be the full embedded VC object (not a reference-only
  object).

#### Scenario: Two embedded credentials rejected

- **WHEN** `verifiableCredential` contains two VC objects
- **THEN** verification fails

#### Scenario: holder mismatch rejected

- **WHEN** `holder` differs from the embedded VC’s `credentialSubject.id`
- **THEN** verification fails

### Requirement: Holder EIP-712 on presentation

The holder MUST prove control by providing an EIP-712 signature over the
presentation using dedicated presentation types (distinct from the VC
issuer types). The verifier SHALL recover the holder address and MUST require
equality with the address parsed from `credentialSubject.id` of the embedded
VC.

#### Scenario: Holder signature does not match subject

- **WHEN** the recovered holder address does not match the subject DID address
- **THEN** verification fails

### Requirement: No chain or IPFS dependency in slice A

Neither issuance nor verification logic in this change SHALL require IPFS
fetch, RPC calls to Sepolia, or on-chain registry reads for correctness of the
above rules. Ports for future storage or registry MAY exist as interfaces with
in-memory or fake implementations only.

#### Scenario: Unit tests run without network

- **WHEN** the unit test suite for Issuer and Verifier domains runs in CI
  without outbound blockchain or IPFS access
- **THEN** tests that cover the requirements above still execute successfully
