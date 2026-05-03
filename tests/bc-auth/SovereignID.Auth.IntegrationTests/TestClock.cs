using SovereignID.SharedKernel.Domain;

namespace SovereignID.Auth.IntegrationTests;

public sealed class TestClock : IClock
{
    public TestClock(DateTimeOffset now) => Now = now;

    public DateTimeOffset Now { get; private set; }

    public Task<DateTimeOffset> GetUtcNowAsync(CancellationToken cancellationToken) =>
        Task.FromResult(Now);

    public void Advance(TimeSpan delta) => Now = Now.Add(delta);
}
