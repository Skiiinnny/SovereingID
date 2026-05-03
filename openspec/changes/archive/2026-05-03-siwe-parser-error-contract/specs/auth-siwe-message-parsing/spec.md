## ADDED Requirements

### Requirement: Explicit parse outcome on the SIWE parser port

The `ISiweMessageParser` port SHALL declare a single parse operation whose completion value distinguishes success from failure using `SovereignID.Auth.Domain.Result<SiweMessage, AuthError>` (or a type alias with identical observable semantics). The operation SHALL NOT use thrown exceptions as the sole or primary channel for expected parse failures (malformed payload, invalid SIWE lines, invalid address, unsupported optional fields).

#### Scenario: Valid EIP-4361 payload succeeds

- **WHEN** `ParseAsync` is invoked with a payload that satisfies EIP-4361 layout rules implemented by the adapter
- **THEN** the returned `Result` is success and its value contains a non-null `SiweMessage` including `OriginalPayload` equal to the supplied payload

#### Scenario: Malformed payload fails with structured parse error

- **WHEN** `ParseAsync` is invoked with a payload that violates SIWE layout or field rules
- **THEN** the returned `Result` is failure
- **AND** the `AuthError.Code` is `siwe_parse_failed`
- **AND** the `AuthError.Detail` contains human-readable diagnostic text suitable for HTTP 400 and logs

#### Scenario: No exception-only contract for expected parse failures

- **WHEN** a consumer awaits `ParseAsync` for any payload that the adapter rejects as invalid SIWE
- **THEN** the consumer can determine failure exclusively from the `Result` without catching an exception type defined only by the infrastructure assembly

### Requirement: Verify flow preserves parser AuthError

`VerifySiweCommandHandler` SHALL propagate a parse failure `AuthError` returned by `ISiweMessageParser` unchanged into its own `Result<VerifySiweResult, AuthError>` failure branch. It MUST NOT replace a failure `AuthError` originating from the parser with a new `siwe_parse_failed` envelope derived only from `Exception.Message`.

#### Scenario: Parser returns parse failure

- **WHEN** the parser returns `Result` failure with `AuthError` code `siwe_parse_failed`
- **THEN** the handler's `HandleAsync` returns failure with the same `AuthError.Code` and the same `AuthError.Detail`
