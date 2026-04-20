using System.Globalization;
using Microsoft.Extensions.Configuration;
using SovereignID.Chain;
using SovereignID.Crypto;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .Build();

Console.WriteLine("=== SovereignID Phase 1 Demo ===");
Console.WriteLine();

var keyPair = KeyPair.Generate();
Console.WriteLine("[KeyPair]");
Console.WriteLine($"Address : {keyPair.Address}");
Console.WriteLine($"Public  : {TruncateHex(keyPair.PublicKey, 6, 6)}");
Console.WriteLine("Private : *** (hidden in output)");
Console.WriteLine();

const string message = "Hello SovereignID";
var signer = new MessageSigner();
var signature = signer.Sign(message, keyPair.PrivateKey);
var verifier = new SignatureVerifier();
var recovered = verifier.RecoverSigner(message, signature);
var match = verifier.Verify(message, signature, keyPair.Address);

Console.WriteLine("[Sign & Verify]");
Console.WriteLine($"Message   : \"{message}\"");
Console.WriteLine($"Signature : {TruncateHex(signature, 6, 6)}");
Console.WriteLine($"Recovered : {recovered}");
Console.WriteLine($"Match     : {(match ? "✓ TRUE" : "FALSE")}");
Console.WriteLine();

var rpcUrl = Environment.GetEnvironmentVariable("SEPOLIA_RPC_URL")
             ?? configuration["SovereignID:SepoliaRpcUrl"];
var contractAddress = Environment.GetEnvironmentVariable("NOTARY_CONTRACT_ADDRESS")
                      ?? configuration["SovereignID:NotaryContractAddress"];
var notarizeKey = Environment.GetEnvironmentVariable("NOTARIZE_DEMO_PRIVATE_KEY");

if (string.IsNullOrWhiteSpace(rpcUrl))
{
    Console.WriteLine("[Chain — Sepolia]");
    Console.WriteLine("Skipped: set SEPOLIA_RPC_URL (or SovereignID:SepoliaRpcUrl in appsettings.Development.json).");
    Console.WriteLine();
}
else
{
    var chain = new SepoliaClient(rpcUrl);
    var block = await chain.GetBlockNumberAsync().ConfigureAwait(false);
    var balance = await chain.GetBalanceEtherAsync(keyPair.Address).ConfigureAwait(false);

    Console.WriteLine("[Chain — Sepolia]");
    Console.WriteLine($"Block     : {block.ToString("N0", CultureInfo.InvariantCulture)}");
    Console.WriteLine($"Balance   : {balance.ToString("0.##########", CultureInfo.InvariantCulture)} ETH");
    Console.WriteLine();
}

if (string.IsNullOrWhiteSpace(rpcUrl)
    || string.IsNullOrWhiteSpace(contractAddress)
    || string.IsNullOrWhiteSpace(notarizeKey))
{
    Console.WriteLine("[Notarize]");
    Console.WriteLine("Skipped: set NOTARY_CONTRACT_ADDRESS and NOTARIZE_DEMO_PRIVATE_KEY (env only).");
    Console.WriteLine("Deploy contracts/Notary.sol to Sepolia, then export the contract address.");
    Console.WriteLine();
}
else
{
    const string doc = "My important document v1.0";
    var sha = HashHelper.Sha256(doc);
    var notary = new DocumentNotarizer(rpcUrl!, contractAddress!);
    var txHash = await notary.NotarizeAsync(doc, notarizeKey!).ConfigureAwait(false);
    var ok = await notary.VerifyAsync(doc, txHash).ConfigureAwait(false);

    Console.WriteLine("[Notarize]");
    Console.WriteLine($"Content   : \"{doc}\"");
    Console.WriteLine($"SHA256    : {TruncateHex(sha, 6, 6)}");
    Console.WriteLine($"TxHash    : {TruncateHex(txHash, 6, 6)}");
    Console.WriteLine($"Verified  : {(ok ? "✓ TRUE" : "FALSE")}");
    Console.WriteLine();
}

static string TruncateHex(string hex, int headChars, int tailChars)
{
    ArgumentException.ThrowIfNullOrEmpty(hex);
    return hex.Length <= headChars + tailChars + 3
        ? hex
        : string.Concat(hex.AsSpan(0, headChars), "...", hex.AsSpan(hex.Length - tailChars, tailChars));
}
