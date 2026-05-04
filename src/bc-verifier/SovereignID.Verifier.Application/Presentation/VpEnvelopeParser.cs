using System.Text.Json;
using SovereignID.SharedKernel.Domain;

namespace SovereignID.Verifier.Application.Presentation;

/// <summary>
/// Valida la envoltura mínima de la VP (slice A): <c>type</c>, <c>holder</c> y un único VC en <c>verifiableCredential</c>.
/// No valida documento del VC ni exige <c>proof</c> en la raíz de la VP.
/// </summary>
internal static class VpEnvelopeParser
{
    /// <summary>
    /// Interpreta la envoltura VP y devuelve el VC incrustado único.
    /// </summary>
    internal static bool TryParse(
        JsonElement root,
        out string holderDid,
        out EthereumAddress? holderAddr,
        out JsonElement embeddedVc,
        out string? errorCode)
    {
        holderDid = "";
        holderAddr = null;
        embeddedVc = default;
        errorCode = null;

        if (root.ValueKind != JsonValueKind.Object)
        {
            errorCode = "vp_not_object";
            return false;
        }

        if (!root.TryGetProperty("type", out var vpType) || vpType.ValueKind != JsonValueKind.Array)
        {
            errorCode = "vp_type_missing";
            return false;
        }

        var vpTypes = vpType.EnumerateArray().ToArray();
        if (vpTypes.Length != 1 || vpTypes[0].ValueKind != JsonValueKind.String ||
            vpTypes[0].GetString() != "VerifiablePresentation")
        {
            errorCode = "vp_type_invalid";
            return false;
        }

        if (!root.TryGetProperty("holder", out var holderEl) || holderEl.ValueKind != JsonValueKind.String)
        {
            errorCode = "vp_holder_missing";
            return false;
        }

        holderDid = holderEl.GetString()!;
        if (!EthrSepoliaDidParser.TryParse(holderDid, out _, out var addrParsed))
        {
            errorCode = "vp_holder_did_invalid";
            return false;
        }

        holderAddr = addrParsed;

        if (!root.TryGetProperty("verifiableCredential", out var vcArr) || vcArr.ValueKind != JsonValueKind.Array)
        {
            errorCode = "vp_verifiableCredential_missing";
            return false;
        }

        var vcList = vcArr.EnumerateArray().ToArray();
        if (vcList.Length != 1)
        {
            errorCode = "vp_verifiableCredential_count";
            return false;
        }

        embeddedVc = vcList[0];
        if (embeddedVc.ValueKind != JsonValueKind.Object)
        {
            errorCode = "vc_not_object";
            return false;
        }

        return true;
    }
}
