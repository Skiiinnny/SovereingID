namespace SovereignID.Auth.Domain;

/// <summary>
/// Provides typed auth domain errors.
/// </summary>
public static class AuthErrors
{
    public static AuthError NonceUnknown(string nonce) => new("nonce_unknown", $"Nonce '{nonce}' was not found.");
    public static AuthError NonceConsumed(string nonce) => new("nonce_consumed", $"Nonce '{nonce}' was already consumed.");
    public static AuthError NonceExpired(DateTimeOffset now, DateTimeOffset expiresAt) => new("nonce_expired", $"Nonce expired at '{expiresAt:O}' (now '{now:O}').");
    public static AuthError UnsupportedChain(int chainId) => new("unsupported_chain", $"Unsupported chain id '{chainId}'. Expected 11155111.");
    public static AuthError SignatureMismatch(string claimedAddress, string recoveredAddress) => new("signature_mismatch", $"Claimed '{claimedAddress}' does not match recovered '{recoveredAddress}'.");
    public static AuthError SiweParseFailed(string detail) => new("siwe_parse_failed", detail);
}
