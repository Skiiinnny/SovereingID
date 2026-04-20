using Nethereum.Signer;

namespace SovereignID.Crypto;

/// <summary>Signs UTF-8 messages using the Ethereum personal_sign prefix (EIP-191).</summary>
public class MessageSigner
{
    private readonly EthereumMessageSigner _ethereumMessageSigner = new();

    /// <summary>Produces a 0x-prefixed ECDSA signature for <paramref name="message"/>.</summary>
    /// <param name="message">Arbitrary UTF-8 payload.</param>
    /// <param name="privateKey">Hex-encoded private key (0x optional).</param>
    public string Sign(string message, string privateKey)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentException.ThrowIfNullOrWhiteSpace(privateKey);
        var key = new EthECKey(privateKey);
        return _ethereumMessageSigner.EncodeUTF8AndSign(message, key);
    }
}
