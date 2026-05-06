using System.Text.Json;
using System.Text.Json.Nodes;
using SovereignID.VerifiableCredential.Document;

namespace SovereignID.Issuer.Domain.TituloGraduacion;

/// <summary>
/// Construye el documento JSON-LD del VC (sin <c>proof</c>) según slice A.
/// </summary>
public static class VerifiableCredentialJsonBuilder
{
    /// <summary>Delegado en <see cref="TituloGraduacionVcDocumentConstants"/> (paridad con EIP-712).</summary>
    public const string ProofType = TituloGraduacionVcDocumentConstants.ProofType;

    /// <summary>
    /// Crea el objeto JSON del VC sin la prueba criptográfica.
    /// </summary>
    public static JsonObject BuildUnsignedCredential(
        string credentialIdUrn,
        string issuerDid,
        string subjectDid,
        TituloGraduacionClaims claims,
        string issuanceDateRfc3339UtcZ,
        string? expirationDateRfc3339UtcZ)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(credentialIdUrn);
        ArgumentException.ThrowIfNullOrWhiteSpace(issuerDid);
        ArgumentException.ThrowIfNullOrWhiteSpace(subjectDid);
        ArgumentException.ThrowIfNullOrWhiteSpace(issuanceDateRfc3339UtcZ);

        var subject = new JsonObject
        {
            ["id"] = subjectDid,
            ["degreeTitle"] = claims.DegreeTitle,
            ["programName"] = claims.ProgramName,
            ["awardDate"] = claims.AwardDate,
        };

        var secondContext = TituloGraduacionVcDocumentConstants.CreateSecondContextObject();

        var context = new JsonArray(TituloGraduacionVcDocumentConstants.W3CVerifiableCredentialsContextV1, secondContext);

        var types = new JsonArray("VerifiableCredential", "TituloGraduacionCredential");

        var root = new JsonObject
        {
            ["@context"] = context,
            ["id"] = credentialIdUrn,
            ["type"] = types,
            ["issuer"] = issuerDid,
            ["issuanceDate"] = issuanceDateRfc3339UtcZ,
            ["credentialSubject"] = subject,
        };

        if (!string.IsNullOrEmpty(expirationDateRfc3339UtcZ))
        {
            root["expirationDate"] = expirationDateRfc3339UtcZ;
        }

        return root;
    }

    /// <summary>
    /// Añade <c>proof</c> coherente con el tipo de prueba acordado para slice A.
    /// </summary>
    public static void AttachIssuerProof(JsonObject credentialRoot, string issuerDid, string signatureHex)
    {
        ArgumentNullException.ThrowIfNull(credentialRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(issuerDid);
        ArgumentException.ThrowIfNullOrWhiteSpace(signatureHex);

        var proof = new JsonObject
        {
            ["type"] = ProofType,
            ["proofPurpose"] = "assertionMethod",
            ["verificationMethod"] = issuerDid + "#controller",
            ["proofValue"] = signatureHex,
        };
        credentialRoot["proof"] = proof;
    }

    /// <summary>
    /// Serializa compacta (sin indentación) para transporte estable.
    /// </summary>
    public static string Serialize(JsonObject root)
    {
        return root.ToJsonString(
            new JsonSerializerOptions { WriteIndented = false });
    }
}
