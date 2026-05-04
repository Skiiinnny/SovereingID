using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using SovereignID.SharedKernel.Domain;

namespace SovereignID.VcSliceA.Document;

/// <summary>
/// Valida la forma del documento JSON del VC TituloGraduacion (slice A) sobre <see cref="JsonElement"/>.
/// </summary>
public static partial class TituloGraduacionVcDocumentValidator
{
    private static readonly HashSet<string> AllowedSubjectPropertyNames =
        new(StringComparer.Ordinal) { "id", "degreeTitle", "programName", "awardDate" };

    [GeneratedRegex("^urn:uuid:[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", RegexOptions.CultureInvariant)]
    private static partial Regex UrnUuidLowerRegex();

    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}$", RegexOptions.CultureInvariant)]
    private static partial Regex AwardDatePattern();

    /// <summary>
    /// Valida el nodo raíz del VC. Devuelve <see langword="null"/> si es válido; en caso contrario un código de
    /// <see cref="TituloGraduacionVcDocumentErrorCodes"/>.
    /// </summary>
    /// <param name="vc">Elemento raíz del objeto VC.</param>
    /// <param name="nowUtc">Instante actual UTC para reglas temporales.</param>
    /// <param name="mode"><see cref="CredentialValidationMode.Unsigned"/> no admite <c>proof</c>;
    /// <see cref="CredentialValidationMode.Signed"/> exige <c>proof</c> mínimo.</param>
    public static string? Validate(JsonElement vc, DateTimeOffset nowUtc, CredentialValidationMode mode)
    {
        if (vc.ValueKind != JsonValueKind.Object)
        {
            return TituloGraduacionVcDocumentErrorCodes.DocumentRootNotObject;
        }

        if (mode == CredentialValidationMode.Unsigned && vc.TryGetProperty("proof", out _))
        {
            return TituloGraduacionVcDocumentErrorCodes.DocumentProofUnexpected;
        }

        if (!vc.TryGetProperty("@context", out var ctx))
        {
            return TituloGraduacionVcDocumentErrorCodes.DocumentContextMissing;
        }

        if (ctx.ValueKind != JsonValueKind.Array)
        {
            return TituloGraduacionVcDocumentErrorCodes.DocumentContextNotArray;
        }

        var ctxItems = ctx.EnumerateArray().ToArray();
        if (ctxItems.Length != 2)
        {
            return TituloGraduacionVcDocumentErrorCodes.DocumentContextLengthInvalid;
        }

        if (ctxItems[0].ValueKind != JsonValueKind.String ||
            !string.Equals(ctxItems[0].GetString(), TituloGraduacionVcDocumentConstants.W3CVerifiableCredentialsContextV1, StringComparison.Ordinal))
        {
            return TituloGraduacionVcDocumentErrorCodes.DocumentContextFirstMismatch;
        }

        if (ctxItems[1].ValueKind != JsonValueKind.Object)
        {
            return TituloGraduacionVcDocumentErrorCodes.DocumentContextSecondNotObject;
        }

        if (!SecondContextMatches(ctxItems[1]))
        {
            return TituloGraduacionVcDocumentErrorCodes.DocumentContextSecondMismatch;
        }

        if (!vc.TryGetProperty("type", out var typeEl) || typeEl.ValueKind != JsonValueKind.Array)
        {
            return TituloGraduacionVcDocumentErrorCodes.DocumentTypeMissing;
        }

        var typeSet = new HashSet<string>(StringComparer.Ordinal);
        foreach (var el in typeEl.EnumerateArray())
        {
            if (el.ValueKind != JsonValueKind.String)
            {
                return TituloGraduacionVcDocumentErrorCodes.DocumentTypeInvalid;
            }

            typeSet.Add(el.GetString()!);
        }

        if (typeSet.Count != 2 ||
            !typeSet.Contains("VerifiableCredential") ||
            !typeSet.Contains("TituloGraduacionCredential"))
        {
            return TituloGraduacionVcDocumentErrorCodes.DocumentTypeInvalid;
        }

        if (!vc.TryGetProperty("id", out var idEl) || idEl.ValueKind != JsonValueKind.String)
        {
            return TituloGraduacionVcDocumentErrorCodes.DocumentIdMissing;
        }

        if (!UrnUuidLowerRegex().IsMatch(idEl.GetString()!))
        {
            return TituloGraduacionVcDocumentErrorCodes.DocumentIdFormat;
        }

        if (!vc.TryGetProperty("issuer", out var issuerEl) || issuerEl.ValueKind != JsonValueKind.String)
        {
            return TituloGraduacionVcDocumentErrorCodes.DocumentIssuerMissing;
        }

        if (!EthrSepoliaDidParser.TryParse(issuerEl.GetString()!, out _, out _))
        {
            return TituloGraduacionVcDocumentErrorCodes.DocumentIssuerDidInvalid;
        }

        if (!vc.TryGetProperty("issuanceDate", out var issEl) || issEl.ValueKind != JsonValueKind.String)
        {
            return TituloGraduacionVcDocumentErrorCodes.DocumentIssuanceDateMissing;
        }

        if (!TryParseRfc3339UtcZ(issEl.GetString()!, out var issuanceDto))
        {
            return TituloGraduacionVcDocumentErrorCodes.DocumentIssuanceDateFormat;
        }

        if (issuanceDto > nowUtc)
        {
            return TituloGraduacionVcDocumentErrorCodes.DocumentIssuanceDateFuture;
        }

        if (vc.TryGetProperty("expirationDate", out var expEl) && expEl.ValueKind == JsonValueKind.String)
        {
            if (!TryParseRfc3339UtcZ(expEl.GetString()!, out var expDto))
            {
                return TituloGraduacionVcDocumentErrorCodes.DocumentExpirationDateFormat;
            }

            if (expDto < nowUtc)
            {
                return TituloGraduacionVcDocumentErrorCodes.DocumentExpirationDateExpired;
            }
        }

        if (!vc.TryGetProperty("credentialSubject", out var subj))
        {
            return TituloGraduacionVcDocumentErrorCodes.DocumentSubjectMissing;
        }

        var subErr = ValidateCredentialSubject(subj);
        if (subErr is not null)
        {
            return subErr;
        }

        if (mode == CredentialValidationMode.Signed)
        {
            return ValidateProof(vc);
        }

        return null;
    }

    private static bool SecondContextMatches(JsonElement actual)
    {
        var expected = TituloGraduacionVcDocumentConstants.CreateSecondContextObject();
        using var expectedDoc = JsonDocument.Parse(expected.ToJsonString());
        var expectedRoot = expectedDoc.RootElement;

        if (CountProperties(actual) != CountProperties(expectedRoot))
        {
            return false;
        }

        foreach (var prop in expectedRoot.EnumerateObject())
        {
            if (!actual.TryGetProperty(prop.Name, out var a) || a.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            if (!string.Equals(a.GetString(), prop.Value.GetString(), StringComparison.Ordinal))
            {
                return false;
            }
        }

        foreach (var prop in actual.EnumerateObject())
        {
            if (!expectedRoot.TryGetProperty(prop.Name, out _))
            {
                return false;
            }
        }

        return true;
    }

    private static int CountProperties(JsonElement obj)
    {
        var n = 0;
        foreach (var _ in obj.EnumerateObject())
        {
            n++;
        }

        return n;
    }

    private static string? ValidateCredentialSubject(JsonElement subject)
    {
        if (subject.ValueKind != JsonValueKind.Object)
        {
            return TituloGraduacionVcDocumentErrorCodes.DocumentSubjectNotObject;
        }

        var allowed = AllowedSubjectPropertyNames;
        foreach (var prop in subject.EnumerateObject())
        {
            if (!allowed.Contains(prop.Name))
            {
                return TituloGraduacionVcDocumentErrorCodes.DocumentSubjectExtraProperty;
            }
        }

        if (!subject.TryGetProperty("id", out var idEl) || idEl.ValueKind != JsonValueKind.String)
        {
            return TituloGraduacionVcDocumentErrorCodes.DocumentSubjectIdMissing;
        }

        if (!EthrSepoliaDidParser.TryParse(idEl.GetString()!, out _, out _))
        {
            return TituloGraduacionVcDocumentErrorCodes.DocumentSubjectIdDidInvalid;
        }

        var degreeErr = RequireNonEmptyString(subject, "degreeTitle",
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectDegreeTitleMissing,
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectDegreeTitleEmpty);
        if (degreeErr is not null)
        {
            return degreeErr;
        }

        var programErr = RequireNonEmptyString(subject, "programName",
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectProgramNameMissing,
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectProgramNameEmpty);
        if (programErr is not null)
        {
            return programErr;
        }

        var awardErr = RequireNonEmptyString(subject, "awardDate",
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectAwardDateMissing,
            TituloGraduacionVcDocumentErrorCodes.DocumentSubjectAwardDateEmpty);
        if (awardErr is not null)
        {
            return awardErr;
        }

        var award = subject.GetProperty("awardDate").GetString()!;
        if (!AwardDatePattern().IsMatch(award))
        {
            return TituloGraduacionVcDocumentErrorCodes.DocumentSubjectAwardDateFormat;
        }

        if (!DateOnly.TryParseExact(
                award,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _))
        {
            return TituloGraduacionVcDocumentErrorCodes.DocumentSubjectAwardDateInvalid;
        }

        return null;
    }

    private static string? RequireNonEmptyString(
        JsonElement subject,
        string name,
        string missingCode,
        string emptyCode)
    {
        if (!subject.TryGetProperty(name, out var p) || p.ValueKind != JsonValueKind.String)
        {
            return missingCode;
        }

        if (string.IsNullOrWhiteSpace(p.GetString()))
        {
            return emptyCode;
        }

        return null;
    }

    private static string? ValidateProof(JsonElement vc)
    {
        if (!vc.TryGetProperty("proof", out var proof) || proof.ValueKind != JsonValueKind.Object)
        {
            return TituloGraduacionVcDocumentErrorCodes.DocumentProofMissing;
        }

        if (!proof.TryGetProperty("type", out var t) || t.ValueKind != JsonValueKind.String ||
            !string.Equals(t.GetString(), TituloGraduacionVcDocumentConstants.ProofType, StringComparison.Ordinal))
        {
            return TituloGraduacionVcDocumentErrorCodes.DocumentProofTypeInvalid;
        }

        if (!proof.TryGetProperty("proofValue", out var pv) || pv.ValueKind != JsonValueKind.String)
        {
            return TituloGraduacionVcDocumentErrorCodes.DocumentProofValueMissing;
        }

        if (string.IsNullOrWhiteSpace(pv.GetString()))
        {
            return TituloGraduacionVcDocumentErrorCodes.DocumentProofValueEmpty;
        }

        return null;
    }

    private static bool TryParseRfc3339UtcZ(string value, out DateTimeOffset dto)
    {
        dto = default;
        if (string.IsNullOrEmpty(value) || !value.EndsWith("Z", StringComparison.Ordinal))
        {
            return false;
        }

        if (!DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out dto))
        {
            return false;
        }

        return dto.Offset == TimeSpan.Zero;
    }
}
