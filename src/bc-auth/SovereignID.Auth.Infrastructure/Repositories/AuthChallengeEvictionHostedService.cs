using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SovereignID.SharedKernel.Domain;

namespace SovereignID.Auth.Infrastructure.Repositories;

public sealed class AuthChallengeEvictionHostedService(
    InMemoryAuthChallengeRepository repository,
    IClock clock,
    ILogger<AuthChallengeEvictionHostedService> logger)
    : BackgroundService
{
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
