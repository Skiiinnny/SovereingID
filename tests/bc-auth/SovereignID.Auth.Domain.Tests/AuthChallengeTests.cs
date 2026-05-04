using SovereignID.Auth.Domain.Ports;
using SovereignID.SharedKernel.Domain;

namespace SovereignID.Auth.Domain.Tests;

public class AuthChallengeTests
{
    [Fact]
    public async Task Consume_FreshChallenge_SucceedsAndMarksConsumed()
    {
        var challenge = await CreateChallengeAsync(
            new DateTimeOffset(2026, 4, 25, 10, 0, 0, TimeSpan.Zero),
            TimeSpan.FromMinutes(10));

        var result = challenge.Consume(new DateTimeOffset(2026, 4, 25, 10, 5, 0, TimeSpan.Zero));

        Assert.True(result.IsSuccess);
        Assert.True(challenge.IsConsumed);
    }

    [Fact]
    public async Task Consume_Twice_ReturnsNonceConsumed()
    {
        var challenge = await CreateChallengeAsync(
            new DateTimeOffset(2026, 4, 25, 10, 0, 0, TimeSpan.Zero),
            TimeSpan.FromMinutes(10));
        _ = challenge.Consume(new DateTimeOffset(2026, 4, 25, 10, 5, 0, TimeSpan.Zero));

        var second = challenge.Consume(new DateTimeOffset(2026, 4, 25, 10, 6, 0, TimeSpan.Zero));

        Assert.True(second.IsFailure);
        Assert.Equal("nonce_consumed", second.Error?.Code);
    }

    [Fact]
    public async Task Consume_AfterExpiration_ReturnsNonceExpired()
    {
        var challenge = await CreateChallengeAsync(
            new DateTimeOffset(2026, 4, 25, 10, 0, 0, TimeSpan.Zero),
            TimeSpan.FromMinutes(10));

        var result = challenge.Consume(new DateTimeOffset(2026, 4, 25, 10, 10, 0, TimeSpan.Zero));

        Assert.True(result.IsFailure);
        Assert.Equal("nonce_expired", result.Error?.Code);
    }

    [Fact]
    public async Task IssueAsync_UsesClockAndHonorsTtl()
    {
        var issuedAt = new DateTimeOffset(2026, 4, 25, 12, 0, 0, TimeSpan.Zero);
        var nonce = Nonce.Create("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        var nonceGenerator = new FakeNonceGenerator(nonce);
        var clock = new FakeClock(issuedAt);

        var challenge = await AuthChallenge.IssueAsync(nonceGenerator, clock, TimeSpan.FromMinutes(10), CancellationToken.None);

        Assert.Equal(nonce, challenge.Nonce);
        Assert.Equal(issuedAt, challenge.IssuedAt);
        Assert.Equal(issuedAt.AddMinutes(10), challenge.ExpiresAt);
    }

    private static async Task<AuthChallenge> CreateChallengeAsync(DateTimeOffset issuedAt, TimeSpan ttl)
    {
        var nonce = Nonce.Create("0123456789abcdef0123456789abcdef");
        var nonceGenerator = new FakeNonceGenerator(nonce);
        var clock = new FakeClock(issuedAt);
        return await AuthChallenge.IssueAsync(nonceGenerator, clock, ttl, CancellationToken.None);
    }

    private sealed class FakeNonceGenerator(Nonce nonce) : INonceGenerator
    {
        public Task<Nonce> NewAsync(CancellationToken cancellationToken) => Task.FromResult(nonce);
    }

    private sealed class FakeClock(DateTimeOffset now) : IClock
    {
        public Task<DateTimeOffset> GetUtcNowAsync(CancellationToken cancellationToken) => Task.FromResult(now);
    }
}
