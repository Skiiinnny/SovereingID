using SovereignID.Issuer.Domain.TituloGraduacion;
using SovereignID.VcSliceA.Eip712;

namespace SovereignID.Issuer.Application.TituloGraduacion;

internal static class IssuerVcIntegritySignRequestMapper
{
    public static TituloGraduacionCredential712 ToEip712(IssuerVcIntegritySignRequest r) =>
        new()
        {
            CredentialId = r.CredentialId,
            Issuer = r.IssuerDid,
            CredentialSubjectId = r.CredentialSubjectId,
            DegreeTitle = r.DegreeTitle,
            ProgramName = r.ProgramName,
            AwardDate = r.AwardDate,
            IssuanceDate = r.IssuanceDate,
            ExpirationDate = r.ExpirationDateOrEmpty,
        };
}
