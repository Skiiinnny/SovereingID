using SovereignID.SharedKernel.Domain;
using SovereignID.Verifier.Application.Presentation;

namespace SovereignID.Verifier.Application.Tests;

public sealed class VerifyPresentationCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_delegates_to_PresentationVerifier_with_clock_now()
    {
        var instant = DateTimeOffset.Parse("2026-05-03T12:00:00Z", System.Globalization.CultureInfo.InvariantCulture);
        var handler = new VerifyPresentationCommandHandler(new FixedClock(instant));
        var outcome = await handler.HandleAsync(
            new VerifyPresentationCommand("   "),
            CancellationToken.None);

        Assert.False(outcome.IsSuccess);
        Assert.Equal("vp_json_empty", outcome.ErrorCode);
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public Task<DateTimeOffset> GetUtcNowAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(utcNow);
        }
    }
}
