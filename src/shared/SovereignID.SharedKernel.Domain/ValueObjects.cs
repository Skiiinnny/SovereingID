namespace SovereignID.SharedKernel.Domain;

public sealed record DecentralizedIdentifier
{
    private DecentralizedIdentifier(string value) => Value = value;
    public string Value { get; }
    public static DecentralizedIdentifier Create(string value) => new(value);
}

public sealed record EthereumAddress
{
    private EthereumAddress(string value) => Value = value;
    public string Value { get; }
    public static EthereumAddress Create(string value) => new(value);
}

public sealed record Sha256Hash
{
    private Sha256Hash(string value) => Value = value;
    public string Value { get; }
    public static Sha256Hash Create(string value) => new(value);
}

public sealed record PublicKey
{
    private PublicKey(string value) => Value = value;
    public string Value { get; }
    public static PublicKey Create(string value) => new(value);
}

public sealed record Signature
{
    private Signature(string value) => Value = value;
    public string Value { get; }
    public static Signature Create(string value) => new(value);
}
