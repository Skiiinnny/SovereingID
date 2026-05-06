using System.Text.Json;
using SovereignID.VerifiableCredential.Eip712;

namespace SovereignID.Verifier.Application.Presentation;

/// <summary>
/// Lectura compartida de <c>proof.type</c> y <c>proofValue</c> para pruebas EIP-712 del VC y de la VP.
/// </summary>
internal static class PresentationEip712ProofValueReader
{
    /// <summary>
    /// Intenta obtener la firma hex desde un objeto <c>proof</c> JSON.
    /// </summary>
    /// <returns><see langword="true"/> si la firma está presente y no vacía; códigos: <c>proof_type_invalid</c>, <c>proof_value_missing</c>, <c>proof_value_empty</c>.</returns>
    internal static bool TryGetProofSignature(JsonElement proof, out string signature, out string? errorCode)
    {
        signature = "";
        errorCode = null;
        if (!proof.TryGetProperty("type", out var t) || t.GetString() != VerifiableCredentialEip712Constants.ProofType)
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
}
