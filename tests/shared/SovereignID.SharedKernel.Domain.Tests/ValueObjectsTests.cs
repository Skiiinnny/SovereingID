namespace SovereignID.SharedKernel.Domain.Tests;

public class ValueObjectsTests
{
    [Fact]
    public void DecentralizedIdentifier_Create_PreservesValue()
    {
        const string input = "did:ethr:0x123";

        var value = DecentralizedIdentifier.Create(input);

        Assert.Equal(input, value.Value);
    }

    [Fact]
    public void EthereumAddress_Create_PreservesValue()
    {
        const string input = "0x1234567890abcdef1234567890abcdef12345678";

        var value = EthereumAddress.Create(input);

        Assert.Equal(input, value.Value);
    }

    [Fact]
    public void Sha256Hash_Create_PreservesValue()
    {
        const string input = "abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789";

        var value = Sha256Hash.Create(input);

        Assert.Equal(input, value.Value);
    }

    [Fact]
    public void PublicKey_Create_PreservesValue()
    {
        const string input = "0x04abcdef";

        var value = PublicKey.Create(input);

        Assert.Equal(input, value.Value);
    }

    [Fact]
    public void Signature_Create_PreservesValue()
    {
        const string input = "0xdeadbeef";

        var value = Signature.Create(input);

        Assert.Equal(input, value.Value);
    }

    [Fact]
    public void ValueObjects_WithSameValue_AreEqual()
    {
        var left = EthereumAddress.Create("0xabc");
        var right = EthereumAddress.Create("0xabc");

        Assert.Equal(left, right);
    }
}
