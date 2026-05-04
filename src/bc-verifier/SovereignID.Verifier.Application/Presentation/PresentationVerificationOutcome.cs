namespace SovereignID.Verifier.Application.Presentation;

/// <summary>
/// Resultado de la verificación de una VP mínima con VC TituloGraduacion embebido.
/// </summary>
public sealed record PresentationVerificationOutcome(bool IsSuccess, string? ErrorCode);
