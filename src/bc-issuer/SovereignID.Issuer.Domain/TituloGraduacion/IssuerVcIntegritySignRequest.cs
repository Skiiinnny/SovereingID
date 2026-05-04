namespace SovereignID.Issuer.Domain.TituloGraduacion;

/// <summary>
/// Carga firmable acordada para el VC TituloGraduacion (alineada con EIP-712 en Application/Infra).
/// </summary>
public sealed record IssuerVcIntegritySignRequest(
    string CredentialId,
    string IssuerDid,
    string CredentialSubjectId,
    string DegreeTitle,
    string ProgramName,
    string AwardDate,
    string IssuanceDate,
    string ExpirationDateOrEmpty);
