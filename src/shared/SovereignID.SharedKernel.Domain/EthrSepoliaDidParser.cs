using System.Diagnostics.CodeAnalysis;

namespace SovereignID.SharedKernel.Domain;

/// <summary>
/// Parsea DIDs <c>did:ethr:sepolia:0x</c> con exactamente 40 caracteres hexadecimales en minúsculas.
/// </summary>
public static class EthrSepoliaDidParser
{
    public const string Prefix = "did:ethr:sepolia:0x";

    /// <summary>
    /// Intenta interpretar <paramref name="did"/> como DID ethr en Sepolia y extraer la dirección Ethereum.
    /// </summary>
    /// <param name="did">Cadena DID sin espacios ni mayúsculas en la parte hex.</param>
    /// <param name="decentralizedIdentifier">DID canónico (misma forma que la entrada válida).</param>
    /// <param name="ethereumAddress">Dirección <c>0x</c> + 40 hex minúsculas.</param>
    /// <returns><see langword="true"/> si el formato es válido.</returns>
    public static bool TryParse(
        string did,
        [NotNullWhen(true)] out DecentralizedIdentifier? decentralizedIdentifier,
        [NotNullWhen(true)] out EthereumAddress? ethereumAddress)
    {
        decentralizedIdentifier = null;
        ethereumAddress = null;

        if (string.IsNullOrEmpty(did) || !did.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return false;
        }

        var hex = did.AsSpan(Prefix.Length);
        if (hex.Length != 40)
        {
            return false;
        }

        foreach (var c in hex)
        {
            if (!Uri.IsHexDigit(c) || char.IsUpper(c))
            {
                return false;
            }
        }

        var addr = "0x" + did.Substring(Prefix.Length);
        decentralizedIdentifier = DecentralizedIdentifier.Create(did);
        ethereumAddress = EthereumAddress.Create(addr);
        return true;
    }

    /// <summary>
    /// Equivalente a <see cref="TryParse"/> pero lanza <see cref="FormatException"/> si el formato no es válido.
    /// </summary>
    public static (DecentralizedIdentifier Did, EthereumAddress Address) Parse(string did)
    {
        if (!TryParse(did, out var d, out var a))
        {
            throw new FormatException(
                "Se esperaba did:ethr:sepolia:0x seguido de 40 dígitos hexadecimales en minúsculas.");
        }

        return (d, a);
    }
}
