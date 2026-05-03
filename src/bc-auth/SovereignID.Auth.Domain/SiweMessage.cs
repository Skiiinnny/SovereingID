using SovereignID.SharedKernel.Domain;

namespace SovereignID.Auth.Domain;

public sealed record SiweMessage(
    string Domain,
    EthereumAddress Address,
    string Statement,
    Uri Uri,
    int Version,
    int ChainId,
    Nonce Nonce,
    DateTimeOffset IssuedAt,
    DateTimeOffset? ExpirationTime,
    DateTimeOffset? NotBefore,
    string? RequestId,
    IReadOnlyList<Uri> Resources,
    string OriginalPayload);
