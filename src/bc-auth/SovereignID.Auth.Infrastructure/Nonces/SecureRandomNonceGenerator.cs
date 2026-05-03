using System.Security.Cryptography;
using SovereignID.Auth.Domain;
using SovereignID.Auth.Domain.Ports;

namespace SovereignID.Auth.Infrastructure.Nonces;

public sealed class SecureRandomNonceGenerator : INonceGenerator
{
    public Task<Nonce> NewAsync(CancellationToken cancellationToken)
    {
        var bytes = RandomNumberGenerator.GetBytes(16);
        var value = Convert.ToHexString(bytes).ToLowerInvariant();
        return Task.FromResult(Nonce.Create(value));
    }
}
