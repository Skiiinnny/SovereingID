namespace SovereignID.Auth.Api.Contracts;

/// <summary>
/// Response returned when a SIWE payload is successfully verified.
/// </summary>
/// <param name="Jwt">Issued bearer token.</param>
/// <param name="Address">Recovered Ethereum address.</param>
/// <param name="ExpiresAt">JWT expiration timestamp in UTC.</param>
public sealed record VerifyResponse(string Jwt, string Address, DateTimeOffset ExpiresAt);
