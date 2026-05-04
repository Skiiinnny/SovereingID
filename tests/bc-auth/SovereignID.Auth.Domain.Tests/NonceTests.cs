namespace SovereignID.Auth.Domain.Tests;

public class NonceTests
{
    [Fact]
    public void Create_WithValid32Hex_Lowercase_Works()
    {
        var nonce = Nonce.Create("0123456789abcdef0123456789abcdef");
        Assert.Equal("0123456789abcdef0123456789abcdef", nonce.Value);
    }

    [Theory]
    [InlineData("0123")]
    [InlineData("0123456789abcdef0123456789abcdeff")]
    [InlineData("0123456789abcdef0123456789abcdeg")]
    [InlineData("0123456789ABCDEF0123456789ABCDEF")]
    public void Create_WithInvalidFormat_Throws(string value)
    {
        Assert.Throws<ArgumentException>(() => Nonce.Create(value));
    }
}
