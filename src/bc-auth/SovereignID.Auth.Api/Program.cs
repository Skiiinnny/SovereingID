using System.Text;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SovereignID.Auth.Api.Configuration;
using SovereignID.Auth.Api.Endpoints;
using SovereignID.Auth.Api.Infrastructure;
using SovereignID.Auth.Application.Nonce;
using SovereignID.Auth.Application.Verify;
using SovereignID.Auth.Domain;
using SovereignID.Auth.Domain.Ports;
using SovereignID.Auth.Infrastructure.Jwt;
using SovereignID.Auth.Infrastructure.Nonces;
using SovereignID.Auth.Infrastructure.Repositories;
using SovereignID.Auth.Infrastructure.Siwe;
using SovereignID.SharedKernel.Application;
using SovereignID.SharedKernel.Domain;
using InfrastructureAuthOptions = SovereignID.Auth.Infrastructure.Configuration.AuthOptions;

var builder = WebApplication.CreateBuilder(args);

var signingKeyFromEnvironment = Environment.GetEnvironmentVariable("AUTH_JWT_SIGNING_KEY");
if (!string.IsNullOrWhiteSpace(signingKeyFromEnvironment))
{
    builder.Configuration["Auth:JwtSigningKey"] = signingKeyFromEnvironment;
}
else if (!builder.Environment.IsDevelopment())
{
    throw new InvalidOperationException("AUTH_JWT_SIGNING_KEY is required outside Development.");
}

builder.Services
    .AddOptions<AuthOptions>()
    .Bind(builder.Configuration.GetSection("Auth"))
    .ValidateDataAnnotations();

builder.Services
    .AddOptions<InfrastructureAuthOptions>()
    .Bind(builder.Configuration.GetSection("Auth"));

builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<IGuidGenerator, GuidGenerator>();

builder.Services.AddSingleton<InMemoryAuthChallengeRepository>();
builder.Services.AddSingleton<IAuthChallengeRepository>(sp => sp.GetRequiredService<InMemoryAuthChallengeRepository>());
builder.Services.AddHostedService<AuthChallengeEvictionHostedService>();

builder.Services.AddScoped<INonceGenerator, SecureRandomNonceGenerator>();
builder.Services.AddScoped<ISiweMessageParser, ManualSiweMessageParser>();
builder.Services.AddScoped<ISiweSignatureVerifier, NethereumSiweSignatureVerifier>();
builder.Services.AddScoped<IJwtTokenIssuer, JwtBearerTokenIssuer>();

builder.Services.AddScoped<IQueryHandler<GenerateNonceQuery, GenerateNonceResult>>(sp =>
{
    var options = sp.GetRequiredService<IOptions<AuthOptions>>().Value;
    var nonceTtl = TimeSpan.FromSeconds(options.NonceTtlSeconds);
    return new GenerateNonceQueryHandler(
        sp.GetRequiredService<INonceGenerator>(),
        sp.GetRequiredService<IClock>(),
        sp.GetRequiredService<IAuthChallengeRepository>(),
        nonceTtl);
});
builder.Services.AddScoped<ICommandHandler<VerifySiweCommand, Result<VerifySiweResult, AuthError>>, VerifySiweCommandHandler>();

var authSection = builder.Configuration.GetSection("Auth");
var jwtIssuer = authSection["JwtIssuer"] ?? "sovereignid-auth";
var jwtAudience = authSection["JwtAudience"] ?? "sovereignid-clients";
var jwtSigningKey = authSection["JwtSigningKey"] ?? string.Empty;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

AuthEndpoints.MapAuth(app);

await app.RunAsync();

[ExcludeFromCodeCoverage]
public partial class Program
{
    protected Program()
    {
    }
}
