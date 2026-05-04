using SovereignID.Auth.Domain;
using SovereignID.Auth.Domain.Ports;
using SovereignID.SharedKernel.Application;
using SovereignID.SharedKernel.Domain;

namespace SovereignID.Auth.Application.Nonce;

public sealed class GenerateNonceQueryHandler(
    INonceGenerator nonceGenerator,
    IClock clock,
    IAuthChallengeRepository repository,
    TimeSpan nonceTtl)
    : IQueryHandler<GenerateNonceQuery, GenerateNonceResult>
{
    public async Task<GenerateNonceResult> HandleAsync(GenerateNonceQuery input, CancellationToken cancellationToken)
    {
        var challenge = await AuthChallenge.IssueAsync(nonceGenerator, clock, nonceTtl, cancellationToken);
        await repository.SaveAsync(challenge, cancellationToken);
        return new GenerateNonceResult(challenge.Nonce.Value, challenge.ExpiresAt);
    }
}
