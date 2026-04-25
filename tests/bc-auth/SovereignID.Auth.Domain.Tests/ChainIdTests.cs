using SovereignID.Auth.Domain;

namespace SovereignID.Auth.Domain.Tests;

public class ChainIdTests
{
    [Fact]
    public void Create_WithSepoliaValue_ReturnsSepolia()
    {
        var chainId = ChainId.Create(11155111);
        Assert.Equal(ChainId.Sepolia, chainId);
    }

    [Fact]
    public void Create_WithUnsupportedValue_ThrowsDomainException()
    {
        var ex = Assert.Throws<AuthDomainException>(() => ChainId.Create(1));
        Assert.Equal("unsupported_chain", ex.Error.Code);
    }
}
