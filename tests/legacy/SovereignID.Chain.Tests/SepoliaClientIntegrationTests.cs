namespace SovereignID.Chain.Tests;

public class SepoliaClientIntegrationTests
{
    [SkippableFact]
    [Trait("Category", "Integration")]
    public async Task GetBlockNumberAsync_ReturnsPositive()
    {
        var rpc = Environment.GetEnvironmentVariable("SEPOLIA_RPC_URL");
        Skip.If(string.IsNullOrWhiteSpace(rpc), "Set SEPOLIA_RPC_URL to run Sepolia integration tests.");
        var client = new SepoliaClient(rpc!);
        var block = await client.GetBlockNumberAsync();
        Assert.True(block > 0UL);
    }

    [SkippableFact]
    [Trait("Category", "Integration")]
    public async Task GetBalanceEtherAsync_ForWellKnownAddress_IsNonNegative()
    {
        var rpc = Environment.GetEnvironmentVariable("SEPOLIA_RPC_URL");
        Skip.If(string.IsNullOrWhiteSpace(rpc), "Set SEPOLIA_RPC_URL to run Sepolia integration tests.");
        var address = Environment.GetEnvironmentVariable("SEPOLIA_TEST_ADDRESS")
                      ?? "0x0000000000000000000000000000000000000000";
        var client = new SepoliaClient(rpc!);
        var balance = await client.GetBalanceEtherAsync(address);
        Assert.True(balance >= 0m);
    }
}
