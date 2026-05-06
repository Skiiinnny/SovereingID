using System.Text.Json.Nodes;

namespace SovereignID.VerifiableCredential.Document;

/// <summary>
/// Vocabulario y literales canónicos del VC TituloGraduacion en JSON-LD (producto cerrado).
/// </summary>
public static class TituloGraduacionVcDocumentConstants
{
    /// <summary>Primer elemento de <c>@context</c> (W3C VC v1).</summary>
    public const string W3CVerifiableCredentialsContextV1 = "https://www.w3.org/2018/credentials/v1";

    /// <summary>Base IRI del vocabulario de producto cerrado.</summary>
    public const string VocabBase = "https://sovereignid.local/vocab/titulo-graduacion/v1#";

    /// <summary>Literal de <c>proof.type</c> compartido con EIP-712.</summary>
    public const string ProofType = "SovereignIDEip712Signature2026";

    /// <summary>
    /// Segundo objeto de <c>@context</c> (debe coincidir byte a byte en serialización lógica: mismas claves y valores).
    /// </summary>
    public static JsonObject CreateSecondContextObject()
    {
        return new JsonObject
        {
            ["TituloGraduacionCredential"] = VocabBase + "TituloGraduacionCredential",
            ["degreeTitle"] = VocabBase + "degreeTitle",
            ["programName"] = VocabBase + "programName",
            ["awardDate"] = VocabBase + "awardDate",
        };
    }
}
