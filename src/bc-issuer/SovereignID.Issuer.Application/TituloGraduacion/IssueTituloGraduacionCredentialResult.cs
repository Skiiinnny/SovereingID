namespace SovereignID.Issuer.Application.TituloGraduacion;

/// <summary>
/// Resultado de la emisión lógica del VC TituloGraduacion (JSON-LD final con prueba).
/// </summary>
public sealed record IssueTituloGraduacionCredentialResult(bool IsSuccess, string? CredentialJson, string? ErrorCode);
