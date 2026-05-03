using SovereignID.SharedKernel.Domain;

namespace SovereignID.Auth.Domain.Ports;

/// <summary>
/// Issues JWT tokens for authenticated Ethereum subjects.
/// </summary>
public interface IJwtTokenIssuer
{
    /// <summary>
    /// Issues a signed token for the given subject and lifetime.
    /// </summary>
    Task<JwtToken> IssueAsync(EthereumAddress subject, TimeSpan lifetime, CancellationToken cancellationToken);
}
