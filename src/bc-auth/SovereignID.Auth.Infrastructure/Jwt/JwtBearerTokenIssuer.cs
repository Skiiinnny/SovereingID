using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SovereignID.Auth.Domain;
using SovereignID.Auth.Domain.Ports;
using SovereignID.Auth.Infrastructure.Configuration;
using SovereignID.SharedKernel.Domain;

namespace SovereignID.Auth.Infrastructure.Jwt;

public sealed class JwtBearerTokenIssuer : IJwtTokenIssuer
{
    private readonly IClock clock;
    private readonly AuthOptions options;
    private readonly JwtSecurityTokenHandler handler = new();

    public JwtBearerTokenIssuer(IClock clock, IOptions<AuthOptions> options)
    {
        this.clock = clock;
        this.options = options.Value;
        ValidateSigningKey(this.options.JwtSigningKey);
    }

    public async Task<JwtToken> IssueAsync(EthereumAddress subject, TimeSpan lifetime, CancellationToken cancellationToken)
    {
        var now = await clock.GetUtcNowAsync(cancellationToken);
        var expiresAt = now.Add(lifetime);
        var normalized = subject.Value.ToLowerInvariant();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, normalized),
            new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new("address", normalized),
            new("did", $"did:ethr:sepolia:{normalized}")
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = options.JwtIssuer,
            Audience = options.JwtAudience,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.JwtSigningKey)),
                SecurityAlgorithms.HmacSha256)
        };

        var token = handler.CreateToken(descriptor);
        return new JwtToken(handler.WriteToken(token), expiresAt);
    }

    private static void ValidateSigningKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("AUTH_JWT_SIGNING_KEY is required.");
        }

        if (Encoding.UTF8.GetByteCount(key) < 32)
        {
            throw new InvalidOperationException("AUTH_JWT_SIGNING_KEY must be at least 32 bytes.");
        }
    }
}
