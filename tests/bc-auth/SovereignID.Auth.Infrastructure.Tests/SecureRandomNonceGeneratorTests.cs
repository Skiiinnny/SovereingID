using SovereignID.Auth.Domain;
using SovereignID.Auth.Infrastructure.Nonces;

namespace SovereignID.Auth.Infrastructure.Tests;

public class SecureRandomNonceGeneratorTests
{
    [Fact]
    public async Task TwoConsecutiveCalls_AreUsuallyDistinct()
    {
        var generator = new SecureRandomNonceGenerator();
        var values = new HashSet<string>(StringComparer.Ordinal);

        for (var i = 0; i < 1000; i++)
        {
            values.Add((await generator.NewAsync(CancellationToken.None)).Value);
        }

        Assert.True(values.Count > 990);
    }

    [Fact]
    public async Task Output_AlwaysMatchesNonceConstraints()
    {
        var generator = new SecureRandomNonceGenerator();
        var nonce = await generator.NewAsync(CancellationToken.None);

        var reparsed = Nonce.Create(nonce.Value);
        Assert.Equal(nonce, reparsed);
    }
}
