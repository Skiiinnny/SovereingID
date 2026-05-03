using Nethereum.Signer;
using SovereignID.Auth.Infrastructure.Siwe;
using SovereignID.SharedKernel.Domain;

namespace SovereignID.Auth.Infrastructure.Tests;

public class NethereumSiweSignatureVerifierTests
{
    [Fact]
    public async Task RecoverAddress_KnownPrivateKeyAndMessage_Matches()
    {
        const string privateKey = "0x4f3edf983ac636a65a842ce7c78d9aa706d3b113bce036f4f11f2bb3f3a58d16";
        const string message = "hello siwe";
        var key = new EthECKey(privateKey);
        var signer = new EthereumMessageSigner();
        var signature = signer.EncodeUTF8AndSign(message, key);
        var verifier = new NethereumSiweSignatureVerifier();

        var recovered = await verifier.RecoverAddressAsync(message, Signature.Create(signature), CancellationToken.None);

        Assert.Equal(key.GetPublicAddress(), recovered.Value, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RecoverAddress_TamperedMessage_Differs()
    {
        const string privateKey = "0x4f3edf983ac636a65a842ce7c78d9aa706d3b113bce036f4f11f2bb3f3a58d16";
        const string original = "hello siwe";
        const string tampered = "hello siwe tampered";
        var key = new EthECKey(privateKey);
        var signer = new EthereumMessageSigner();
        var signature = signer.EncodeUTF8AndSign(original, key);
        var verifier = new NethereumSiweSignatureVerifier();

        var recovered = await verifier.RecoverAddressAsync(tampered, Signature.Create(signature), CancellationToken.None);

        Assert.NotEqual(key.GetPublicAddress(), recovered.Value, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RecoverAddress_MalformedSignature_ThrowsConsistentException()
    {
        var verifier = new NethereumSiweSignatureVerifier();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            verifier.RecoverAddressAsync("hello", Signature.Create("0x1234"), CancellationToken.None));

        Assert.Contains("Unable to recover Ethereum address", ex.Message, StringComparison.Ordinal);
    }
}
