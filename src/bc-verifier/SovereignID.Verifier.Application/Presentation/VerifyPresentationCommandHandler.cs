using SovereignID.SharedKernel.Application;
using SovereignID.SharedKernel.Domain;

namespace SovereignID.Verifier.Application.Presentation;

/// <summary>
/// Obtiene el instante actual vía <see cref="IClock"/> y delega en <see cref="PresentationVerifier"/>.
/// </summary>
public sealed class VerifyPresentationCommandHandler(IClock clock)
    : ICommandHandler<VerifyPresentationCommand, PresentationVerificationOutcome>
{
    /// <inheritdoc />
    public async Task<PresentationVerificationOutcome> HandleAsync(
        VerifyPresentationCommand input,
        CancellationToken cancellationToken)
    {
        var now = await clock.GetUtcNowAsync(cancellationToken).ConfigureAwait(false);
        return PresentationVerifier.Verify(input.PresentationJson, now);
    }
}
