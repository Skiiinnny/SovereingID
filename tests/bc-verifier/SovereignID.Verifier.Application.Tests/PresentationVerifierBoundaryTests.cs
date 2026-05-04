using System.Text.Json.Nodes;
using SovereignID.Verifier.Application.Presentation;

namespace SovereignID.Verifier.Application.Tests;

/// <summary>
/// VP inválidos que fallan antes de <see cref="PresentationVerifier"/> validar un VC firmado.
/// </summary>
public sealed class PresentationVerifierBoundaryTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse(
        "2026-05-03T12:00:00Z",
        System.Globalization.CultureInfo.InvariantCulture);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Verify_empty_presentation_json_fails(string json)
    {
        var o = PresentationVerifier.Verify(json, Now);
        Assert.False(o.IsSuccess);
        Assert.Equal("vp_json_empty", o.ErrorCode);
    }

    [Fact]
    public void Verify_non_object_root_fails()
    {
        var o = PresentationVerifier.Verify("[]", Now);
        Assert.False(o.IsSuccess);
        Assert.Equal("vp_not_object", o.ErrorCode);
    }

    [Fact]
    public void Verify_missing_type_fails()
    {
        var o = PresentationVerifier.Verify("{}", Now);
        Assert.False(o.IsSuccess);
        Assert.Equal("vp_type_missing", o.ErrorCode);
    }

    [Fact]
    public void Verify_vp_type_not_array_fails()
    {
        var json = new JsonObject { ["type"] = "VerifiablePresentation" }.ToJsonString();
        var o = PresentationVerifier.Verify(json, Now);
        Assert.False(o.IsSuccess);
        Assert.Equal("vp_type_missing", o.ErrorCode);
    }

    [Fact]
    public void Verify_invalid_vp_type_fails()
    {
        var json = new JsonObject
        {
            ["type"] = new JsonArray("OtherType"),
            ["holder"] = "did:ethr:sepolia:0xabcdef0123456789abcdef0123456789abcdef01",
            ["verifiableCredential"] = new JsonArray(),
        }.ToJsonString();

        var o = PresentationVerifier.Verify(json, Now);
        Assert.False(o.IsSuccess);
        Assert.Equal("vp_type_invalid", o.ErrorCode);
    }

    [Fact]
    public void Verify_holder_missing_fails()
    {
        var json = new JsonObject
        {
            ["type"] = new JsonArray("VerifiablePresentation"),
            ["verifiableCredential"] = new JsonArray(),
        }.ToJsonString();

        var o = PresentationVerifier.Verify(json, Now);
        Assert.False(o.IsSuccess);
        Assert.Equal("vp_holder_missing", o.ErrorCode);
    }

    [Fact]
    public void Verify_holder_did_invalid_fails()
    {
        var json = new JsonObject
        {
            ["type"] = new JsonArray("VerifiablePresentation"),
            ["holder"] = "not-a-did",
            ["verifiableCredential"] = new JsonArray(new JsonObject()),
        }.ToJsonString();

        var o = PresentationVerifier.Verify(json, Now);
        Assert.False(o.IsSuccess);
        Assert.Equal("vp_holder_did_invalid", o.ErrorCode);
    }

    [Fact]
    public void Verify_missing_verifiableCredential_fails()
    {
        var json = new JsonObject
        {
            ["type"] = new JsonArray("VerifiablePresentation"),
            ["holder"] = "did:ethr:sepolia:0xabcdef0123456789abcdef0123456789abcdef01",
        }.ToJsonString();

        var o = PresentationVerifier.Verify(json, Now);
        Assert.False(o.IsSuccess);
        Assert.Equal("vp_verifiableCredential_missing", o.ErrorCode);
    }

    [Fact]
    public void Verify_zero_embedded_credentials_fails()
    {
        var json = new JsonObject
        {
            ["type"] = new JsonArray("VerifiablePresentation"),
            ["holder"] = "did:ethr:sepolia:0xabcdef0123456789abcdef0123456789abcdef01",
            ["verifiableCredential"] = new JsonArray(),
        }.ToJsonString();

        var o = PresentationVerifier.Verify(json, Now);
        Assert.False(o.IsSuccess);
        Assert.Equal("vp_verifiableCredential_count", o.ErrorCode);
    }

    [Fact]
    public void Verify_vc_not_object_fails()
    {
        var json = new JsonObject
        {
            ["type"] = new JsonArray("VerifiablePresentation"),
            ["holder"] = "did:ethr:sepolia:0xabcdef0123456789abcdef0123456789abcdef01",
            ["verifiableCredential"] = new JsonArray("not-an-object"),
        }.ToJsonString();

        var o = PresentationVerifier.Verify(json, Now);
        Assert.False(o.IsSuccess);
        Assert.Equal("vc_not_object", o.ErrorCode);
    }
}
