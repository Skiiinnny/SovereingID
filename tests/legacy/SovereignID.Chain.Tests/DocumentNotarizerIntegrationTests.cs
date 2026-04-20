using SovereignID.Chain;
using Xunit;

namespace SovereignID.Chain.Tests;

public class DocumentNotarizerIntegrationTests
{
    /// <summary>Covers notarization + positive/negative verification (tasks 35–37).</summary>
    [SkippableFact]
    [Trait("Category", "Integration")]
    public async Task Notarize_V1_ThenVerify_V1True_V2False()
    {
        var rpc = Environment.GetEnvironmentVariable("SEPOLIA_RPC_URL");
        var contract = Environment.GetEnvironmentVariable("NOTARY_CONTRACT_ADDRESS");
        var pk = Environment.GetEnvironmentVariable("NOTARIZE_TEST_PRIVATE_KEY");
        Skip.If(
            string.IsNullOrWhiteSpace(rpc)
            || string.IsNullOrWhiteSpace(contract)
            || string.IsNullOrWhiteSpace(pk),
            "Set SEPOLIA_RPC_URL, NOTARY_CONTRACT_ADDRESS, and NOTARIZE_TEST_PRIVATE_KEY (never commit real keys).");

        var notary = new DocumentNotarizer(rpc!, contract!);
        var txHash = await notary.NotarizeAsync("Hello World v1", pk!);
        Assert.False(string.IsNullOrWhiteSpace(txHash));
        Assert.StartsWith("0x", txHash, StringComparison.Ordinal);

        Assert.True(await notary.VerifyAsync("Hello World v1", txHash));
        Assert.False(await notary.VerifyAsync("Hello World v2", txHash));
    }
}
