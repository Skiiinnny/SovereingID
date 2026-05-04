using System.Text.Json;
using System.Text.Json.Nodes;

namespace SovereignID.VcSliceA.Document.Tests;

public class TituloGraduacionVcDocumentValidatorTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse(
        "2026-05-03T12:00:00Z",
        System.Globalization.CultureInfo.InvariantCulture);

    private static JsonObject BuildValidUnsignedRoot()
    {
        var subject = new JsonObject
        {
            ["id"] = "did:ethr:sepolia:0x2222222222222222222222222222222222222222",
            ["degreeTitle"] = "Grado",
            ["programName"] = "Programa",
            ["awardDate"] = "2025-06-01",
        };

        var second = TituloGraduacionVcDocumentConstants.CreateSecondContextObject();
        var context = new JsonArray(TituloGraduacionVcDocumentConstants.W3CVerifiableCredentialsContextV1, second);
        return new JsonObject
        {
            ["@context"] = context,
            ["id"] = "urn:uuid:aaaaaaaa-bbbb-4ccc-dddd-eeeeeeeeeeee",
            ["type"] = new JsonArray("VerifiableCredential", "TituloGraduacionCredential"),
            ["issuer"] = "did:ethr:sepolia:0x1111111111111111111111111111111111111111",
            ["issuanceDate"] = "2026-05-03T10:00:00Z",
            ["credentialSubject"] = subject,
        };
    }

    private static JsonObject BuildValidSignedRoot()
    {
        var root = BuildValidUnsignedRoot();
        root["proof"] = new JsonObject
        {
            ["type"] = TituloGraduacionVcDocumentConstants.ProofType,
            ["proofValue"] = "0x" + new string('c', 130),
        };
        return root;
    }

    private static string? ValidateUnsigned(JsonObject root) =>
        Validate(root, CredentialValidationMode.Unsigned);

    private static string? ValidateSigned(JsonObject root) =>
        Validate(root, CredentialValidationMode.Signed);

    private static string? Validate(JsonObject root, CredentialValidationMode mode)
    {
        using var doc = JsonDocument.Parse(root.ToJsonString());
        return TituloGraduacionVcDocumentValidator.Validate(doc.RootElement, Now, mode);
    }

    [Fact]
    public void Validate_root_not_object_returns_code()
    {
        using var doc = JsonDocument.Parse("[]");
        var err = TituloGraduacionVcDocumentValidator.Validate(doc.RootElement, Now, CredentialValidationMode.Unsigned);
        Assert.Equal(TituloGraduacionVcDocumentErrorCodes.DocumentRootNotObject, err);
    }

    [Theory]
    [MemberData(nameof(ContextNegativeCases))]
    public void Validate_unsigned_context_branch(string expectedCode, Action<JsonObject> mutate)
    {
        var root = BuildValidUnsignedRoot();
        mutate(root);
        Assert.Equal(expectedCode, ValidateUnsigned(root));
    }

    public static IEnumerable<object[]> ContextNegativeCases()
    {
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentContextMissing,
            (Action<JsonObject>)(r => r.Remove("@context"))
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentContextNotArray,
            (Action<JsonObject>)(r => r["@context"] = "https://www.w3.org/2018/credentials/v1")
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentContextLengthInvalid,
            (Action<JsonObject>)(r => r["@context"] = new JsonArray(TituloGraduacionVcDocumentConstants.W3CVerifiableCredentialsContextV1))
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentContextLengthInvalid,
            (Action<JsonObject>)(r => r["@context"] = new JsonArray(
                TituloGraduacionVcDocumentConstants.W3CVerifiableCredentialsContextV1,
                TituloGraduacionVcDocumentConstants.CreateSecondContextObject(),
                "https://example.com/extra"))
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentContextFirstMismatch,
            (Action<JsonObject>)(r => r["@context"] = new JsonArray(
                "https://example.com/wrong-v1",
                TituloGraduacionVcDocumentConstants.CreateSecondContextObject()))
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentContextSecondNotObject,
            (Action<JsonObject>)(r => r["@context"] = new JsonArray(
                TituloGraduacionVcDocumentConstants.W3CVerifiableCredentialsContextV1,
                "inline-should-be-object"))
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentContextSecondMismatch,
            (Action<JsonObject>)(r =>
            {
                var oneKey = new JsonObject { ["degreeTitle"] = TituloGraduacionVcDocumentConstants.VocabBase + "degreeTitle" };
                r["@context"] = new JsonArray(TituloGraduacionVcDocumentConstants.W3CVerifiableCredentialsContextV1, oneKey);
            })
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentContextSecondMismatch,
            (Action<JsonObject>)(r =>
            {
                var bad = TituloGraduacionVcDocumentConstants.CreateSecondContextObject();
                bad["degreeTitle"] = 1;
                r["@context"] = new JsonArray(TituloGraduacionVcDocumentConstants.W3CVerifiableCredentialsContextV1, bad);
            })
        ];
    }

    [Theory]
    [MemberData(nameof(TypeIdIssuerIssuanceNegativeCases))]
    public void Validate_unsigned_type_id_issuer_issuance_branch(string expectedCode, Action<JsonObject> mutate)
    {
        var root = BuildValidUnsignedRoot();
        mutate(root);
        Assert.Equal(expectedCode, ValidateUnsigned(root));
    }

    public static IEnumerable<object[]> TypeIdIssuerIssuanceNegativeCases()
    {
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentTypeMissing,
            (Action<JsonObject>)(r => r.Remove("type"))
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentTypeMissing,
            (Action<JsonObject>)(r => r["type"] = "VerifiableCredential")
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentTypeInvalid,
            (Action<JsonObject>)(r => r["type"] = new JsonArray("VerifiableCredential", 1))
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentTypeInvalid,
            (Action<JsonObject>)(r => r["type"] = new JsonArray("VerifiableCredential", "OtherCredential"))
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentIdMissing,
            (Action<JsonObject>)(r => r.Remove("id"))
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentIdMissing,
            (Action<JsonObject>)(r => r["id"] = 123)
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentIdFormat,
            (Action<JsonObject>)(r => r["id"] = "urn:uuid:AAAAAAAA-BBBB-4CCC-DDDD-EEEEEEEEEEEE")
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentIssuerMissing,
            (Action<JsonObject>)(r => r.Remove("issuer"))
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentIssuerDidInvalid,
            (Action<JsonObject>)(r => r["issuer"] = "did:ethr:sepolia:0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF")
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentIssuanceDateMissing,
            (Action<JsonObject>)(r => r.Remove("issuanceDate"))
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentIssuanceDateFormat,
            (Action<JsonObject>)(r => r["issuanceDate"] = "2026-05-03T10:00:00+00:00")
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentIssuanceDateFormat,
            (Action<JsonObject>)(r => r["issuanceDate"] = "not-a-dateZ")
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentIssuanceDateFuture,
            (Action<JsonObject>)(r => r["issuanceDate"] = "2026-05-04T00:00:00Z")
        ];
    }

    [Fact]
    public void Validate_unsigned_expiration_date_bad_format_when_present()
    {
        var root = BuildValidUnsignedRoot();
        root["expirationDate"] = "2026-13-40T00:00:00Z";
        Assert.Equal(TituloGraduacionVcDocumentErrorCodes.DocumentExpirationDateFormat, ValidateUnsigned(root));
    }

    [Theory]
    [MemberData(nameof(SubjectNegativeCases))]
    public void Validate_unsigned_subject_branch(string expectedCode, Action<JsonObject> mutate)
    {
        var root = BuildValidUnsignedRoot();
        mutate(root);
        Assert.Equal(expectedCode, ValidateUnsigned(root));
    }

    public static IEnumerable<object[]> SubjectNegativeCases()
    {
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectMissing,
            (Action<JsonObject>)(r => r.Remove("credentialSubject"))
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectNotObject,
            (Action<JsonObject>)(r => r["credentialSubject"] = new JsonArray())
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectExtraProperty,
            (Action<JsonObject>)(r => r["credentialSubject"]!["legalName"] = "X")
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectIdMissing,
            (Action<JsonObject>)(r => r["credentialSubject"]!.AsObject().Remove("id", out _))
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectDegreeTitleMissing,
            (Action<JsonObject>)(r => r["credentialSubject"]!.AsObject().Remove("degreeTitle", out _))
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectDegreeTitleEmpty,
            (Action<JsonObject>)(r => r["credentialSubject"]!["degreeTitle"] = "  ")
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectProgramNameMissing,
            (Action<JsonObject>)(r => r["credentialSubject"]!.AsObject().Remove("programName", out _))
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectProgramNameEmpty,
            (Action<JsonObject>)(r => r["credentialSubject"]!["programName"] = "")
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectAwardDateMissing,
            (Action<JsonObject>)(r => r["credentialSubject"]!.AsObject().Remove("awardDate", out _))
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectAwardDateEmpty,
            (Action<JsonObject>)(r => r["credentialSubject"]!["awardDate"] = "\t")
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectAwardDateInvalid,
            (Action<JsonObject>)(r => r["credentialSubject"]!["awardDate"] = "2025-02-30")
        ];
    }

    [Theory]
    [MemberData(nameof(ProofSignedNegativeCases))]
    public void Validate_signed_proof_branch(string expectedCode, Action<JsonObject> mutate)
    {
        var root = BuildValidSignedRoot();
        mutate(root);
        Assert.Equal(expectedCode, ValidateSigned(root));
    }

    public static IEnumerable<object[]> ProofSignedNegativeCases()
    {
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentProofMissing,
            (Action<JsonObject>)(r => r.Remove("proof"))
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentProofMissing,
            (Action<JsonObject>)(r => r["proof"] = "not-object")
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentProofTypeInvalid,
            (Action<JsonObject>)(r => r["proof"]!["type"] = "WrongProofType")
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentProofValueMissing,
            (Action<JsonObject>)(r => r["proof"]!.AsObject().Remove("proofValue", out _))
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentProofValueEmpty,
            (Action<JsonObject>)(r => r["proof"]!["proofValue"] = "")
        ];
        yield return
        [
            TituloGraduacionVcDocumentErrorCodes.DocumentProofValueEmpty,
            (Action<JsonObject>)(r => r["proof"]!["proofValue"] = "   ")
        ];
    }

    [Fact]
    public void Validate_unsigned_golden_ok()
    {
        Assert.Null(ValidateUnsigned(BuildValidUnsignedRoot()));
    }

    [Fact]
    public void Validate_unsigned_rejects_proof_present()
    {
        var root = BuildValidUnsignedRoot();
        root["proof"] = new JsonObject
        {
            ["type"] = TituloGraduacionVcDocumentConstants.ProofType,
            ["proofValue"] = "0xab",
        };
        Assert.Equal(TituloGraduacionVcDocumentErrorCodes.DocumentProofUnexpected, ValidateUnsigned(root));
    }

    [Fact]
    public void Validate_signed_requires_proof()
    {
        Assert.Equal(TituloGraduacionVcDocumentErrorCodes.DocumentProofMissing, ValidateSigned(BuildValidUnsignedRoot()));
    }

    [Fact]
    public void Validate_signed_ok_with_proof()
    {
        Assert.Null(ValidateSigned(BuildValidSignedRoot()));
    }

    [Fact]
    public void Validate_rejects_wrong_second_context_iri()
    {
        var root = BuildValidUnsignedRoot();
        var badSecond = new JsonObject
        {
            ["TituloGraduacionCredential"] = TituloGraduacionVcDocumentConstants.VocabBase + "TituloGraduacionCredential",
            ["degreeTitle"] = "https://wrong.example/vocab#degreeTitle",
            ["programName"] = TituloGraduacionVcDocumentConstants.VocabBase + "programName",
            ["awardDate"] = TituloGraduacionVcDocumentConstants.VocabBase + "awardDate",
        };
        root["@context"] = new JsonArray(TituloGraduacionVcDocumentConstants.W3CVerifiableCredentialsContextV1, badSecond);
        Assert.Equal(TituloGraduacionVcDocumentErrorCodes.DocumentContextSecondMismatch, ValidateUnsigned(root));
    }

    [Fact]
    public void Validate_rejects_extra_term_in_second_context()
    {
        var root = BuildValidUnsignedRoot();
        var second = TituloGraduacionVcDocumentConstants.CreateSecondContextObject();
        second["extraTerm"] = "https://sovereignid.local/vocab/titulo-graduacion/v1#extra";
        root["@context"] = new JsonArray(TituloGraduacionVcDocumentConstants.W3CVerifiableCredentialsContextV1, second);
        Assert.Equal(TituloGraduacionVcDocumentErrorCodes.DocumentContextSecondMismatch, ValidateUnsigned(root));
    }

    [Theory]
    [InlineData("2025-13-01", TituloGraduacionVcDocumentErrorCodes.DocumentSubjectAwardDateInvalid)]
    [InlineData("25-01-01", TituloGraduacionVcDocumentErrorCodes.DocumentSubjectAwardDateFormat)]
    [InlineData("2025-1-1", TituloGraduacionVcDocumentErrorCodes.DocumentSubjectAwardDateFormat)]
    public void Validate_rejects_award_date_with_exact_code(string awardDate, string expectedCode)
    {
        var root = BuildValidUnsignedRoot();
        root["credentialSubject"]!["awardDate"] = awardDate;
        Assert.Equal(expectedCode, ValidateUnsigned(root));
    }

    [Fact]
    public void Validate_accepts_leap_year_award_date()
    {
        var root = BuildValidUnsignedRoot();
        root["credentialSubject"]!["awardDate"] = "2024-02-29";
        Assert.Null(ValidateUnsigned(root));
    }

    [Fact]
    public void Validate_rejects_invalid_subject_did()
    {
        var root = BuildValidUnsignedRoot();
        root["credentialSubject"]!["id"] = "did:ethr:sepolia:0xGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG";
        Assert.Equal(TituloGraduacionVcDocumentErrorCodes.DocumentSubjectIdDidInvalid, ValidateUnsigned(root));
    }

    [Fact]
    public void Validate_rejects_expired_when_expiration_before_now()
    {
        var root = BuildValidUnsignedRoot();
        root["expirationDate"] = "2026-05-02T00:00:00Z";
        Assert.Equal(TituloGraduacionVcDocumentErrorCodes.DocumentExpirationDateExpired, ValidateUnsigned(root));
    }
}
