namespace SovereignID.Auth.Domain;

/// <summary>
/// Exception raised when a domain invariant is violated.
/// </summary>
public sealed class AuthDomainException : Exception
{
    public AuthDomainException(AuthError error)
        : base(error.Detail)
    {
        Error = error;
    }

    /// <summary>
    /// Gets the typed error associated with the failure.
    /// </summary>
    public AuthError Error { get; }
}
