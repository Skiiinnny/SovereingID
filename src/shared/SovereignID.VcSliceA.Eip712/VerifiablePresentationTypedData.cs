using System.Numerics;
using Nethereum.ABI.EIP712;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;

namespace SovereignID.VcSliceA.Eip712;

/// <summary>
/// EIP-712 para la VP mínima (titular).
/// </summary>
public static class VerifiablePresentationTypedData
{
    public static TypedData<DomainWithNameVersionAndChainId> CreateTypedData(VerifiablePresentation712 message)
    {
        var domain = new DomainWithNameVersionAndChainId
        {
            Name = VcSliceAEip712Constants.VpDomainName,
            Version = VcSliceAEip712Constants.VpDomainVersion,
            ChainId = new BigInteger(VcSliceAEip712Constants.SepoliaChainId),
        };

        var typedData = new TypedData<DomainWithNameVersionAndChainId>
        {
            Domain = domain,
            Types = MemberDescriptionFactory.GetTypesMemberDescription(
                typeof(VerifiablePresentation712),
                typeof(DomainWithNameVersionAndChainId)),
            PrimaryType = nameof(VerifiablePresentation712),
        };
        typedData.SetMessage(message);
        return typedData;
    }

    public static string Sign(VerifiablePresentation712 message, EthECKey holderKey)
    {
        ArgumentNullException.ThrowIfNull(holderKey);
        var typedData = CreateTypedData(message);
        return new Eip712TypedDataSigner().SignTypedDataV4(message, typedData, holderKey);
    }

    public static string RecoverSignerAddress(VerifiablePresentation712 message, string signatureHex)
    {
        var typedData = CreateTypedData(message);
        var recovered = new Eip712TypedDataSigner().RecoverFromSignatureV4(message, typedData, signatureHex);
        var a = recovered.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? recovered[2..] : recovered;
        return "0x" + a.ToLowerInvariant();
    }
}
