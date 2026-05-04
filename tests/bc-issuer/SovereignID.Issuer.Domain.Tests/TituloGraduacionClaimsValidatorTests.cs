using SovereignID.Issuer.Domain.TituloGraduacion;

namespace SovereignID.Issuer.Domain.Tests;

public class TituloGraduacionClaimsValidatorTests
{
    [Fact]
    public void Validate_accepts_valid_award_date()
    {
        var c = new TituloGraduacionClaims("Título", "Programa", "2024-02-29");
        Assert.Null(TituloGraduacionClaimsValidator.Validate(c));
    }

    [Theory]
    [InlineData("2025-13-01")]
    [InlineData("25-01-01")]
    [InlineData("2025-1-1")]
    public void Validate_rejects_bad_award_date(string awardDate)
    {
        var c = new TituloGraduacionClaims("T", "P", awardDate);
        Assert.NotNull(TituloGraduacionClaimsValidator.Validate(c));
    }
}
