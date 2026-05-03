using System.Text.Json;
using System.Text.Json.Nodes;
using Nethereum.Signer;
using SovereignID.Issuer.Application.TituloGraduacion;
using SovereignID.Issuer.Infrastructure.TituloGraduacion;
using SovereignID.SharedKernel.Domain;
using SovereignID.Verifier.Application.Presentation;
using SovereignID.VcSliceA.Eip712;

namespace SovereignID.Verifier.Domain.Tests;

public class PresentationVerifierTests
{
    private static readonly DateTimeOffset ReferenceInstant =
        DateTimeOffset.Parse("2026-05-03T12:00:00Z", System.Globalization.CultureInfo.InvariantCulture);

    private static string Did(EthECKey key)
    {
        var a = key.GetPublicAddress();
        var hex = (a.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? a[2..] : a).ToLowerInvariant();
        return "did:ethr:sepolia:0x" + hex;
    }

    private static async Task<(string VcJson, string IssuerDid, string HolderDid, EthECKey IssuerKey, EthECKey HolderKey)> IssueVcAsync(
        DateTimeOffset issuanceClock,
        Guid credentialGuid,
        DateTimeOffset? expirationUtc = null)
    {
        var issuerKey = EthECKey.GenerateKey();
        var holderKey = EthECKey.GenerateKey();
        var issuerDid = Did(issuerKey);
        var holderDid = Did(holderKey);
        var handler = new IssueTituloGraduacionCredentialCommandHandler(
            new NethereumIssuerVcIntegritySigner(issuerKey),
            new TestClock(issuanceClock),
            new TestGuid(credentialGuid));

        var result = await handler.HandleAsync(
            new IssueTituloGraduacionCredentialCommand(
                issuerDid,
                holderDid,
                "Grado en Ingeniería",
                "Informática",
                "2025-06-01",
                expirationUtc),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        return (result.CredentialJson!, issuerDid, holderDid, issuerKey, holderKey);
    }

    private static string WrapVp(string vcJson, string holderDid, string issuerDid, EthECKey holderKey)
    {
        var vcNode = JsonNode.Parse(vcJson)!;
        var credentialId = vcNode["id"]!.GetValue<string>();
        var vp = new JsonObject
        {
            ["type"] = new JsonArray("VerifiablePresentation"),
            ["holder"] = holderDid,
            ["verifiableCredential"] = new JsonArray(vcNode),
        };

        var vp712 = new VerifiablePresentation712
        {
            Holder = holderDid,
            VcCredentialId = credentialId,
            VcIssuer = issuerDid,
        };
        var sig = VerifiablePresentationTypedData.Sign(vp712, holderKey);
        vp["proof"] = new JsonObject
        {
            ["type"] = VcSliceAEip712Constants.ProofType,
            ["proofPurpose"] = "authentication",
            ["verificationMethod"] = holderDid + "#controller",
            ["proofValue"] = sig,
        };

        return vp.ToJsonString();
    }

    [Fact]
    public async Task Verify_valid_vp_passes()
    {
        var (vc, issuerDid, holderDid, _, holderKey) = await IssueVcAsync(
            ReferenceInstant,
            Guid.Parse("aaaaaaaa-bbbb-4ccc-dddd-eeeeeeeeeeee"));

        var vpJson = WrapVp(vc, holderDid, issuerDid, holderKey);
        var outcome = PresentationVerifier.Verify(vpJson, ReferenceInstant);
        Assert.True(outcome.IsSuccess);
        Assert.Null(outcome.ErrorCode);
    }

    [Fact]
    public async Task Verify_wrong_issuer_signature_fails()
    {
        var (vc, issuerDid, holderDid, _, holderKey) = await IssueVcAsync(
            ReferenceInstant,
            Guid.Parse("bbbbbbbb-bbbb-4bbb-bbbb-bbbbbbbbbbbb"));

        var vcNode = JsonNode.Parse(vc)!;
        vcNode["proof"]!["proofValue"] = "0x" + new string('1', 130);
        var tamperedVc = vcNode.ToJsonString();
        var vpJson = WrapVp(tamperedVc, holderDid, issuerDid, holderKey);
        var outcome = PresentationVerifier.Verify(vpJson, ReferenceInstant);
        Assert.False(outcome.IsSuccess);
        Assert.True(outcome.ErrorCode is "vc_issuer_signature_mismatch" or "vc_signature_recover_failed");
    }

    [Fact]
    public async Task Verify_wrong_holder_signature_fails()
    {
        var (vc, issuerDid, holderDid, _, holderKey) = await IssueVcAsync(
            ReferenceInstant,
            Guid.Parse("cccccccc-cccc-4ccc-cccc-cccccccccccc"));

        var other = EthECKey.GenerateKey();
        var vpJson = WrapVp(vc, holderDid, issuerDid, other);
        var outcome = PresentationVerifier.Verify(vpJson, ReferenceInstant);
        Assert.False(outcome.IsSuccess);
        Assert.Equal("vp_holder_signature_mismatch", outcome.ErrorCode);
    }

    [Fact]
    public async Task Verify_holder_mismatch_subject_fails()
    {
        var (vc, issuerDid, holderDid, _, holderKey) = await IssueVcAsync(
            ReferenceInstant,
            Guid.Parse("dddddddd-dddd-4ddd-dddd-dddddddddddd"));

        var wrongHolder = "did:ethr:sepolia:0xffffffffffffffffffffffffffffffffffffffff";
        var vpJson = WrapVp(vc, wrongHolder, issuerDid, holderKey);
        var outcome = PresentationVerifier.Verify(vpJson, ReferenceInstant);
        Assert.False(outcome.IsSuccess);
        Assert.Equal("vp_holder_subject_mismatch", outcome.ErrorCode);
    }

    [Fact]
    public async Task Verify_two_embedded_credentials_fails()
    {
        var (vc, issuerDid, holderDid, _, holderKey) = await IssueVcAsync(
            ReferenceInstant,
            Guid.Parse("eeeeeeee-eeee-4eee-eeee-eeeeeeeeeeee"));

        var vcNode = JsonNode.Parse(vc)!;
        var vp = new JsonObject
        {
            ["type"] = new JsonArray("VerifiablePresentation"),
            ["holder"] = holderDid,
            ["verifiableCredential"] = new JsonArray(vcNode, vcNode.DeepClone()),
        };
        var credentialId = vcNode["id"]!.GetValue<string>();
        var vp712 = new VerifiablePresentation712
        {
            Holder = holderDid,
            VcCredentialId = credentialId,
            VcIssuer = issuerDid,
        };
        vp["proof"] = new JsonObject
        {
            ["type"] = VcSliceAEip712Constants.ProofType,
            ["proofPurpose"] = "authentication",
            ["verificationMethod"] = holderDid + "#controller",
            ["proofValue"] = VerifiablePresentationTypedData.Sign(vp712, holderKey),
        };

        var outcome = PresentationVerifier.Verify(vp.ToJsonString(), ReferenceInstant);
        Assert.False(outcome.IsSuccess);
        Assert.Equal("vp_verifiableCredential_count", outcome.ErrorCode);
    }

    [Fact]
    public async Task Verify_future_issuance_fails()
    {
        var future = ReferenceInstant.AddDays(1);
        var (vc, issuerDid, holderDid, _, holderKey) = await IssueVcAsync(
            future,
            Guid.Parse("11111111-2222-4333-8444-555555555555"));

        var vpJson = WrapVp(vc, holderDid, issuerDid, holderKey);
        var outcome = PresentationVerifier.Verify(vpJson, ReferenceInstant);
        Assert.False(outcome.IsSuccess);
        Assert.Equal("vc_issuanceDate_future", outcome.ErrorCode);
    }

    [Fact]
    public async Task Verify_expired_vc_fails()
    {
        var exp = ReferenceInstant.AddDays(-10);
        var (vc, issuerDid, holderDid, _, holderKey) = await IssueVcAsync(
            ReferenceInstant.AddDays(-30),
            Guid.Parse("22222222-3333-4333-8444-555555555555"),
            exp);

        var vpJson = WrapVp(vc, holderDid, issuerDid, holderKey);
        var outcome = PresentationVerifier.Verify(vpJson, ReferenceInstant);
        Assert.False(outcome.IsSuccess);
        Assert.Equal("vc_expirationDate_expired", outcome.ErrorCode);
    }

    private sealed class TestClock : IClock
    {
        private readonly DateTimeOffset instant;

        public TestClock(DateTimeOffset instant) => this.instant = instant;

        public Task<DateTimeOffset> GetUtcNowAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(instant);
        }
    }

    private sealed class TestGuid : IGuidGenerator
    {
        private readonly Guid value;

        public TestGuid(Guid value) => this.value = value;

        public Task<Guid> NewGuidAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(value);
        }
    }
}
