namespace SovereignID.Auth.Application.Verify;

public sealed record VerifySiweResult(string Jwt, string Address, DateTimeOffset ExpiresAt);
