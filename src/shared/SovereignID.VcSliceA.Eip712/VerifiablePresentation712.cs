using Nethereum.ABI.FunctionEncoding.Attributes;

namespace SovereignID.VcSliceA.Eip712;

/// <summary>
/// Mensaje EIP-712 primario para la VP mínima (titular), distinto del tipo del VC.
/// </summary>
[Struct("VerifiablePresentation712")]
public class VerifiablePresentation712
{
    [Parameter("string", "holder", 1)]
    public string Holder { get; set; } = "";

    [Parameter("string", "vcCredentialId", 2)]
    public string VcCredentialId { get; set; } = "";

    [Parameter("string", "vcIssuer", 3)]
    public string VcIssuer { get; set; } = "";
}
