using SovereignID.Crypto;

namespace SovereignID.Crypto.Tests;

public class HashHelperTests
{
    [Fact]
    public void Sha256_IsStableAndBytes32Formatted()
    {
        var h1 = HashHelper.Sha256("Hello World v1");
        var h2 = HashHelper.Sha256("Hello World v1");
        Assert.Equal(h1, h2);
        Assert.StartsWith("0x", h1, StringComparison.Ordinal);
        Assert.Equal(66, h1.Length);
    }

    [Fact]
    public void Sha256Bytes_MatchesSha256Hex()
    {
        const string content = "Hello World v1";
        var hex = HashHelper.Sha256(content);
        var bytes = HashHelper.Sha256Bytes(content);
        var roundTrip = "0x" + Convert.ToHexString(bytes).ToLowerInvariant();
        Assert.Equal(hex, roundTrip, StringComparer.OrdinalIgnoreCase);
    }
}
