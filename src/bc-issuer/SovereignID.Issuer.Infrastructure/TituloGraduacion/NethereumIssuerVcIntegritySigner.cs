using Nethereum.Signer;
using SovereignID.Issuer.Domain.TituloGraduacion;
using SovereignID.VerifiableCredential.Eip712;

namespace SovereignID.Issuer.Infrastructure.TituloGraduacion;

/// <summary>
/// Firma el digest EIP-712 del VC con una clave secp256k1 del emisor (Nethereum).
/// </summary>
public sealed class NethereumIssuerVcIntegritySigner : IIssuerVcIntegritySigner
{
    private readonly EthECKey issuerKey;

    /// <summary>
    /// Crea el firmante con la clave del emisor (nunca loguear la clave privada).
    /// </summary>
    public NethereumIssuerVcIntegritySigner(EthECKey issuerKey)
    {
        this.issuerKey = issuerKey ?? throw new ArgumentNullException(nameof(issuerKey));
    }

    /// <inheritdoc />
    public Task<string> SignAsync(IssuerVcIntegritySignRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var message = new TituloGraduacionCredential712
        {
            CredentialId = request.CredentialId,
            Issuer = request.IssuerDid,
            CredentialSubjectId = request.CredentialSubjectId,
            DegreeTitle = request.DegreeTitle,
            ProgramName = request.ProgramName,
            AwardDate = request.AwardDate,
            IssuanceDate = request.IssuanceDate,
            ExpirationDate = request.ExpirationDateOrEmpty,
        };

        var sig = TituloGraduacionVcTypedData.Sign(message, issuerKey);
        return Task.FromResult(sig);
    }
}
