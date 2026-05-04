using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using SovereignID.Auth.Infrastructure.Configuration;
using SovereignID.Auth.Infrastructure.Jwt;
using SovereignID.SharedKernel.Domain;

namespace SovereignID.Auth.Infrastructure.Tests;

public class JwtBearerTokenIssuerTests
{
    [Fact]
    public async Task IssueAsync_TokenContainsExpectedClaims()
    {
        var now = new DateTimeOffset(2026, 4, 25, 16, 0, 0, TimeSpan.Zero);
        var issuer = CreateIssuer(now);
        var subject = EthereumAddress.Create("0xA0b86991c6218b36c1d19d4a2e9eb0ce3606eb48");

        var token = await issuer.IssueAsync(subject, TimeSpan.FromHours(24), CancellationToken.None);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.Value);

        Assert.Equal("sovereignid-auth", jwt.Issuer);
        Assert.Contains("sovereignid-clients", jwt.Audiences);
        Assert.Equal(subject.Value.ToLowerInvariant(), jwt.Subject);
        Assert.Equal(subject.Value.ToLowerInvariant(), jwt.Claims.Single(c => c.Type == "address").Value);
        Assert.Equal($"did:ethr:sepolia:{subject.Value.ToLowerInvariant()}", jwt.Claims.Single(c => c.Type == "did").Value);
    }

    [Fact]
    public async Task IssueAsync_ExpirationIsNowPlusLifetime()
    {
        var now = new DateTimeOffset(2026, 4, 25, 16, 0, 0, TimeSpan.Zero);
        var issuer = CreateIssuer(now);

        var token = await issuer.IssueAsync(EthereumAddress.Create("0xA0b86991c6218b36c1d19d4a2e9eb0ce3606eb48"), TimeSpan.FromHours(24), CancellationToken.None);

        Assert.Equal(now.AddHours(24), token.ExpiresAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("short-key")]
    public void Constructor_InvalidSigningKey_FailsFast(string key)
    {
        var options = Options.Create(new AuthOptions { JwtSigningKey = key });

        Assert.Throws<InvalidOperationException>(() => new JwtBearerTokenIssuer(new FakeClock(DateTimeOffset.UtcNow), options));
    }

    private static JwtBearerTokenIssuer CreateIssuer(DateTimeOffset now)
    {
        var options = Options.Create(new AuthOptions
        {
            JwtSigningKey = "this-is-a-very-secure-signing-key-32bytes",
            JwtIssuer = "sovereignid-auth",
            JwtAudience = "sovereignid-clients"
        });

        return new JwtBearerTokenIssuer(new FakeClock(now), options);
    }

    private sealed class FakeClock(DateTimeOffset now) : IClock
    {
        public Task<DateTimeOffset> GetUtcNowAsync(CancellationToken cancellationToken) => Task.FromResult(now);
    }
}
