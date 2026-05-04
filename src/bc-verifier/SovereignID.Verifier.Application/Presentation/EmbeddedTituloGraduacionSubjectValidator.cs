using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SovereignID.Verifier.Application.Presentation;

/// <summary>
/// Valida la allowlist de <c>credentialSubject</c> para el perfil TituloGraduacion (sin dependencia del BC emisor).
/// </summary>
internal static partial class EmbeddedTituloGraduacionSubjectValidator
{
    private static readonly string[] AllowedKeys = ["id", "degreeTitle", "programName", "awardDate"];

    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}$", RegexOptions.CultureInvariant)]
    private static partial Regex AwardDatePattern();

    internal static string? ValidateSubject(JsonElement subject)
    {
        if (subject.ValueKind != JsonValueKind.Object)
        {
            return "vc_subject_not_object";
        }

        foreach (var prop in subject.EnumerateObject())
        {
            if (Array.IndexOf(AllowedKeys, prop.Name) < 0)
            {
                return "vc_subject_extra_property";
            }
        }

        if (!subject.TryGetProperty("id", out var idEl) || idEl.ValueKind != JsonValueKind.String)
        {
            return "vc_subject_id_missing";
        }

        foreach (var key in new[] { "degreeTitle", "programName", "awardDate" })
        {
            if (!subject.TryGetProperty(key, out var p) || p.ValueKind != JsonValueKind.String)
            {
                return "vc_subject_claim_missing_" + key;
            }

            var s = p.GetString();
            if (string.IsNullOrWhiteSpace(s))
            {
                return "vc_subject_claim_empty_" + key;
            }

            if (key == "awardDate")
            {
                if (!AwardDatePattern().IsMatch(s))
                {
                    return "vc_subject_awardDate_format";
                }

                if (!DateOnly.TryParseExact(
                        s,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out _))
                {
                    return "vc_subject_awardDate_invalid";
                }
            }
        }

        return null;
    }
}
