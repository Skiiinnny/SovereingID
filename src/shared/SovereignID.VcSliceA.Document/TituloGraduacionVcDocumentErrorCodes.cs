namespace SovereignID.VcSliceA.Document;

/// <summary>
/// Códigos de error canónicos (sin prefijo de bounded context) devueltos por
/// <see cref="TituloGraduacionVcDocumentValidator"/>.
/// </summary>
public static class TituloGraduacionVcDocumentErrorCodes
{
    public const string DocumentRootNotObject = "document_root_not_object";

    public const string DocumentContextMissing = "document_context_missing";
    public const string DocumentContextNotArray = "document_context_not_array";
    public const string DocumentContextLengthInvalid = "document_context_length_invalid";
    public const string DocumentContextFirstMismatch = "document_context_first_mismatch";
    public const string DocumentContextSecondNotObject = "document_context_second_not_object";
    public const string DocumentContextSecondMismatch = "document_context_second_mismatch";

    public const string DocumentTypeMissing = "document_type_missing";
    public const string DocumentTypeInvalid = "document_type_invalid";

    public const string DocumentIdMissing = "document_id_missing";
    public const string DocumentIdFormat = "document_id_format";

    public const string DocumentIssuerMissing = "document_issuer_missing";
    public const string DocumentIssuerDidInvalid = "document_issuer_did_invalid";

    public const string DocumentIssuanceDateMissing = "document_issuanceDate_missing";
    public const string DocumentIssuanceDateFormat = "document_issuanceDate_format";
    public const string DocumentIssuanceDateFuture = "document_issuanceDate_future";

    public const string DocumentExpirationDateFormat = "document_expirationDate_format";
    public const string DocumentExpirationDateExpired = "document_expirationDate_expired";

    public const string DocumentSubjectMissing = "document_subject_missing";
    public const string DocumentSubjectNotObject = "document_subject_not_object";
    public const string DocumentSubjectExtraProperty = "document_subject_extra_property";
    public const string DocumentSubjectIdMissing = "document_subject_id_missing";
    public const string DocumentSubjectIdDidInvalid = "document_subject_id_did_invalid";

    public const string DocumentSubjectDegreeTitleMissing = "document_subject_degreeTitle_missing";
    public const string DocumentSubjectDegreeTitleEmpty = "document_subject_degreeTitle_empty";
    public const string DocumentSubjectProgramNameMissing = "document_subject_programName_missing";
    public const string DocumentSubjectProgramNameEmpty = "document_subject_programName_empty";
    public const string DocumentSubjectAwardDateMissing = "document_subject_awardDate_missing";
    public const string DocumentSubjectAwardDateEmpty = "document_subject_awardDate_empty";
    public const string DocumentSubjectAwardDateFormat = "document_subject_awardDate_format";
    public const string DocumentSubjectAwardDateInvalid = "document_subject_awardDate_invalid";

    public const string DocumentProofUnexpected = "document_proof_unexpected";
    public const string DocumentProofMissing = "document_proof_missing";
    public const string DocumentProofTypeInvalid = "document_proof_type_invalid";
    public const string DocumentProofValueMissing = "document_proof_value_missing";
    public const string DocumentProofValueEmpty = "document_proof_value_empty";
}
