using SovereignID.Auth.Domain;
using SovereignID.Auth.Domain.Ports;

namespace SovereignID.Auth.IntegrationTests;

public sealed class DeterministicNonceGenerator : INonceGenerator
{
    private int index;

    public Task<Nonce> NewAsync(CancellationToken cancellationToken)
    {
        var next = Interlocked.Increment(ref index);
        var nonce = next.ToString("x").PadLeft(32, '0');
        return Task.FromResult(Nonce.Create(nonce));
    }
}
