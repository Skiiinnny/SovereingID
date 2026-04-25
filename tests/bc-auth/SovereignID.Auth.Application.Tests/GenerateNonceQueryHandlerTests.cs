using SovereignID.Auth.Application.Nonce;
using SovereignID.Auth.Domain;
using SovereignID.Auth.Domain.Ports;
using SovereignID.SharedKernel.Domain;
using DomainNonce = SovereignID.Auth.Domain.Nonce;

namespace SovereignID.Auth.Application.Tests;

public class GenerateNonceQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsFreshNonce_AndSavesOnce()
    {
        var nonce = DomainNonce.Create("0123456789abcdef0123456789abcdef");
        var nonceGenerator = new FakeNonceGenerator(nonce);
        var clock = new FakeClock(new DateTimeOffset(2026, 4, 25, 10, 0, 0, TimeSpan.Zero));
        var repository = new SpyRepository();
        var handler = new GenerateNonceQueryHandler(nonceGenerator, clock, repository);

        var result = await handler.HandleAsync(new GenerateNonceQuery(), CancellationToken.None);

        Assert.Equal(nonce.Value, result.Nonce);
        Assert.Equal(1, repository.SaveCalls);
    }

    [Fact]
    public async Task HandleAsync_ExpiresAt_IsClockPlusTenMinutes()
    {
        var now = new DateTimeOffset(2026, 4, 25, 10, 0, 0, TimeSpan.Zero);
        var nonceGenerator = new FakeNonceGenerator(DomainNonce.Create("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"));
        var clock = new FakeClock(now);
        var repository = new SpyRepository();
        var handler = new GenerateNonceQueryHandler(nonceGenerator, clock, repository);

        var result = await handler.HandleAsync(new GenerateNonceQuery(), CancellationToken.None);

        Assert.Equal(now.AddMinutes(10), result.ExpiresAt);
    }

    private sealed class SpyRepository : IAuthChallengeRepository
    {
        public int SaveCalls { get; private set; }

        public Task SaveAsync(AuthChallenge challenge, CancellationToken cancellationToken)
        {
            SaveCalls++;
            return Task.CompletedTask;
        }

        public Task<AuthChallenge?> FindByNonceAsync(DomainNonce nonce, CancellationToken cancellationToken)
            => Task.FromResult<AuthChallenge?>(null);

        public Task DeleteAsync(DomainNonce nonce, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private sealed class FakeNonceGenerator : INonceGenerator
    {
        private readonly DomainNonce nonce;
        public FakeNonceGenerator(DomainNonce nonce) => this.nonce = nonce;
        public Task<DomainNonce> NewAsync(CancellationToken cancellationToken) => Task.FromResult(nonce);
    }

    private sealed class FakeClock : IClock
    {
        private readonly DateTimeOffset now;
        public FakeClock(DateTimeOffset now) => this.now = now;
        public Task<DateTimeOffset> GetUtcNowAsync(CancellationToken cancellationToken) => Task.FromResult(now);
    }
}
