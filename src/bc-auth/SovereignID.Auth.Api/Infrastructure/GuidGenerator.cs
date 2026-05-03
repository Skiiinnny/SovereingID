using System.Diagnostics.CodeAnalysis;
using SovereignID.SharedKernel.Domain;

namespace SovereignID.Auth.Api.Infrastructure;

[ExcludeFromCodeCoverage]
public sealed class GuidGenerator : IGuidGenerator
{
    public Task<Guid> NewGuidAsync(CancellationToken cancellationToken) =>
        Task.FromResult(Guid.NewGuid());
}
