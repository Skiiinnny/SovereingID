using SovereignID.Auth.Domain;
using SovereignID.Auth.Domain.Ports;
using SovereignID.SharedKernel.Application;
using SovereignID.SharedKernel.Domain;

namespace SovereignID.Auth.Application.Nonce;

public sealed class GenerateNonceQueryHandler : IQueryHandler<GenerateNonceQuery, GenerateNonceResult>
{
    private readonly INonceGenerator nonceGenerator;
    private readonly IClock clock;
    private readonly IAuthChallengeRepository repository;
    private readonly TimeSpan nonceTtl;

    public GenerateNonceQueryHandler(
        INonceGenerator nonceGenerator,
        IClock clock,
        IAuthChallengeRepository repository,
        TimeSpan nonceTtl)
    {
        this.nonceGenerator = nonceGenerator;
        this.clock = clock;
        this.repository = repository;
        this.nonceTtl = nonceTtl;
    }

    public async Task<GenerateNonceResult> HandleAsync(GenerateNonceQuery input, CancellationToken cancellationToken)
    {
        var challenge = await AuthChallenge.IssueAsync(nonceGenerator, clock, nonceTtl, cancellationToken);
        await repository.SaveAsync(challenge, cancellationToken);
        return new GenerateNonceResult(challenge.Nonce.Value, challenge.ExpiresAt);
    }
}
