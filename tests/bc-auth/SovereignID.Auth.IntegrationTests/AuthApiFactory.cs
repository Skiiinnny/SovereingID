using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SovereignID.Auth.Domain.Ports;
using SovereignID.SharedKernel.Domain;

namespace SovereignID.Auth.IntegrationTests;

public sealed class AuthApiFactory : WebApplicationFactory<Program>
{
    public TestClock Clock { get; } = new(new DateTimeOffset(2026, 4, 25, 12, 0, 0, TimeSpan.Zero));

    public DeterministicNonceGenerator Nonces { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Auth:JwtSigningKey", "0123456789abcdef0123456789abcdef");
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IClock>();
            services.RemoveAll<INonceGenerator>();

            services.AddSingleton<IClock>(Clock);
            services.AddSingleton<INonceGenerator>(Nonces);
        });
    }
}
