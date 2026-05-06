using System.Globalization;
using System.Text.Json;
using SovereignID.Issuer.Domain.TituloGraduacion;
using SovereignID.SharedKernel.Application;
using SovereignID.SharedKernel.Domain;
using SovereignID.VerifiableCredential.Document;

namespace SovereignID.Issuer.Application.TituloGraduacion;

/// <summary>
/// Valida entrada, construye el VC y delega la firma EIP-712 en <see cref="IIssuerVcIntegritySigner"/>.
/// </summary>
public sealed class IssueTituloGraduacionCredentialCommandHandler(
    IIssuerVcIntegritySigner signer,
    IClock clock,
    IGuidGenerator guidGenerator)
    : ICommandHandler<IssueTituloGraduacionCredentialCommand, IssueTituloGraduacionCredentialResult>
{
    /// <inheritdoc />
    public async Task<IssueTituloGraduacionCredentialResult> HandleAsync(
        IssueTituloGraduacionCredentialCommand input,
        CancellationToken cancellationToken)
    {
        if (!EthrSepoliaDidParser.TryParse(input.IssuerDid, out _, out _))
        {
            return new IssueTituloGraduacionCredentialResult(false, null, "issuer_did_invalid");
        }

        if (!EthrSepoliaDidParser.TryParse(input.SubjectDid, out _, out _))
        {
            return new IssueTituloGraduacionCredentialResult(false, null, "subject_did_invalid");
        }

        var claims = new TituloGraduacionClaims(input.DegreeTitle, input.ProgramName, input.AwardDate);

        var now = await clock.GetUtcNowAsync(cancellationToken).ConfigureAwait(false);
        var issuance = now.UtcDateTime;
        var issuanceString = issuance.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);

        string? expirationString = null;
        if (input.ExpirationUtc is { } exp)
        {
            expirationString = exp.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
        }

        var id = await guidGenerator.NewGuidAsync(cancellationToken).ConfigureAwait(false);
        var credentialId = "urn:uuid:" + id.ToString("d", CultureInfo.InvariantCulture);

        var unsigned = VerifiableCredentialJsonBuilder.BuildUnsignedCredential(
            credentialId,
            input.IssuerDid,
            input.SubjectDid,
            claims,
            issuanceString,
            expirationString);

        using (var vcDoc = JsonDocument.Parse(VerifiableCredentialJsonBuilder.Serialize(unsigned)))
        {
            var shapeError = TituloGraduacionVcDocumentValidator.Validate(
                vcDoc.RootElement,
                now,
                CredentialValidationMode.Unsigned);
            if (shapeError is not null)
            {
                return new IssueTituloGraduacionCredentialResult(false, null, IssuerTituloGraduacionDocumentErrorMapper.Map(shapeError));
            }
        }

        var signRequest = new IssuerVcIntegritySignRequest(
            credentialId,
            input.IssuerDid,
            input.SubjectDid,
            claims.DegreeTitle,
            claims.ProgramName,
            claims.AwardDate,
            issuanceString,
            expirationString ?? string.Empty);

        var signature = await signer.SignAsync(signRequest, cancellationToken).ConfigureAwait(false);
        VerifiableCredentialJsonBuilder.AttachIssuerProof(unsigned, input.IssuerDid, signature);

        var json = VerifiableCredentialJsonBuilder.Serialize(unsigned);
        return new IssueTituloGraduacionCredentialResult(true, json, null);
    }
}
