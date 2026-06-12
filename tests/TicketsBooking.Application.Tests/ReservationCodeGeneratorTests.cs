using System.Text.RegularExpressions;
using TicketsBooking.Core.Generators;

namespace TicketsBooking.Application.Tests;

public class ReservationCodeGeneratorTests
{
    private static readonly Regex ExpectedFormat = new(@"^[A-Z]{3}-\d{5}$");

    [Fact]
    public void Generate_SingleCode_MatchesExpectedFormat()
    {
        var code = ReservationCodeGenerator.Generate();
        Assert.Matches(ExpectedFormat, code);
    }

    [Fact]
    public void Generate_AllCodesInBatch_MatchExpectedFormat()
    {
        var codes = Enumerable.Range(0, 50)
            .Select(_ => ReservationCodeGenerator.Generate());

        foreach (var code in codes)
            Assert.Matches(ExpectedFormat, code);
    }

    [Fact]
    public void Generate_LargeBatch_ProducesHighUniqueness()
    {
        var codes = Enumerable.Range(0, 100)
            .Select(_ => ReservationCodeGenerator.Generate())
            .ToHashSet();

        // 26^3 * 90000 combinações possíveis; colisões em 100 chamadas devem ser raras
        Assert.True(codes.Count >= 95,
            $"Esperado >= 95 códigos únicos em 100 chamadas, obtido: {codes.Count}");
    }
}
