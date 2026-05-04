namespace SovereignID.Auth.Domain;

/// <summary>
/// Exception raised when a domain invariant is violated.
/// </summary>
public sealed class AuthDomainException(AuthError error) : Exception(error.Detail)
{
    /// <summary>
    /// Gets the typed error associated with the failure.
    /// </summary>
    public AuthError Error { get; } = error;
}
