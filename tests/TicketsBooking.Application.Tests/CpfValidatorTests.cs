using TicketsBooking.Core.Validators;

namespace TicketsBooking.Application.Tests;

public class CpfValidatorTests
{
    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    public void IsValid_ValidCpf_ReturnsTrue(string cpf)
    {
        Assert.True(CpfValidator.IsValid(cpf));
    }

    [Theory]
    [InlineData("111.111.111-11")]
    [InlineData("000.000.000-00")]
    [InlineData("529.982.247-26")]
    [InlineData("12345")]
    [InlineData("")]
    [InlineData(null)]
    public void IsValid_InvalidCpf_ReturnsFalse(string? cpf)
    {
        Assert.False(CpfValidator.IsValid(cpf!));
    }
}
