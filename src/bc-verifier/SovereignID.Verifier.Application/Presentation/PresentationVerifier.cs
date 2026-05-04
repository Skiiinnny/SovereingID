using System.Text.Json;

namespace SovereignID.Verifier.Application.Presentation;

/// <summary>
/// Valida forma VP/VC, ventana temporal y firmas EIP-712 del emisor y del titular.
/// </summary>
public static class PresentationVerifier
{
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
        if (!VpEnvelopeParser.TryParse(root, out var holderDid, out var holderAddr, out var vc, out var envelopeErr))
        {
            return new PresentationVerificationOutcome(false, envelopeErr);
        }

        return EmbeddedTituloGraduacionVpChain.Verify(root, vc, nowUtc, holderDid, holderAddr!);
    }
}
