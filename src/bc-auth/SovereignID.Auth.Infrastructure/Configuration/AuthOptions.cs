namespace SovereignID.Auth.Infrastructure.Configuration;

public sealed record AuthOptions
{
    public string JwtSigningKey { get; init; } = string.Empty;
    public string JwtIssuer { get; init; } = "sovereignid-auth";
    public string JwtAudience { get; init; } = "sovereignid-clients";
}
