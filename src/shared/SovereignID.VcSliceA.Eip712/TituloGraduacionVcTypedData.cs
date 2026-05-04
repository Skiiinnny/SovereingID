using System.Numerics;
using Nethereum.ABI.EIP712;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;

namespace SovereignID.VcSliceA.Eip712;

/// <summary>
/// Construye <see cref="TypedData"/> y operaciones de firma / recuperación para el VC TituloGraduacion.
/// </summary>
public static class TituloGraduacionVcTypedData
{
    public static TypedData<DomainWithNameVersionAndChainId> CreateTypedData(TituloGraduacionCredential712 message)
    {
        var domain = new DomainWithNameVersionAndChainId
        {
            Name = VcSliceAEip712Constants.VcDomainName,
            Version = VcSliceAEip712Constants.VcDomainVersion,
            ChainId = new BigInteger(VcSliceAEip712Constants.SepoliaChainId),
        };

        var typedData = new TypedData<DomainWithNameVersionAndChainId>
        {
            Domain = domain,
            Types = MemberDescriptionFactory.GetTypesMemberDescription(
                typeof(TituloGraduacionCredential712),
                typeof(DomainWithNameVersionAndChainId)),
            PrimaryType = nameof(TituloGraduacionCredential712),
        };
        typedData.SetMessage(message);
        return typedData;
    }

    /// <summary>Firma con <c>eth_signTypedData_v4</c> (digest EIP-712 + ECDSA).</summary>
    public static string Sign(TituloGraduacionCredential712 message, EthECKey issuerKey)
    {
        ArgumentNullException.ThrowIfNull(issuerKey);
        var typedData = CreateTypedData(message);
        return new Eip712TypedDataSigner().SignTypedDataV4(message, typedData, issuerKey);
    }

    /// <summary>Recupera la dirección del firmante (0x + 40 hex minúsculas).</summary>
    public static string RecoverSignerAddress(TituloGraduacionCredential712 message, string signatureHex)
    {
        var typedData = CreateTypedData(message);
        var recovered = new Eip712TypedDataSigner().RecoverFromSignatureV4(message, typedData, signatureHex);
        return NormalizeAddress(recovered);
    }

    private static string NormalizeAddress(string address)
    {
        if (string.IsNullOrEmpty(address))
        {
            return address;
        }

        var a = address.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? address[2..]
            : address;
        return "0x" + a.ToLowerInvariant();
    }
}
