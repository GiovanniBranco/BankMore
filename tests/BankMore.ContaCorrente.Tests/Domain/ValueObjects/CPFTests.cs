using BankMore.ContaCorrente.API.Domain.Exceptions;
using BankMore.ContaCorrente.API.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace BankMore.ContaCorrente.Tests.Domain.ValueObjects;

public class CPFTests
{
    [Theory]
    [InlineData("12345678909")]
    [InlineData("123.456.789-09")]
    public void CPF_ComNumeroValido_DeveCriarInstancia(string cpfValido)
    {
        var cpf = new CPF(cpfValido);

        cpf.Should().NotBeNull();
        cpf.Valor.Should().MatchRegex(@"^\d{11}$");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    [InlineData("12345678901")]
    [InlineData("00000000000")]
    [InlineData("11111111111")]
    [InlineData("99999999999")]
    public void CPF_ComNumeroInvalido_DeveLancarExcecao(string cpfInvalido)
    {
        Action act = () => new CPF(cpfInvalido);

        act.Should().Throw<InvalidDocumentException>()
            .WithMessage("CPF inválido");
    }

    [Fact]
    public void CPF_ComNull_DeveLancarExcecao()
    {
        Action act = () => new CPF(null!);

        act.Should().Throw<InvalidDocumentException>();
    }

    [Fact]
    public void CPF_DeveRemoverCaracteresEspeciais()
    {
        var cpf = new CPF("123.456.789-09");

        cpf.Valor.Should().Be("12345678909");
    }

    [Theory]
    [InlineData("12345678909", true)]
    [InlineData("123.456.789-09", true)]
    [InlineData("00000000000", false)]
    [InlineData("11111111111", false)]
    [InlineData("123", false)]
    [InlineData("", false)]
    public void Validar_DeveRetornarResultadoCorreto(string cpf, bool esperado)
    {
        var resultado = CPF.Validar(cpf);

        resultado.Should().Be(esperado);
    }

    [Fact]
    public void Criar_ComCPFValido_DeveCriarInstancia()
    {
        var cpf = CPF.Criar("12345678909");

        cpf.Should().NotBeNull();
        cpf.Valor.Should().Be("12345678909");
    }

    [Fact]
    public void ToString_DeveRetornarValor()
    {
        var cpf = new CPF("12345678909");

        cpf.ToString().Should().Be("12345678909");
    }
}
