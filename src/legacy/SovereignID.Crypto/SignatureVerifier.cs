using Nethereum.Signer;

namespace SovereignID.Crypto;

/// <summary>Recovers and verifies Ethereum personal_sign signatures.</summary>
public class SignatureVerifier
{
    private readonly EthereumMessageSigner _ethereumMessageSigner = new();

    /// <summary>Recovers the signer's checksummed address from a message and signature.</summary>
    public string RecoverSigner(string message, string signature)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentException.ThrowIfNullOrWhiteSpace(signature);
        return _ethereumMessageSigner.EncodeUTF8AndEcRecover(message, signature);
    }

    /// <summary>Returns true when the recovered signer matches <paramref name="expectedAddress"/> (case-insensitive).</summary>
    public bool Verify(string message, string signature, string expectedAddress)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedAddress);
        var recovered = RecoverSigner(message, signature);
        return string.Equals(recovered, expectedAddress, StringComparison.OrdinalIgnoreCase);
    }
}
