namespace SovereignID.Auth.Domain.Ports;

/// <summary>
/// Parses SIWE payloads into typed messages.
/// </summary>
public interface ISiweMessageParser
{
    /// <summary>
    /// Parses the supplied SIWE payload.
    /// </summary>
    Task<SiweMessage> ParseAsync(string payload, CancellationToken cancellationToken);
}
