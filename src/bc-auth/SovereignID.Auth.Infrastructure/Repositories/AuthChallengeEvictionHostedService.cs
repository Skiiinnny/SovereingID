using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SovereignID.SharedKernel.Domain;

namespace SovereignID.Auth.Infrastructure.Repositories;

public sealed class AuthChallengeEvictionHostedService : BackgroundService
{
    private readonly InMemoryAuthChallengeRepository repository;
    private readonly IClock clock;
    private readonly ILogger<AuthChallengeEvictionHostedService> logger;

    public AuthChallengeEvictionHostedService(
        InMemoryAuthChallengeRepository repository,
        IClock clock,
        ILogger<AuthChallengeEvictionHostedService> logger)
    {
        this.repository = repository;
        this.clock = clock;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (!stoppingToken.IsCancellationRequested &&
               await timer.WaitForNextTickAsync(stoppingToken))
        {
            var now = await clock.GetUtcNowAsync(stoppingToken);
            var removed = repository.EvictExpired(now);
            if (removed > 0)
            {
                logger.LogInformation("Evicted {Count} expired auth challenges", removed);
            }
        }
    }
}
