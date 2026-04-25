namespace SovereignID.Auth.Domain;

/// <summary>
/// Represents a typed authentication domain error.
/// </summary>
/// <param name="Code">Stable machine-readable error code.</param>
/// <param name="Detail">Human-readable detail for diagnostics.</param>
public sealed record AuthError(string Code, string Detail);
