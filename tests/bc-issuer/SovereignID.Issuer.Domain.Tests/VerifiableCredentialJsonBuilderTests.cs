using System.Text.Json;
using SovereignID.Issuer.Domain.TituloGraduacion;
using SovereignID.VcSliceA.Document;

namespace SovereignID.Issuer.Domain.Tests;

public class VerifiableCredentialJsonBuilderTests
{
    [Fact]
    public void BuildUnsignedCredential_contains_required_json_ld_shape()
    {
        var claims = new TituloGraduacionClaims("Grado en Ingeniería", "Informática", "2025-06-15");
        var root = VerifiableCredentialJsonBuilder.BuildUnsignedCredential(
            "urn:uuid:aaaaaaaa-bbbb-4ccc-dddd-eeeeeeeeeeee",
            "did:ethr:sepolia:0x1111111111111111111111111111111111111111",
            "did:ethr:sepolia:0x2222222222222222222222222222222222222222",
            claims,
            "2026-05-03T12:00:00Z",
            null);

        using var doc = JsonDocument.Parse(root.ToJsonString());
        var el = doc.RootElement;
        Assert.Equal(JsonValueKind.Array, el.GetProperty("@context").ValueKind);
        Assert.Equal(TituloGraduacionVcDocumentConstants.W3CVerifiableCredentialsContextV1, el.GetProperty("@context")[0].GetString());
        Assert.Equal(JsonValueKind.Object, el.GetProperty("@context")[1].ValueKind);
        Assert.Equal("urn:uuid:aaaaaaaa-bbbb-4ccc-dddd-eeeeeeeeeeee", el.GetProperty("id").GetString());

        var types = el.GetProperty("type").EnumerateArray().Select(x => x.GetString()).OrderBy(x => x).ToArray();
        Assert.Equal(new[] { "TituloGraduacionCredential", "VerifiableCredential" }, types);

        Assert.Equal(
            "did:ethr:sepolia:0x1111111111111111111111111111111111111111",
            el.GetProperty("issuer").GetString());

        var sub = el.GetProperty("credentialSubject");
        Assert.Equal("did:ethr:sepolia:0x2222222222222222222222222222222222222222", sub.GetProperty("id").GetString());
        Assert.Equal("Grado en Ingeniería", sub.GetProperty("degreeTitle").GetString());
        Assert.False(el.TryGetProperty("proof", out _));
    }

    [Fact]
    public void AttachIssuerProof_adds_proof_object()
    {
        var claims = new TituloGraduacionClaims("G", "P", "2025-01-01");
        var root = VerifiableCredentialJsonBuilder.BuildUnsignedCredential(
            "urn:uuid:bbbbbbbb-bbbb-4bbb-bbbb-bbbbbbbbbbbb",
            "did:ethr:sepolia:0xaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
            "did:ethr:sepolia:0xbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb",
            claims,
            "2026-01-01T00:00:00Z",
            null);

        VerifiableCredentialJsonBuilder.AttachIssuerProof(
            root,
            "did:ethr:sepolia:0xaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
            "0x" + new string('c', 130));

        using var doc = JsonDocument.Parse(root.ToJsonString());
        var proof = doc.RootElement.GetProperty("proof");
        Assert.Equal(VerifiableCredentialJsonBuilder.ProofType, proof.GetProperty("type").GetString());
        Assert.Equal("assertionMethod", proof.GetProperty("proofPurpose").GetString());
        Assert.StartsWith("0x", proof.GetProperty("proofValue").GetString(), StringComparison.Ordinal);
    }
}
