namespace SovereignID.Auth.Domain.Ports;

/// <summary>
/// Parses SIWE payloads into typed messages.
/// </summary>
public interface ISiweMessageParser
{
    /// <summary>
    /// Parses the supplied SIWE payload.
    /// </summary>
    /// <returns>
    /// Success with a populated <see cref="SiweMessage"/> (including <see cref="SiweMessage.OriginalPayload"/>)
    /// when the payload is valid; otherwise failure with an <see cref="AuthError"/> (typically
    /// <c>siwe_parse_failed</c> for malformed EIP-4361). Expected parse failures are reported via the result,
    /// not thrown.
    /// </returns>
    Task<Result<SiweMessage, AuthError>> ParseAsync(string payload, CancellationToken cancellationToken);
}
