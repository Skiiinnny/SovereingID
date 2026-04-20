using Nethereum.Web3;

namespace SovereignID.Chain;

/// <summary>Thin wrapper around <see cref="Web3"/> configured for an Ethereum JSON-RPC endpoint (Sepolia).</summary>
public class SepoliaClient
{
    private readonly Web3 _web3;

    /// <summary>Creates a client using <paramref name="rpcUrl"/> (for example from <c>SEPOLIA_RPC_URL</c>).</summary>
    public SepoliaClient(string rpcUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rpcUrl);
        _web3 = new Web3(rpcUrl);
    }

    /// <summary>ETH balance for <paramref name="address"/>.</summary>
    public async Task<decimal> GetBalanceEtherAsync(string address, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(address);
        var wei = await _web3.Eth.GetBalance.SendRequestAsync(address).ConfigureAwait(false);
        return Web3.Convert.FromWei(wei);
    }

    /// <summary>Latest canonical head block number.</summary>
    public async Task<ulong> GetBlockNumberAsync(CancellationToken cancellationToken = default)
    {
        var number = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync().ConfigureAwait(false);
        return (ulong)number.Value;
    }
}
