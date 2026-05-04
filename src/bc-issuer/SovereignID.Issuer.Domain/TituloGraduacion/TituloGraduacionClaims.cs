using SovereignID.SharedKernel.Domain;

namespace SovereignID.Issuer.Domain.TituloGraduacion;

/// <summary>
/// Claims permitidos en <c>credentialSubject</c> para <see cref="CredentialType.TituloGraduacion"/>.
/// </summary>
public sealed record TituloGraduacionClaims(string DegreeTitle, string ProgramName, string AwardDate);
