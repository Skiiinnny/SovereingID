using System.Text.Json;
using System.Text.RegularExpressions;
using SovereignID.SharedKernel.Domain;
using SovereignID.VcSliceA.Eip712;

namespace SovereignID.Verifier.Application.Presentation;

/// <summary>
/// Valida forma VP/VC, ventana temporal y firmas EIP-712 del emisor y del titular.
/// </summary>
public static partial class PresentationVerifier
{
    private const string W3CContext = "https://www.w3.org/2018/credentials/v1";

    [GeneratedRegex(@"^urn:uuid:[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", RegexOptions.CultureInvariant)]
    private static partial Regex UrnUuidLowerRegex();

    /// <summary>
    /// Verifica la presentación JSON frente al reloj inyectado (UTC).
    /// </summary>
    public static PresentationVerificationOutcome Verify(string presentationJson, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(presentationJson))
        {
            return new PresentationVerificationOutcome(false, "vp_json_empty");
        }

        using var doc = JsonDocument.Parse(presentationJson);
        var root = doc.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            return new PresentationVerificationOutcome(false, "vp_not_object");
        }

        if (!root.TryGetProperty("type", out var vpType) || vpType.ValueKind != JsonValueKind.Array)
        {
            return new PresentationVerificationOutcome(false, "vp_type_missing");
        }

        var vpTypes = vpType.EnumerateArray().ToArray();
        if (vpTypes.Length != 1 || vpTypes[0].ValueKind != JsonValueKind.String ||
            vpTypes[0].GetString() != "VerifiablePresentation")
        {
            return new PresentationVerificationOutcome(false, "vp_type_invalid");
        }

        if (!root.TryGetProperty("holder", out var holderEl) || holderEl.ValueKind != JsonValueKind.String)
        {
            return new PresentationVerificationOutcome(false, "vp_holder_missing");
        }

        var holderDid = holderEl.GetString()!;
        if (!EthrSepoliaDidParser.TryParse(holderDid, out _, out var holderAddr))
        {
            return new PresentationVerificationOutcome(false, "vp_holder_did_invalid");
        }

        if (!root.TryGetProperty("verifiableCredential", out var vcArr) || vcArr.ValueKind != JsonValueKind.Array)
        {
            return new PresentationVerificationOutcome(false, "vp_verifiableCredential_missing");
        }

        var vcList = vcArr.EnumerateArray().ToArray();
        if (vcList.Length != 1)
        {
            return new PresentationVerificationOutcome(false, "vp_verifiableCredential_count");
        }

        var vc = vcList[0];
        if (vc.ValueKind != JsonValueKind.Object)
        {
            return new PresentationVerificationOutcome(false, "vc_not_object");
        }

        var vcOutcome = VerifyEmbeddedVc(vc, nowUtc, holderDid, holderAddr!);
        if (!vcOutcome.IsSuccess)
        {
            return vcOutcome;
        }

        if (!root.TryGetProperty("proof", out var vpProof) || vpProof.ValueKind != JsonValueKind.Object)
        {
            return new PresentationVerificationOutcome(false, "vp_proof_missing");
        }

        if (!TryGetProofSignature(vpProof, out var vpSig, out var vpProofErr))
        {
            return new PresentationVerificationOutcome(false, vpProofErr);
        }

        if (!vc.TryGetProperty("id", out var vcIdEl) || vcIdEl.ValueKind != JsonValueKind.String)
        {
            return new PresentationVerificationOutcome(false, "vc_id_missing");
        }

        var vcId = vcIdEl.GetString()!;
        if (!vc.TryGetProperty("issuer", out var issuerEl) || issuerEl.ValueKind != JsonValueKind.String)
        {
            return new PresentationVerificationOutcome(false, "vc_issuer_missing");
        }

        var issuerDid = issuerEl.GetString()!;

        var vp712 = new VerifiablePresentation712
        {
            Holder = holderDid,
            VcCredentialId = vcId,
            VcIssuer = issuerDid,
        };

        string recoveredHolder;
        try
        {
            recoveredHolder = VerifiablePresentationTypedData.RecoverSignerAddress(vp712, vpSig);
        }
        catch
        {
            return new PresentationVerificationOutcome(false, "vp_signature_recover_failed");
        }

        if (!string.Equals(recoveredHolder, holderAddr!.Value, StringComparison.Ordinal))
        {
            return new PresentationVerificationOutcome(false, "vp_holder_signature_mismatch");
        }

        return new PresentationVerificationOutcome(true, null);
    }

    private static PresentationVerificationOutcome VerifyEmbeddedVc(
        JsonElement vc,
        DateTimeOffset nowUtc,
        string holderDid,
        EthereumAddress holderAddr)
    {
        if (!vc.TryGetProperty("@context", out var ctx) || ctx.ValueKind != JsonValueKind.Array)
        {
            return new PresentationVerificationOutcome(false, "vc_context_missing");
        }

        var ctxItems = ctx.EnumerateArray().ToArray();
        if (ctxItems.Length < 2 ||
            ctxItems[0].ValueKind != JsonValueKind.String ||
            ctxItems[0].GetString() != W3CContext ||
            ctxItems[1].ValueKind != JsonValueKind.Object)
        {
            return new PresentationVerificationOutcome(false, "vc_context_shape");
        }

        if (!vc.TryGetProperty("type", out var typeEl) || typeEl.ValueKind != JsonValueKind.Array)
        {
            return new PresentationVerificationOutcome(false, "vc_type_missing");
        }

        var typeSet = new HashSet<string>(StringComparer.Ordinal);
        foreach (var el in typeEl.EnumerateArray())
        {
            if (el.ValueKind != JsonValueKind.String)
            {
                return new PresentationVerificationOutcome(false, "vc_type_invalid");
            }

            typeSet.Add(el.GetString()!);
        }

        if (typeSet.Count != 2 ||
            !typeSet.Contains("VerifiableCredential") ||
            !typeSet.Contains("TituloGraduacionCredential"))
        {
            return new PresentationVerificationOutcome(false, "vc_type_invalid");
        }

        if (!vc.TryGetProperty("id", out var idEl) || idEl.ValueKind != JsonValueKind.String)
        {
            return new PresentationVerificationOutcome(false, "vc_id_missing");
        }

        var id = idEl.GetString()!;
        if (!UrnUuidLowerRegex().IsMatch(id))
        {
            return new PresentationVerificationOutcome(false, "vc_id_format");
        }

        if (!vc.TryGetProperty("issuer", out var issuerEl) || issuerEl.ValueKind != JsonValueKind.String)
        {
            return new PresentationVerificationOutcome(false, "vc_issuer_missing");
        }

        var issuerDid = issuerEl.GetString()!;
        if (!EthrSepoliaDidParser.TryParse(issuerDid, out _, out var issuerAddr))
        {
            return new PresentationVerificationOutcome(false, "vc_issuer_did_invalid");
        }

        if (!vc.TryGetProperty("issuanceDate", out var issEl) || issEl.ValueKind != JsonValueKind.String)
        {
            return new PresentationVerificationOutcome(false, "vc_issuanceDate_missing");
        }

        if (!TryParseRfc3339Utc(issEl.GetString()!, out var issuanceDto))
        {
            return new PresentationVerificationOutcome(false, "vc_issuanceDate_format");
        }

        if (issuanceDto > nowUtc)
        {
            return new PresentationVerificationOutcome(false, "vc_issuanceDate_future");
        }

        if (vc.TryGetProperty("expirationDate", out var expEl) && expEl.ValueKind == JsonValueKind.String)
        {
            if (!TryParseRfc3339Utc(expEl.GetString()!, out var expDto))
            {
                return new PresentationVerificationOutcome(false, "vc_expirationDate_format");
            }

            if (expDto < nowUtc)
            {
                return new PresentationVerificationOutcome(false, "vc_expirationDate_expired");
            }
        }

        if (!vc.TryGetProperty("credentialSubject", out var subj))
        {
            return new PresentationVerificationOutcome(false, "vc_subject_missing");
        }

        var subErr = EmbeddedTituloGraduacionSubjectValidator.ValidateSubject(subj);
        if (subErr is not null)
        {
            return new PresentationVerificationOutcome(false, subErr);
        }

        var subjectId = subj.GetProperty("id").GetString()!;
        if (!EthrSepoliaDidParser.TryParse(subjectId, out _, out var subjectAddr))
        {
            return new PresentationVerificationOutcome(false, "vc_subject_did_invalid");
        }

        if (!string.Equals(holderDid, subjectId, StringComparison.Ordinal))
        {
            return new PresentationVerificationOutcome(false, "vp_holder_subject_mismatch");
        }

        if (!string.Equals(holderAddr.Value, subjectAddr!.Value, StringComparison.Ordinal))
        {
            return new PresentationVerificationOutcome(false, "vp_holder_address_mismatch");
        }

        if (!vc.TryGetProperty("proof", out var vcProof) || vcProof.ValueKind != JsonValueKind.Object)
        {
            return new PresentationVerificationOutcome(false, "vc_proof_missing");
        }

        if (!TryGetProofSignature(vcProof, out var vcSig, out var vcProofErr))
        {
            return new PresentationVerificationOutcome(false, vcProofErr);
        }

        var eip712 = new TituloGraduacionCredential712
        {
            CredentialId = id,
            Issuer = issuerDid,
            CredentialSubjectId = subjectId,
            DegreeTitle = subj.GetProperty("degreeTitle").GetString()!,
            ProgramName = subj.GetProperty("programName").GetString()!,
            AwardDate = subj.GetProperty("awardDate").GetString()!,
            IssuanceDate = issEl.GetString()!,
            ExpirationDate = vc.TryGetProperty("expirationDate", out var ex) && ex.ValueKind == JsonValueKind.String
                ? ex.GetString()!
                : string.Empty,
        };

        string recoveredIssuer;
        try
        {
            recoveredIssuer = TituloGraduacionVcTypedData.RecoverSignerAddress(eip712, vcSig);
        }
        catch
        {
            return new PresentationVerificationOutcome(false, "vc_signature_recover_failed");
        }

        if (!string.Equals(recoveredIssuer, issuerAddr!.Value, StringComparison.Ordinal))
        {
            return new PresentationVerificationOutcome(false, "vc_issuer_signature_mismatch");
        }

        return new PresentationVerificationOutcome(true, null);
    }

    private static bool TryGetProofSignature(JsonElement proof, out string signature, out string? errorCode)
    {
        signature = "";
        errorCode = null;
        if (!proof.TryGetProperty("type", out var t) || t.GetString() != VcSliceAEip712Constants.ProofType)
        {
            errorCode = "proof_type_invalid";
            return false;
        }

        if (!proof.TryGetProperty("proofValue", out var pv) || pv.ValueKind != JsonValueKind.String)
        {
            errorCode = "proof_value_missing";
            return false;
        }

        signature = pv.GetString()!;
        if (string.IsNullOrWhiteSpace(signature))
        {
            errorCode = "proof_value_empty";
            return false;
        }

        return true;
    }

    private static bool TryParseRfc3339Utc(string value, out DateTimeOffset dto)
    {
        if (DateTimeOffset.TryParse(
                value,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal |
                System.Globalization.DateTimeStyles.AdjustToUniversal,
                out dto))
        {
            return true;
        }

        dto = default;
        return false;
    }
}
