using System.Globalization;
using SovereignID.Auth.Domain;
using SovereignID.Auth.Domain.Ports;
using SovereignID.SharedKernel.Domain;

namespace SovereignID.Auth.Infrastructure.Siwe;

public sealed class ManualSiweMessageParser : ISiweMessageParser
{
    public Task<SiweMessage> ParseAsync(string payload, CancellationToken cancellationToken)
    {
        try
        {
            var lines = payload.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
            Ensure(lines.Length >= 10, "Payload must contain at least 10 lines.");

            var domainSuffix = " wants you to sign in with your Ethereum account:";
            Ensure(lines[0].EndsWith(domainSuffix, StringComparison.Ordinal), "Line 1 must contain SIWE domain preamble.");
            var domain = lines[0][..^domainSuffix.Length];
            Ensure(!string.IsNullOrWhiteSpace(domain), "Line 1 domain is required.");

            var addressLine = lines[1].Trim();
            Ensure(IsHexAddress(addressLine), "Line 2 must be a valid Ethereum address.");
            Ensure(lines[2] == string.Empty, "Line 3 must be blank.");

            var statement = lines[3];
            Ensure(lines[4] == string.Empty, "Line 5 must be blank.");

            var uri = ParsePrefixed(lines[5], "URI: ", "Line 6");
            var versionText = ParsePrefixed(lines[6], "Version: ", "Line 7");
            Ensure(versionText == "1", "Line 7 must be 'Version: 1'.");
            var chainId = int.Parse(ParsePrefixed(lines[7], "Chain ID: ", "Line 8"));
            var nonce = Nonce.Create(ParsePrefixed(lines[8], "Nonce: ", "Line 9"));
            var issuedAt = DateTimeOffset.Parse(
                ParsePrefixed(lines[9], "Issued At: ", "Line 10"),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);

            DateTimeOffset? expirationTime = null;
            DateTimeOffset? notBefore = null;
            string? requestId = null;
            var resources = new List<Uri>();

            var skipUntilIndex = -1;
            for (var i = 10; i < lines.Length; i++)
            {
                if (i <= skipUntilIndex)
                {
                    continue;
                }

                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (TryParseOptionalLine(
                        line,
                        lines,
                        i,
                        out var nextIndex,
                        ref expirationTime,
                        ref notBefore,
                        ref requestId,
                        resources))
                {
                    skipUntilIndex = nextIndex;
                    continue;
                }

                throw new FormatException($"Line {i + 1}: unsupported SIWE line '{line}'.");
            }

            return Task.FromResult(new SiweMessage(
                Domain: domain,
                Address: EthereumAddress.Create(addressLine),
                Statement: statement,
                Uri: new Uri(uri, UriKind.Absolute),
                Version: 1,
                ChainId: chainId,
                Nonce: nonce,
                IssuedAt: issuedAt,
                ExpirationTime: expirationTime,
                NotBefore: notBefore,
                RequestId: requestId,
                Resources: resources,
                OriginalPayload: payload));
        }
        catch (Exception ex) when (ex is not AuthDomainException)
        {
            throw new AuthDomainException(AuthErrors.SiweParseFailed(ex.Message));
        }
    }

    private static bool TryParseOptionalLine(
        string line,
        string[] lines,
        int index,
        out int nextIndex,
        ref DateTimeOffset? expirationTime,
        ref DateTimeOffset? notBefore,
        ref string? requestId,
        List<Uri> resources)
    {
        nextIndex = index;
        var location = $"Line {index + 1}";

        if (line.StartsWith("Expiration Time: ", StringComparison.Ordinal))
        {
            expirationTime = DateTimeOffset.Parse(
                ParsePrefixed(line, "Expiration Time: ", location),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);
            return true;
        }

        if (line.StartsWith("Not Before: ", StringComparison.Ordinal))
        {
            notBefore = DateTimeOffset.Parse(
                ParsePrefixed(line, "Not Before: ", location),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);
            return true;
        }

        if (line.StartsWith("Request ID: ", StringComparison.Ordinal))
        {
            requestId = ParsePrefixed(line, "Request ID: ", location);
            return true;
        }

        if (line != "Resources:")
        {
            return false;
        }

        var resourceIndex = index + 1;
        while (resourceIndex < lines.Length && lines[resourceIndex].StartsWith("- ", StringComparison.Ordinal))
        {
            resources.Add(new Uri(lines[resourceIndex][2..], UriKind.Absolute));
            resourceIndex++;
        }

        nextIndex = resourceIndex - 1;
        return true;
    }

    private static string ParsePrefixed(string line, string prefix, string location)
    {
        if (!line.StartsWith(prefix, StringComparison.Ordinal))
        {
            throw new FormatException($"{location}: expected '{prefix.Trim()}'.");
        }

        var value = line[prefix.Length..];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new FormatException($"{location}: value is required.");
        }

        return value;
    }

    private static void Ensure(bool condition, string message)
    {
        if (!condition)
        {
            throw new FormatException(message);
        }
    }

    private static bool IsHexAddress(string address)
    {
        if (!address.StartsWith("0x", StringComparison.Ordinal) || address.Length != 42)
        {
            return false;
        }

        for (var i = 2; i < address.Length; i++)
        {
            if (!Uri.IsHexDigit(address[i]))
            {
                return false;
            }
        }

        return true;
    }
}
