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
            var issuedAt = DateTimeOffset.Parse(ParsePrefixed(lines[9], "Issued At: ", "Line 10"));

            DateTimeOffset? expirationTime = null;
            DateTimeOffset? notBefore = null;
            string? requestId = null;
            var resources = new List<Uri>();

            for (var i = 10; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.StartsWith("Expiration Time: ", StringComparison.Ordinal))
                {
                    expirationTime = DateTimeOffset.Parse(ParsePrefixed(line, "Expiration Time: ", $"Line {i + 1}"));
                    continue;
                }

                if (line.StartsWith("Not Before: ", StringComparison.Ordinal))
                {
                    notBefore = DateTimeOffset.Parse(ParsePrefixed(line, "Not Before: ", $"Line {i + 1}"));
                    continue;
                }

                if (line.StartsWith("Request ID: ", StringComparison.Ordinal))
                {
                    requestId = ParsePrefixed(line, "Request ID: ", $"Line {i + 1}");
                    continue;
                }

                if (line == "Resources:")
                {
                    i++;
                    while (i < lines.Length && lines[i].StartsWith("- ", StringComparison.Ordinal))
                    {
                        resources.Add(new Uri(lines[i][2..], UriKind.Absolute));
                        i++;
                    }

                    i--;
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
