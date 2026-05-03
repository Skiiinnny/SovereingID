namespace SovereignID.Issuer.Domain.TituloGraduacion;

/// <summary>
/// Firma la integridad del VC TituloGraduacion (EIP-712 v4) con la clave del emisor.
/// </summary>
public interface IIssuerVcIntegritySigner
{
    /// <summary>
    /// Devuelve la firma en formato hex estándar (0x + r+s+v).
    /// </summary>
    Task<string> SignAsync(IssuerVcIntegritySignRequest request, CancellationToken cancellationToken);
}
