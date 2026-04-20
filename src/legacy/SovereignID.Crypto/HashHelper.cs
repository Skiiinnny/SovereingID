using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace SovereignID.Crypto;

/// <summary>SHA-256 helpers for content anchoring (32-byte <c>bytes32</c> semantics).</summary>
public static class HashHelper
{
    /// <summary>SHA-256 digest of UTF-8 <paramref name="content"/> as lowercase 0x + 64 hex (EVM <c>bytes32</c>).</summary>
    public static string Sha256(string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        var digest = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return "0x" + Convert.ToHexString(digest).ToLowerInvariant();
    }

    /// <summary>SHA-256 digest as a 32-byte big-endian array suitable for ABI encoding.</summary>
    public static byte[] Sha256Bytes(string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        return SHA256.HashData(Encoding.UTF8.GetBytes(content));
    }

    /// <summary>Parses a 0x-prefixed 32-byte hex string into raw bytes.</summary>
    public static byte[] ParseBytes32(string hexBytes32)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hexBytes32);
        var hex = hexBytes32.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? hexBytes32[2..]
            : hexBytes32;
        if (hex.Length != 64)
        {
            throw new FormatException("Expected 64 hex characters for bytes32.");
        }

        var bytes = new byte[32];
        for (var i = 0; i < 32; i++)
        {
            bytes[i] = byte.Parse(hex.AsSpan(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        return bytes;
    }
}
