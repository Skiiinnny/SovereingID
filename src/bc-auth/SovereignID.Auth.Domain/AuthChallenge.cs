using SovereignID.SharedKernel.Domain;
using SovereignID.Auth.Domain.Ports;

namespace SovereignID.Auth.Domain;

public sealed class AuthChallenge
{
    private AuthChallenge(Nonce nonce, DateTimeOffset issuedAt, DateTimeOffset expiresAt)
    {
        Nonce = nonce;
        IssuedAt = issuedAt;
        ExpiresAt = expiresAt;
    }

    public Nonce Nonce { get; }
    public DateTimeOffset IssuedAt { get; }
    public DateTimeOffset ExpiresAt { get; }
    public bool IsConsumed { get; private set; }

    public static async Task<AuthChallenge> IssueAsync(
        INonceGenerator nonceGenerator,
        IClock clock,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        var nonce = await nonceGenerator.NewAsync(cancellationToken);
        var issuedAt = await clock.GetUtcNowAsync(cancellationToken);
        var expiresAt = issuedAt.Add(ttl);
        return new AuthChallenge(nonce, issuedAt, expiresAt);
    }

    public Result<Unit, AuthError> Consume(DateTimeOffset now)
    {
        if (IsConsumed)
        {
            return Result<Unit, AuthError>.Failure(AuthErrors.NonceConsumed(Nonce.Value));
        }

        if (now >= ExpiresAt)
        {
            return Result<Unit, AuthError>.Failure(AuthErrors.NonceExpired(now, ExpiresAt));
        }

        IsConsumed = true;
        return Result<Unit, AuthError>.Success(Unit.Value);
    }
}
