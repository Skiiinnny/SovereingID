namespace SovereignID.Auth.Domain.Ports;

/// <summary>
/// Stores and retrieves auth challenges by nonce.
/// </summary>
public interface IAuthChallengeRepository
{
    /// <summary>
    /// Saves the challenge.
    /// </summary>
    Task SaveAsync(AuthChallenge challenge, CancellationToken cancellationToken);

    /// <summary>
    /// Finds a challenge by nonce.
    /// </summary>
    Task<AuthChallenge?> FindByNonceAsync(Nonce nonce, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a challenge by nonce.
    /// </summary>
    Task DeleteAsync(Nonce nonce, CancellationToken cancellationToken);
}
