using SovereignID.SharedKernel.Application;

namespace SovereignID.Issuer.Application.TituloGraduacion;

/// <summary>
/// Orquesta la validación, construcción JSON-LD y firma EIP-712 del emisor.
/// </summary>
public sealed record IssueTituloGraduacionCredentialCommand(
    string IssuerDid,
    string SubjectDid,
    string DegreeTitle,
    string ProgramName,
    string AwardDate,
    DateTimeOffset? ExpirationUtc) : ICommand<IssueTituloGraduacionCredentialResult>;
