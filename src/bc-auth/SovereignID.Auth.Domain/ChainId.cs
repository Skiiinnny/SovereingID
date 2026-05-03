namespace SovereignID.Auth.Domain;

public sealed record ChainId
{
    public const int SepoliaValue = 11155111;

    private ChainId(int value) => Value = value;

    public int Value { get; }

    public static ChainId Sepolia { get; } = new(SepoliaValue);

    public static ChainId Create(int value)
    {
        if (value != SepoliaValue)
        {
            throw new AuthDomainException(AuthErrors.UnsupportedChain(value));
        }

        return Sepolia;
    }

    public override string ToString() => Value.ToString();
}
