using SovereignID.Auth.Domain;
using SovereignID.Auth.Domain.Ports;
using SovereignID.SharedKernel.Application;
using SovereignID.SharedKernel.Domain;

namespace SovereignID.Auth.Application.Nonce;

public sealed class GenerateNonceQueryHandler : IQueryHandler<GenerateNonceQuery, GenerateNonceResult>
{
    private static readonly TimeSpan NonceTtl = TimeSpan.FromMinutes(10);

    private readonly INonceGenerator nonceGenerator;
    private readonly IClock clock;
    private readonly IAuthChallengeRepository repository;

    public GenerateNonceQueryHandler(INonceGenerator nonceGenerator, IClock clock, IAuthChallengeRepository repository)
    {
        this.nonceGenerator = nonceGenerator;
        this.clock = clock;
        this.repository = repository;
    }

    public async Task<GenerateNonceResult> HandleAsync(GenerateNonceQuery input, CancellationToken cancellationToken)
    {
        var challenge = await AuthChallenge.IssueAsync(nonceGenerator, clock, NonceTtl, cancellationToken);
        await repository.SaveAsync(challenge, cancellationToken);
        return new GenerateNonceResult(challenge.Nonce.Value, challenge.ExpiresAt);
    }
}
