using SovereignID.SharedKernel.Domain;

namespace SovereignID.Auth.IntegrationTests;

public sealed class TestClock(DateTimeOffset now) : IClock
{
    public DateTimeOffset Now { get; private set; } = now;

    public Task<DateTimeOffset> GetUtcNowAsync(CancellationToken cancellationToken) =>
        Task.FromResult(Now);

    public void Advance(TimeSpan delta) => Now = Now.Add(delta);
}
