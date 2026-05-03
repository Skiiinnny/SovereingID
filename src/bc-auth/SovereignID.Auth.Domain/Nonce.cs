using System.Text.RegularExpressions;

namespace SovereignID.Auth.Domain;

public sealed partial record Nonce
{
    [GeneratedRegex("^[0-9a-f]{32}$", RegexOptions.CultureInvariant)]
    private static partial Regex Hex32Regex();

    private Nonce(string value) => Value = value;

    public string Value { get; }

    public static Nonce Create(string value)
    {
        if (!Hex32Regex().IsMatch(value))
        {
            throw new ArgumentException("Nonce must be exactly 32 lowercase hex characters.", nameof(value));
        }

        return new Nonce(value);
    }

    public override string ToString() => Value;
}
