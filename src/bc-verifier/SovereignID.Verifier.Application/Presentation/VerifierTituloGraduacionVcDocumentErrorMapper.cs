using SovereignID.VcSliceA.Document;

namespace SovereignID.Verifier.Application.Presentation;

internal static class VerifierTituloGraduacionVcDocumentErrorMapper
{
    public static string Map(string documentCode) =>
        documentCode switch
        {
            TituloGraduacionVcDocumentErrorCodes.DocumentContextMissing
            or TituloGraduacionVcDocumentErrorCodes.DocumentContextNotArray => "vc_context_missing",

            TituloGraduacionVcDocumentErrorCodes.DocumentContextLengthInvalid
            or TituloGraduacionVcDocumentErrorCodes.DocumentContextFirstMismatch
            or TituloGraduacionVcDocumentErrorCodes.DocumentContextSecondNotObject
            or TituloGraduacionVcDocumentErrorCodes.DocumentContextSecondMismatch => "vc_context_shape",

            TituloGraduacionVcDocumentErrorCodes.DocumentTypeMissing => "vc_type_missing",
            TituloGraduacionVcDocumentErrorCodes.DocumentTypeInvalid => "vc_type_invalid",

            TituloGraduacionVcDocumentErrorCodes.DocumentIdMissing => "vc_id_missing",
            TituloGraduacionVcDocumentErrorCodes.DocumentIdFormat => "vc_id_format",

            TituloGraduacionVcDocumentErrorCodes.DocumentIssuerMissing => "vc_issuer_missing",
            TituloGraduacionVcDocumentErrorCodes.DocumentIssuerDidInvalid => "vc_issuer_did_invalid",

            TituloGraduacionVcDocumentErrorCodes.DocumentIssuanceDateMissing => "vc_issuanceDate_missing",
            TituloGraduacionVcDocumentErrorCodes.DocumentIssuanceDateFormat => "vc_issuanceDate_format",
            TituloGraduacionVcDocumentErrorCodes.DocumentIssuanceDateFuture => "vc_issuanceDate_future",

            TituloGraduacionVcDocumentErrorCodes.DocumentExpirationDateFormat => "vc_expirationDate_format",
            TituloGraduacionVcDocumentErrorCodes.DocumentExpirationDateExpired => "vc_expirationDate_expired",

            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectMissing => "vc_subject_missing",
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectNotObject => "vc_subject_not_object",
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectExtraProperty => "vc_subject_extra_property",
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectIdMissing => "vc_subject_id_missing",
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectIdDidInvalid => "vc_subject_did_invalid",

            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectDegreeTitleMissing => "vc_subject_claim_missing_degreeTitle",
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectDegreeTitleEmpty => "vc_subject_claim_empty_degreeTitle",
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectProgramNameMissing => "vc_subject_claim_missing_programName",
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectProgramNameEmpty => "vc_subject_claim_empty_programName",
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectAwardDateMissing => "vc_subject_claim_missing_awardDate",
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectAwardDateEmpty => "vc_subject_claim_empty_awardDate",
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectAwardDateFormat => "vc_subject_awardDate_format",
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectAwardDateInvalid => "vc_subject_awardDate_invalid",

            TituloGraduacionVcDocumentErrorCodes.DocumentProofMissing => "vc_proof_missing",
            TituloGraduacionVcDocumentErrorCodes.DocumentProofTypeInvalid => "proof_type_invalid",
            TituloGraduacionVcDocumentErrorCodes.DocumentProofValueMissing => "proof_value_missing",
            TituloGraduacionVcDocumentErrorCodes.DocumentProofValueEmpty => "proof_value_empty",

            _ => "vc_document_invalid",
        };
}
