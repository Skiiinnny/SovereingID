using SovereignID.Auth.Domain;
using SovereignID.Auth.Infrastructure.Siwe;

namespace SovereignID.Auth.Infrastructure.Tests;

public class ManualSiweMessageParserTests
{
    [Fact]
    public async Task Parse_CanonicalEip4361Example_ParsesAllFields()
    {
        var parser = new ManualSiweMessageParser();
        var payload = CanonicalPayload();

        var message = await parser.ParseAsync(payload, CancellationToken.None);

        Assert.Equal("service.org", message.Domain);
        Assert.Equal("0xC02aaa39b223FE8D0A0e5C4F27eAD9083C756Cc2", message.Address.Value);
        Assert.Equal(11155111, message.ChainId);
        Assert.Equal("32891756b8f249f39ac0f53f6f38f9dd", message.Nonce.Value);
        Assert.Equal(2, message.Resources.Count);
    }

    [Fact]
    public async Task Parse_MissingChainId_ThrowsSiweParseFailed()
    {
        var parser = new ManualSiweMessageParser();
        var payload = CanonicalPayload().Replace("Chain ID: 11155111\n", string.Empty, StringComparison.Ordinal);

        var ex = await Assert.ThrowsAsync<AuthDomainException>(() => parser.ParseAsync(payload, CancellationToken.None));

        Assert.Equal("siwe_parse_failed", ex.Error.Code);
    }

    [Fact]
    public async Task Parse_MalformedAddress_ThrowsSiweParseFailed()
    {
        var parser = new ManualSiweMessageParser();
        var payload = CanonicalPayload().Replace("0xC02aaa39b223FE8D0A0e5C4F27eAD9083C756Cc2", "0x1234", StringComparison.Ordinal);

        var ex = await Assert.ThrowsAsync<AuthDomainException>(() => parser.ParseAsync(payload, CancellationToken.None));
        Assert.Equal("siwe_parse_failed", ex.Error.Code);
    }

    [Fact]
    public async Task Parse_VersionNotOne_ThrowsSiweParseFailed()
    {
        var parser = new ManualSiweMessageParser();
        var payload = CanonicalPayload().Replace("Version: 1", "Version: 2", StringComparison.Ordinal);

        var ex = await Assert.ThrowsAsync<AuthDomainException>(() => parser.ParseAsync(payload, CancellationToken.None));
        Assert.Equal("siwe_parse_failed", ex.Error.Code);
    }

    [Fact]
    public async Task Parse_ResourcesWith0Or1Or2Entries_ParsesCorrectly()
    {
        var parser = new ManualSiweMessageParser();
        var payload0 = CanonicalPayload(includeResources: false);
        var payload1 = CanonicalPayload(resourcesCount: 1);
        var payload2 = CanonicalPayload(resourcesCount: 2);

        var m0 = await parser.ParseAsync(payload0, CancellationToken.None);
        var m1 = await parser.ParseAsync(payload1, CancellationToken.None);
        var m2 = await parser.ParseAsync(payload2, CancellationToken.None);

        Assert.Empty(m0.Resources);
        Assert.Single(m1.Resources);
        Assert.Equal(2, m2.Resources.Count);
    }

    [Fact]
    public async Task Parse_UnknownChainId_ReturnsMessageWithoutPolicyRejection()
    {
        var parser = new ManualSiweMessageParser();
        var payload = CanonicalPayload().Replace("Chain ID: 11155111", "Chain ID: 42161", StringComparison.Ordinal);

        var message = await parser.ParseAsync(payload, CancellationToken.None);

        Assert.Equal(42161, message.ChainId);
    }

    private static string CanonicalPayload(bool includeResources = true, int resourcesCount = 2)
    {
        var resources = includeResources
            ? resourcesCount switch
            {
                0 => string.Empty,
                1 => "\nResources:\n- https://example.com",
                _ => "\nResources:\n- https://example.com\n- ipfs://bafybeigdyrzt4"
            }
            : string.Empty;

        return "service.org wants you to sign in with your Ethereum account:\n" +
               "0xC02aaa39b223FE8D0A0e5C4F27eAD9083C756Cc2\n" +
               "\n" +
               "I accept the ServiceOrg Terms of Service: https://service.org/tos\n" +
               "\n" +
               "URI: https://service.org/login\n" +
               "Version: 1\n" +
               "Chain ID: 11155111\n" +
               "Nonce: 32891756b8f249f39ac0f53f6f38f9dd\n" +
               "Issued At: 2021-09-30T16:25:24Z" +
               resources;
    }
}
