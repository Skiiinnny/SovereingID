namespace SovereignID.Crypto.Tests;

public class KeyPairTests
{
    [Fact]
    public void Generate_Address_Is0xPlus40Hex()
    {
        var kp = KeyPair.Generate();
        Assert.Matches("^0x[a-fA-F0-9]{40}$", kp.Address);
    }

    [Fact]
    public void FromPrivateKey_IsDeterministic()
    {
        var generated = KeyPair.Generate();
        var pk = generated.PrivateKey;
        var a = KeyPair.FromPrivateKey(pk);
        var b = KeyPair.FromPrivateKey(pk);
        Assert.Equal(a.Address, b.Address, StringComparer.OrdinalIgnoreCase);
        Assert.Equal(a.PrivateKey, b.PrivateKey, StringComparer.OrdinalIgnoreCase);
        Assert.Equal(a.PublicKey, b.PublicKey, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void FromPrivateKey_Without0xPrefix_Works()
    {
        var generated = KeyPair.Generate();
        var pkWithPrefix = generated.PrivateKey;
        Assert.StartsWith("0x", pkWithPrefix, StringComparison.OrdinalIgnoreCase);
        var pkNoPrefix = pkWithPrefix.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? pkWithPrefix[2..]
            : pkWithPrefix;
        var roundTrip = KeyPair.FromPrivateKey(pkNoPrefix);
        Assert.Equal(generated.Address, roundTrip.Address, StringComparer.OrdinalIgnoreCase);
    }
}
