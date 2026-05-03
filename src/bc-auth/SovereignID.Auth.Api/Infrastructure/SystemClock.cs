using System.Diagnostics.CodeAnalysis;
using SovereignID.SharedKernel.Domain;

namespace SovereignID.Auth.Api.Infrastructure;

[ExcludeFromCodeCoverage]
public sealed class SystemClock : IClock
{
    public Task<DateTimeOffset> GetUtcNowAsync(CancellationToken cancellationToken) =>
        Task.FromResult(DateTimeOffset.UtcNow);
}
