namespace SovereignID.Crypto.Tests;

public class MessageSignerTests
{
    private readonly MessageSigner _signer = new();

    [Fact]
    public void Sign_SameMessageTwice_ProducesSameSignature()
    {
        var kp = KeyPair.Generate();
        const string message = "Hello SovereignID";
        var sig1 = _signer.Sign(message, kp.PrivateKey);
        var sig2 = _signer.Sign(message, kp.PrivateKey);
        Assert.Equal(sig1, sig2);
    }
}
