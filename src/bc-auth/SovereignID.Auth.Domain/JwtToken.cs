namespace SovereignID.Auth.Domain;

public sealed record JwtToken(string Value, DateTimeOffset ExpiresAt);
