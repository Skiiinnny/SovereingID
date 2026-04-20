namespace SovereignID.SharedKernel.Domain;

/// <summary>
/// Signs payloads asynchronously.
/// </summary>
public interface ISigner
{
    Task<Signature> SignAsync(string payload, CancellationToken cancellationToken);
}

/// <summary>
/// Verifies signatures asynchronously.
/// </summary>
public interface ISignatureVerifier
{
    Task<bool> VerifyAsync(string payload, Signature signature, PublicKey publicKey, CancellationToken cancellationToken);
}

/// <summary>
/// Hashes payloads asynchronously.
/// </summary>
public interface IHasher
{
    Task<Sha256Hash> ComputeSha256Async(string payload, CancellationToken cancellationToken);
}

/// <summary>
/// Anchors documents into a blockchain.
/// </summary>
public interface IBlockchainAnchor
{
    Task<string> AnchorAsync(Sha256Hash hash, CancellationToken cancellationToken);
}

/// <summary>
/// Queries blockchain state.
/// </summary>
public interface IBlockchainQuery
{
    Task<bool> ExistsAsync(string transactionId, CancellationToken cancellationToken);
}

/// <summary>
/// Provides current UTC time.
/// </summary>
public interface IClock
{
    Task<DateTimeOffset> GetUtcNowAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Generates GUID values.
/// </summary>
public interface IGuidGenerator
{
    Task<Guid> NewGuidAsync(CancellationToken cancellationToken);
}
