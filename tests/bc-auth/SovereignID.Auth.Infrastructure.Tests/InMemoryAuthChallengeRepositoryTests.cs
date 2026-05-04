using SovereignID.Auth.Domain;
using SovereignID.Auth.Domain.Ports;
using SovereignID.Auth.Infrastructure.Repositories;
using SovereignID.SharedKernel.Domain;

namespace SovereignID.Auth.Infrastructure.Tests;

public class InMemoryAuthChallengeRepositoryTests
{
    [Fact]
    public async Task SaveAndFind_Roundtrip_Works()
    {
        var repo = new InMemoryAuthChallengeRepository();
        var challenge = await CreateChallengeAsync();
        await repo.SaveAsync(challenge, CancellationToken.None);

        var found = await repo.FindByNonceAsync(challenge.Nonce, CancellationToken.None);

        Assert.NotNull(found);
        Assert.Equal(challenge.Nonce, found!.Nonce);
    }

    [Fact]
    public async Task Delete_RemovesChallenge()
    {
        var repo = new InMemoryAuthChallengeRepository();
        var challenge = await CreateChallengeAsync();
        await repo.SaveAsync(challenge, CancellationToken.None);
        await repo.DeleteAsync(challenge.Nonce, CancellationToken.None);

        var found = await repo.FindByNonceAsync(challenge.Nonce, CancellationToken.None);

        Assert.Null(found);
    }

    private static Task<AuthChallenge> CreateChallengeAsync()
    {
        return AuthChallenge.IssueAsync(
            new FakeNonceGenerator(Nonce.Create("0123456789abcdef0123456789abcdef")),
            new FakeClock(new DateTimeOffset(2026, 4, 25, 12, 0, 0, TimeSpan.Zero)),
            TimeSpan.FromMinutes(10),
            CancellationToken.None);
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
