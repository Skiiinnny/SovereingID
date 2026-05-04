using System.Text.Json;
using Nethereum.Signer;
using SovereignID.Issuer.Application.TituloGraduacion;
using SovereignID.Issuer.Domain.TituloGraduacion;
using SovereignID.Issuer.Infrastructure.TituloGraduacion;
using SovereignID.SharedKernel.Domain;

namespace SovereignID.Issuer.Application.Tests;

public class IssueTituloGraduacionCredentialCommandHandlerTests
{
    private static string Did(EthECKey key)
    {
        var a = key.GetPublicAddress();
        var hex = (a.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? a[2..] : a).ToLowerInvariant();
        return "did:ethr:sepolia:0x" + hex;
    }

    [Fact]
    public async Task HandleAsync_invalid_subject_did_returns_error()
    {
        var h = new IssueTituloGraduacionCredentialCommandHandler(
            new NethereumIssuerVcIntegritySigner(EthECKey.GenerateKey()),
            new TestClock(DateTimeOffset.UtcNow),
            new TestGuid(Guid.NewGuid()));

        var r = await h.HandleAsync(
            new IssueTituloGraduacionCredentialCommand(
                "did:ethr:sepolia:0x1111111111111111111111111111111111111111",
                "did:example:bad",
                "G",
                "P",
                "2025-01-01",
                null),
            CancellationToken.None);

        Assert.False(r.IsSuccess);
        Assert.Equal("subject_did_invalid", r.ErrorCode);
    }

    [Fact]
    public async Task HandleAsync_invalid_issuer_did_returns_error()
    {
        var h = new IssueTituloGraduacionCredentialCommandHandler(
            new NethereumIssuerVcIntegritySigner(EthECKey.GenerateKey()),
            new TestClock(DateTimeOffset.UtcNow),
            new TestGuid(Guid.NewGuid()));

        var r = await h.HandleAsync(
            new IssueTituloGraduacionCredentialCommand(
                "did:example:bad",
                "did:ethr:sepolia:0x2222222222222222222222222222222222222222",
                "G",
                "P",
                "2025-01-01",
                null),
            CancellationToken.None);

        Assert.False(r.IsSuccess);
        Assert.Equal("issuer_did_invalid", r.ErrorCode);
    }

    [Fact]
    public async Task HandleAsync_with_fake_signer_produces_structural_proof()
    {
        var issuer = EthECKey.GenerateKey();
        var issuerDid = Did(issuer);
        var subjectDid = "did:ethr:sepolia:0x3333333333333333333333333333333333333333";

        var h = new IssueTituloGraduacionCredentialCommandHandler(
            new FakeIssuerSigner(),
            new TestClock(DateTimeOffset.Parse("2026-05-03T12:00:00Z", System.Globalization.CultureInfo.InvariantCulture)),
            new TestGuid(Guid.Parse("aaaaaaaa-bbbb-4ccc-dddd-eeeeeeeeeeee")));

        var r = await h.HandleAsync(
            new IssueTituloGraduacionCredentialCommand(
                issuerDid,
                subjectDid,
                "Grado",
                "Programa",
                "2025-06-01",
                null),
            CancellationToken.None);

        Assert.True(r.IsSuccess);
        using var doc = JsonDocument.Parse(r.CredentialJson!);
        var proof = doc.RootElement.GetProperty("proof");
        Assert.Equal(VerifiableCredentialJsonBuilder.ProofType, proof.GetProperty("type").GetString());
        Assert.False(string.IsNullOrEmpty(proof.GetProperty("proofValue").GetString()));
    }

    private sealed class FakeIssuerSigner : IIssuerVcIntegritySigner
    {
        public Task<string> SignAsync(IssuerVcIntegritySignRequest request, CancellationToken cancellationToken)
        {
            _ = request;
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult("0x" + new string('b', 130));
        }
    }

    private sealed class TestClock(DateTimeOffset instant) : IClock
    {
        public Task<DateTimeOffset> GetUtcNowAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(instant);
        }
    }

    private sealed class TestGuid(Guid value) : IGuidGenerator
    {
        public Task<Guid> NewGuidAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(value);
        }
    }
}
