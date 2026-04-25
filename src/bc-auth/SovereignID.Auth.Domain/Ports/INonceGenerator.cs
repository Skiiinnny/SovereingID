namespace SovereignID.Auth.Domain.Ports;

/// <summary>
/// Produces cryptographically strong nonces.
/// </summary>
public interface INonceGenerator
{
    /// <summary>
    /// Generates a new nonce.
    /// </summary>
    Task<Nonce> NewAsync(CancellationToken cancellationToken);
}
