using SovereignID.SharedKernel.Domain;

namespace SovereignID.Auth.Domain.Ports;

/// <summary>
/// Recovers the signer address from a SIWE signature.
/// </summary>
public interface ISiweSignatureVerifier
{
    /// <summary>
    /// Recovers the address that signed the message.
    /// </summary>
    Task<EthereumAddress> RecoverAddressAsync(string message, Signature signature, CancellationToken cancellationToken);
}
