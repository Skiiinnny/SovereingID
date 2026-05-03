using SovereignID.SharedKernel.Application;

namespace SovereignID.Verifier.Application.Presentation;

/// <summary>
/// Verifica una VP mínima con VC TituloGraduacion embebido (sin red).
/// </summary>
public sealed record VerifyPresentationCommand(string PresentationJson)
    : ICommand<PresentationVerificationOutcome>;
