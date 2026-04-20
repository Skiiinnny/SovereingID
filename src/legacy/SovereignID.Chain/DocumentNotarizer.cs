using System.Numerics;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using SovereignID.Crypto;

namespace SovereignID.Chain;

/// <summary>On-chain document hash registry backed by a minimal <c>Notary</c> contract.</summary>
public sealed class DocumentNotarizer
{
    private const string AbiJson =
        """
        [
          {"inputs":[{"internalType":"bytes32","name":"hash","type":"bytes32"}],"name":"notarize","outputs":[],"stateMutability":"nonpayable","type":"function"},
          {"inputs":[{"internalType":"bytes32","name":"","type":"bytes32"}],"name":"timestamps","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"}
        ]
        """;

    private readonly string _rpcUrl;
    private readonly string _contractAddress;

    /// <summary>Creates a notarizer targeting <paramref name="contractAddress"/> on the network reachable via <paramref name="rpcUrl"/>.</summary>
    public DocumentNotarizer(string rpcUrl, string contractAddress)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rpcUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(contractAddress);
        _rpcUrl = rpcUrl;
        _contractAddress = contractAddress;
    }

    /// <summary>Stores <c>sha256(UTF8(content))</c> on-chain and returns the transaction hash.</summary>
    public async Task<string> NotarizeAsync(string content, string privateKey, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(privateKey);

        var account = new Account(privateKey);
        var web3 = new Web3(account, _rpcUrl);
        var contract = web3.Eth.GetContract(AbiJson, _contractAddress);
        var function = contract.GetFunction("notarize");
        var hashHex = HashHelper.Sha256(content);
        var receipt = await function.SendTransactionAndWaitForReceiptAsync(hashHex).ConfigureAwait(false);
        return receipt.TransactionHash;
    }

    /// <summary>
    /// Returns true when the transaction calls <c>notarize</c> with the SHA-256 of <paramref name="content"/>
    /// and the contract records a non-zero timestamp for that hash.
    /// </summary>
    public async Task<bool> VerifyAsync(string content, string txHash, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(txHash);

        var expectedHex = HashHelper.Sha256(content);
        var web3 = new Web3(_rpcUrl);
        var tx = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(txHash).ConfigureAwait(false);
        if (tx?.Input is null)
        {
            return false;
        }

        var input = tx.Input;
        if (input.Length < 10 + 64)
        {
            return false;
        }

        var argHex = "0x" + input.Substring(10, 64);
        if (!string.Equals(argHex, expectedHex, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var contract = web3.Eth.GetContract(AbiJson, _contractAddress);
        var timestamps = contract.GetFunction("timestamps");
        var stamp = await timestamps.CallAsync<BigInteger>(HashHelper.Sha256Bytes(content)).ConfigureAwait(false);
        return stamp > BigInteger.Zero;
    }
}
