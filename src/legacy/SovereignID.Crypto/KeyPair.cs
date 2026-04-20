using Nethereum.Signer;

namespace SovereignID.Crypto;

/// <summary>Ethereum secp256k1 key material derived via Nethereum <see cref="EthECKey"/>.</summary>
public record KeyPair(string PrivateKey, string PublicKey, string Address)
{
    /// <summary>Creates a new random key pair.</summary>
    public static KeyPair Generate()
    {
        var key = EthECKey.GenerateKey();
        return FromEthECKey(key);
    }

    /// <summary>Loads a key pair from a hex private key (with or without 0x prefix).</summary>
    /// <param name="privateKey">32-byte secp256k1 secret scalar as hex.</param>
    public static KeyPair FromPrivateKey(string privateKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(privateKey);
        var key = new EthECKey(privateKey);
        return FromEthECKey(key);
    }

    private static KeyPair FromEthECKey(EthECKey key)
    {
        var privateHex = key.GetPrivateKey();
        var publicHex = "0x" + Convert.ToHexString(key.GetPubKey()).ToLowerInvariant();
        var address = key.GetPublicAddress();
        return new KeyPair(privateHex, publicHex, address);
    }
}
