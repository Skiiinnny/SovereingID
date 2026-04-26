using System.Diagnostics.CodeAnalysis;

namespace SovereignID.Auth.Api.Contracts;

/// <summary>
/// Response returned by the nonce issuance endpoint.
/// </summary>
/// <param name="Nonce">Single-use SIWE nonce value.</param>
/// <param name="ExpiresAt">Nonce expiration timestamp in UTC.</param>
[ExcludeFromCodeCoverage]
public sealed record NonceResponse(string Nonce, DateTimeOffset ExpiresAt);
