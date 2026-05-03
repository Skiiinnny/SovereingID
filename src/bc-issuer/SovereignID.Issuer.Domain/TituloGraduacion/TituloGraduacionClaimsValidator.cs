using System.Globalization;
using System.Text.RegularExpressions;

namespace SovereignID.Issuer.Domain.TituloGraduacion;

/// <summary>
/// Valida la allowlist de claims y el formato de <c>awardDate</c> (calendario, sin zona horaria).
/// </summary>
public static partial class TituloGraduacionClaimsValidator
{
    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}$", RegexOptions.CultureInvariant)]
    private static partial Regex AwardDatePattern();

    /// <summary>
    /// Devuelve código de error localizado o <see langword="null"/> si es válido.
    /// </summary>
    public static string? Validate(TituloGraduacionClaims claims)
    {
        ArgumentNullException.ThrowIfNull(claims);

        if (string.IsNullOrWhiteSpace(claims.DegreeTitle))
        {
            return "degreeTitle_required";
        }

        if (string.IsNullOrWhiteSpace(claims.ProgramName))
        {
            return "programName_required";
        }

        if (string.IsNullOrWhiteSpace(claims.AwardDate))
        {
            return "awardDate_required";
        }

        if (!AwardDatePattern().IsMatch(claims.AwardDate))
        {
            return "awardDate_format";
        }

        if (!DateOnly.TryParseExact(
                claims.AwardDate,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _))
        {
            return "awardDate_invalid_calendar";
        }

        return null;
    }
}
