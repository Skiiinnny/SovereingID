using SovereignID.Auth.Domain;
using SovereignID.Auth.Domain.Ports;
using SovereignID.SharedKernel.Application;
using SovereignID.SharedKernel.Domain;

namespace SovereignID.Auth.Application.Verify;

public sealed class VerifySiweCommandHandler : ICommandHandler<VerifySiweCommand, Result<VerifySiweResult, AuthError>>
{
    private static readonly TimeSpan SessionTtl = TimeSpan.FromHours(24);

    private readonly ISiweMessageParser parser;
    private readonly IAuthChallengeRepository repository;
    private readonly IClock clock;
    private readonly ISiweSignatureVerifier signatureVerifier;
    private readonly IJwtTokenIssuer jwtTokenIssuer;

    public VerifySiweCommandHandler(
        ISiweMessageParser parser,
        IAuthChallengeRepository repository,
        IClock clock,
        ISiweSignatureVerifier signatureVerifier,
        IJwtTokenIssuer jwtTokenIssuer)
    {
        this.parser = parser;
        this.repository = repository;
        this.clock = clock;
        this.signatureVerifier = signatureVerifier;
        this.jwtTokenIssuer = jwtTokenIssuer;
    }

    public async Task<Result<VerifySiweResult, AuthError>> HandleAsync(VerifySiweCommand input, CancellationToken cancellationToken)
    {
        var parseResult = await parser.ParseAsync(input.Message, cancellationToken);
        if (parseResult.IsFailure)
        {
            return Result<VerifySiweResult, AuthError>.Failure(parseResult.Error!);
        }

        var siweMessage = parseResult.Value!;

        var challenge = await repository.FindByNonceAsync(siweMessage.Nonce, cancellationToken);
        if (challenge is null)
        {
            return Result<VerifySiweResult, AuthError>.Failure(AuthErrors.NonceUnknown(siweMessage.Nonce.Value));
        }

        try
        {
            _ = ChainId.Create(siweMessage.ChainId);
        }
        catch (AuthDomainException ex)
        {
            return Result<VerifySiweResult, AuthError>.Failure(ex.Error);
        }

        var recoveredAddress = await signatureVerifier.RecoverAddressAsync(
            siweMessage.OriginalPayload,
            Signature.Create(input.Signature),
            cancellationToken);

        if (!string.Equals(recoveredAddress.Value, siweMessage.Address.Value, StringComparison.OrdinalIgnoreCase))
        {
            return Result<VerifySiweResult, AuthError>.Failure(
                AuthErrors.SignatureMismatch(siweMessage.Address.Value, recoveredAddress.Value));
        }

        var now = await clock.GetUtcNowAsync(cancellationToken);
        var consumeResult = challenge.Consume(now);
        if (consumeResult.IsFailure)
        {
            return Result<VerifySiweResult, AuthError>.Failure(consumeResult.Error!);
        }

        await repository.DeleteAsync(siweMessage.Nonce, cancellationToken);
        var token = await jwtTokenIssuer.IssueAsync(recoveredAddress, SessionTtl, cancellationToken);
        return Result<VerifySiweResult, AuthError>.Success(new VerifySiweResult(token.Value, recoveredAddress.Value, token.ExpiresAt));
    }
}
