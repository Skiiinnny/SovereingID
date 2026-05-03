using Nethereum.ABI.FunctionEncoding.Attributes;

namespace SovereignID.VcSliceA.Eip712;

/// <summary>
/// Mensaje EIP-712 primario para integridad del VC TituloGraduacion (emisor).
/// </summary>
[Struct("TituloGraduacionCredential712")]
public class TituloGraduacionCredential712
{
    [Parameter("string", "credentialId", 1)]
    public string CredentialId { get; set; } = "";

    [Parameter("string", "issuer", 2)]
    public string Issuer { get; set; } = "";

    [Parameter("string", "credentialSubjectId", 3)]
    public string CredentialSubjectId { get; set; } = "";

    [Parameter("string", "degreeTitle", 4)]
    public string DegreeTitle { get; set; } = "";

    [Parameter("string", "programName", 5)]
    public string ProgramName { get; set; } = "";

    [Parameter("string", "awardDate", 6)]
    public string AwardDate { get; set; } = "";

    [Parameter("string", "issuanceDate", 7)]
    public string IssuanceDate { get; set; } = "";

    /// <summary>Cadena vacía si el VC no tiene <c>expirationDate</c>.</summary>
    [Parameter("string", "expirationDate", 8)]
    public string ExpirationDate { get; set; } = "";
}
