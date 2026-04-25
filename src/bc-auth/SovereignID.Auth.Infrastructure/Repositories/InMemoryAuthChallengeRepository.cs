using System.Collections.Concurrent;
using SovereignID.Auth.Domain;
using SovereignID.Auth.Domain.Ports;

namespace SovereignID.Auth.Infrastructure.Repositories;

public sealed class InMemoryAuthChallengeRepository : IAuthChallengeRepository
{
    private readonly ConcurrentDictionary<Nonce, AuthChallenge> storage = new();

    public Task SaveAsync(AuthChallenge challenge, CancellationToken cancellationToken)
    {
        storage[challenge.Nonce] = challenge;
        return Task.CompletedTask;
    }

    public Task<AuthChallenge?> FindByNonceAsync(Nonce nonce, CancellationToken cancellationToken)
    {
        storage.TryGetValue(nonce, out var challenge);
        return Task.FromResult(challenge);
    }

    public Task DeleteAsync(Nonce nonce, CancellationToken cancellationToken)
    {
        if (!storage.TryGetValue(nonce, out var challenge))
        {
            return Task.CompletedTask;
        }

        // Keep consumed challenges until eviction so replay attempts can be
        // classified as nonce_consumed. Fresh entries are removed.
        if (!challenge.IsConsumed)
        {
            _ = storage.TryRemove(nonce, out _);
        }

        return Task.CompletedTask;
    }

    public int EvictExpired(DateTimeOffset now)
    {
        var removed = 0;
        foreach (var entry in storage)
        {
            if (entry.Value.ExpiresAt <= now && storage.TryRemove(entry.Key, out _))
            {
                removed++;
            }
        }

        return removed;
    }
}
