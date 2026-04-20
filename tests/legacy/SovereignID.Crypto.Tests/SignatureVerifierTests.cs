using SovereignID.Crypto;

namespace SovereignID.Crypto.Tests;

public class SignatureVerifierTests
{
    private readonly MessageSigner _signer = new();
    private readonly SignatureVerifier _verifier = new();

    [Fact]
    public void RecoverSigner_MatchesSigningAddress()
    {
        var kp = KeyPair.Generate();
        const string message = "Hello SovereignID";
        var sig = _signer.Sign(message, kp.PrivateKey);
        var recovered = _verifier.RecoverSigner(message, sig);
        Assert.Equal(kp.Address, recovered, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void Verify_TamperedMessage_ReturnsFalse()
    {
        var kp = KeyPair.Generate();
        const string original = "Hello SovereignID";
        var sig = _signer.Sign(original, kp.PrivateKey);
        var tampered = original + "!";
        Assert.False(_verifier.Verify(tampered, sig, kp.Address));
    }

    [Fact]
    public void Verify_WrongAddress_ReturnsFalse()
    {
        var kp = KeyPair.Generate();
        var other = KeyPair.Generate();
        const string message = "Hello SovereignID";
        var sig = _signer.Sign(message, kp.PrivateKey);
        Assert.False(_verifier.Verify(message, sig, other.Address));
    }
}
