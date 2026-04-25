using Nethereum.Signer;
using Nethereum.Util;
using SovereignID.Auth.Domain.Ports;
using SovereignID.SharedKernel.Domain;

namespace SovereignID.Auth.Infrastructure.Siwe;

public sealed class NethereumSiweSignatureVerifier : ISiweSignatureVerifier
{
    private static readonly EthereumMessageSigner Signer = new();
    private static readonly AddressUtil AddressUtil = new();

    public Task<EthereumAddress> RecoverAddressAsync(string message, Signature signature, CancellationToken cancellationToken)
    {
        try
        {
            var recovered = Signer.EncodeUTF8AndEcRecover(message, signature.Value);
            var checksummed = AddressUtil.ConvertToChecksumAddress(recovered);
            return Task.FromResult(EthereumAddress.Create(checksummed));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Unable to recover Ethereum address from SIWE signature.", ex);
        }
    }
}
