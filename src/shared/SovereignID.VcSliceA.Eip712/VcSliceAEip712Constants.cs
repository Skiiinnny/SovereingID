using SovereignID.VcSliceA.Document;

namespace SovereignID.VcSliceA.Eip712;

/// <summary>
/// Constantes compartidas entre emisor y verificador para pruebas EIP-712 y JSON-LD <c>proof.type</c>.
/// </summary>
public static class VcSliceAEip712Constants
{
    /// <summary>Tipo de prueba declarado en el documento JSON-LD (extensión de producto).</summary>
    public const string ProofType = TituloGraduacionVcDocumentConstants.ProofType;

    public const string VcDomainName = "SovereignID VC";
    public const string VcDomainVersion = "1";

    public const string VpDomainName = "SovereignID VP";
    public const string VpDomainVersion = "1";

    /// <summary>Sepolia testnet (EIP-155).</summary>
    public const int SepoliaChainId = 11155111;
}
