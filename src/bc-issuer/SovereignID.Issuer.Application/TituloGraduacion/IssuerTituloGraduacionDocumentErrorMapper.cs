using SovereignID.VerifiableCredential.Document;

namespace SovereignID.Issuer.Application.TituloGraduacion;

/// <summary>
/// Traduce códigos canónicos de <see cref="TituloGraduacionVcDocumentValidator"/> a códigos
/// de error del flujo de emisión.
/// </summary>
internal static class IssuerTituloGraduacionDocumentErrorMapper
{
    /// <summary>Devuelve el código de error del emisor.</summary>
    public static string Map(string documentCode) =>
        documentCode switch
        {
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectDegreeTitleMissing
            or TituloGraduacionVcDocumentErrorCodes.DocumentSubjectDegreeTitleEmpty => "degreeTitle_required",

            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectProgramNameMissing
            or TituloGraduacionVcDocumentErrorCodes.DocumentSubjectProgramNameEmpty => "programName_required",

            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectAwardDateMissing
            or TituloGraduacionVcDocumentErrorCodes.DocumentSubjectAwardDateEmpty => "awardDate_required",

            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectAwardDateFormat => "awardDate_format",

            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectAwardDateInvalid => "awardDate_invalid_calendar",

            TituloGraduacionVcDocumentErrorCodes.DocumentIssuerDidInvalid => "issuer_did_invalid",

            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectIdDidInvalid => "subject_did_invalid",

            _ => "credential_document_invalid",
        };
}
