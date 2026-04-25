namespace SovereignID.Auth.Application.Nonce;

public sealed record GenerateNonceResult(string Nonce, DateTimeOffset ExpiresAt);
