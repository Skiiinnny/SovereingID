using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Nethereum.Signer;

namespace SovereignID.Auth.IntegrationTests;

public sealed class AuthEndpointsTests : IClassFixture<AuthApiFactory>
{
    private readonly HttpClient client;
    private readonly AuthApiFactory factory;
    private readonly EthECKey signer = EthECKey.GenerateKey();

    public AuthEndpointsTests(AuthApiFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
    }

    [Fact]
    public async Task HappyPath_ReturnsJwtAndAddress()
    {
        var nonce = await GetNonceAsync();
        var address = signer.GetPublicAddress();
        var message = BuildMessage(address, nonce, chainId: 11155111, includeVersion: true);
        var signature = Sign(message);

        var response = await PostVerifyAsync(message, signature);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await ReadJsonAsync(response);
        payload.GetProperty("address").GetString().Should().BeEquivalentTo(address);
        var jwt = payload.GetProperty("jwt").GetString();
        jwt.Should().NotBeNullOrWhiteSpace();

        var jwtHandler = new JwtSecurityTokenHandler();
        var token = jwtHandler.ReadJwtToken(jwt);
        token.Claims.First(c => c.Type == "sub").Value.Should().Be(address.ToLowerInvariant());
    }

    [Fact]
    public async Task Replay_ReturnsNonceConsumed()
    {
        var nonce = await GetNonceAsync();
        var address = signer.GetPublicAddress();
        var message = BuildMessage(address, nonce, chainId: 11155111, includeVersion: true);
        var signature = Sign(message);

        var first = await PostVerifyAsync(message, signature);
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        var second = await PostVerifyAsync(message, signature);
        second.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await ReadErrorCodeAsync(second)).Should().Be("nonce_consumed");
    }

    [Fact]
    public async Task ExpiredNonce_ReturnsNonceExpired()
    {
        var nonce = await GetNonceAsync();
        factory.Clock.Advance(TimeSpan.FromMinutes(11));

        var address = signer.GetPublicAddress();
        var message = BuildMessage(address, nonce, chainId: 11155111, includeVersion: true);
        var signature = Sign(message);

        var response = await PostVerifyAsync(message, signature);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await ReadErrorCodeAsync(response)).Should().Be("nonce_expired");
    }

    [Fact]
    public async Task WrongChainId_ReturnsUnsupportedChain()
    {
        var nonce = await GetNonceAsync();
        var address = signer.GetPublicAddress();
        var message = BuildMessage(address, nonce, chainId: 1, includeVersion: true);
        var signature = Sign(message);

        var response = await PostVerifyAsync(message, signature);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ReadErrorCodeAsync(response)).Should().Be("unsupported_chain");
    }

    [Fact]
    public async Task TamperedMessage_ReturnsSignatureMismatch()
    {
        var nonce = await GetNonceAsync();
        var address = signer.GetPublicAddress();
        var original = BuildMessage(address, nonce, chainId: 11155111, includeVersion: true);
        var tampered = original.Replace("Sign in to SovereignID demo", "Sign in to another app", StringComparison.Ordinal);
        var signature = Sign(original);

        var response = await PostVerifyAsync(tampered, signature);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await ReadErrorCodeAsync(response)).Should().Be("signature_mismatch");
    }

    [Fact]
    public async Task UnknownNonce_ReturnsNonceUnknown()
    {
        var unknownNonce = "dddddddddddddddddddddddddddddddd";
        var address = signer.GetPublicAddress();
        var message = BuildMessage(address, unknownNonce, chainId: 11155111, includeVersion: true);
        var signature = Sign(message);

        var response = await PostVerifyAsync(message, signature);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await ReadErrorCodeAsync(response)).Should().Be("nonce_unknown");
    }

    [Fact]
    public async Task MalformedPayload_ReturnsSiweParseFailed()
    {
        var nonce = await GetNonceAsync();
        var address = signer.GetPublicAddress();
        var message = BuildMessage(address, nonce, chainId: 11155111, includeVersion: false);
        var signature = Sign(message);

        var response = await PostVerifyAsync(message, signature);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ReadErrorCodeAsync(response)).Should().Be("siwe_parse_failed");
        (await ReadJsonAsync(response)).GetProperty("detail").GetString().Should().NotBeNullOrWhiteSpace();
    }

    private async Task<string> GetNonceAsync()
    {
        var response = await client.GetAsync("/auth/nonce");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await ReadJsonAsync(response);
        return payload.GetProperty("nonce").GetString()!;
    }

    private Task<HttpResponseMessage> PostVerifyAsync(string message, string signature) =>
        client.PostAsJsonAsync("/auth/verify", new { message, signature });

    private string Sign(string message)
    {
        var signerUtility = new EthereumMessageSigner();
        return signerUtility.EncodeUTF8AndSign(message, signer);
    }

    private static string BuildMessage(string address, string nonce, int chainId, bool includeVersion)
    {
        var sb = new StringBuilder();
        sb.AppendLine("localhost wants you to sign in with your Ethereum account:");
        sb.AppendLine(address);
        sb.AppendLine();
        sb.AppendLine("Sign in to SovereignID demo");
        sb.AppendLine();
        sb.AppendLine("URI: http://localhost");
        if (includeVersion)
        {
            sb.AppendLine("Version: 1");
        }

        sb.AppendLine($"Chain ID: {chainId}");
        sb.AppendLine($"Nonce: {nonce}");
        sb.Append($"Issued At: {DateTimeOffset.UtcNow:O}");
        return sb.ToString();
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(content);
        return json.RootElement.Clone();
    }

    private static async Task<string?> ReadErrorCodeAsync(HttpResponseMessage response)
    {
        var json = await ReadJsonAsync(response);
        return json.TryGetProperty("error", out var error) ? error.GetString() : null;
    }
}
