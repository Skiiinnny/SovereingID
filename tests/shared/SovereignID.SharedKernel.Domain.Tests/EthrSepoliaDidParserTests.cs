namespace SovereignID.SharedKernel.Domain.Tests;

public class EthrSepoliaDidParserTests
{
    [Fact]
    public void TryParse_accepts_lowercase_sepolia_ethr_did()
    {
        const string did = "did:ethr:sepolia:0xabcdef0123456789abcdef0123456789abcdef01";

        var ok = EthrSepoliaDidParser.TryParse(did, out var parsedDid, out var addr);

        Assert.True(ok);
        Assert.Equal(did, parsedDid!.Value);
        Assert.Equal("0xabcdef0123456789abcdef0123456789abcdef01", addr!.Value);
    }

    [Theory]
    [InlineData("did:ethr:mainnet:0xabcdef0123456789abcdef0123456789abcdef01")]
    [InlineData("did:ethr:sepolia:0xABCDEF0123456789ABCDEF0123456789ABCDEF01")]
    [InlineData("did:ethr:sepolia:0xabc")]
    [InlineData("")]
    public void TryParse_rejects_invalid_dids(string did)
    {
        Assert.False(EthrSepoliaDidParser.TryParse(did, out _, out _));
    }

    [Fact]
    public void Parse_returns_did_and_address_for_valid_did()
    {
        const string did = "did:ethr:sepolia:0xabcdef0123456789abcdef0123456789abcdef01";
        var (parsedDid, addr) = EthrSepoliaDidParser.Parse(did);
        Assert.Equal(did, parsedDid.Value);
        Assert.Equal("0xabcdef0123456789abcdef0123456789abcdef01", addr.Value);
    }

    [Fact]
    public void Parse_throws_FormatException_on_invalid_did()
    {
        var ex = Assert.Throws<FormatException>(() => EthrSepoliaDidParser.Parse("did:ethr:sepolia:0xBAD"));
        Assert.Contains("did:ethr:sepolia:0x", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CredentialType_TituloGraduacion_exists()
    {
        _ = CredentialType.TituloGraduacion;
        Assert.Equal(0, (int)CredentialType.TituloGraduacion);
    }
}
