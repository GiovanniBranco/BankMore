using BankMore.ContaCorrente.API.Domain.ValueObjects;
using FluentAssertions;
using Xunit;
using ContaCorrenteEntity = BankMore.ContaCorrente.API.Domain.Entities.ContaCorrente;

namespace BankMore.ContaCorrente.Tests.Domain.Entities;

public class ContaCorrenteTests
{
    [Fact]
    public void Criar_ComDadosValidos_DeveCriarContaAtiva()
    {
        var cpf = new CPF("12345678909");
        var numero = 123456789;
        var nome = "João Silva";
        var senha = "senha123";

        var conta = ContaCorrenteEntity.Criar(numero, nome, cpf, senha);

        conta.Should().NotBeNull();
        conta.Numero.Should().Be(numero);
        conta.Nome.Should().Be(nome);
        conta.Cpf.Should().Be(cpf);
        conta.Ativo.Should().BeTrue();
        conta.Senha.Should().NotBeNullOrEmpty();
        conta.Senha.Should().NotBe(senha);
    }

    [Fact]
    public void Criar_ComCpfNulo_DeveLancarExcecao()
    {
        Action act = () => ContaCorrenteEntity.Criar(123456789, "João Silva", null!, "senha123");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Criar_ComNomeNulo_DeveLancarExcecao()
    {
        var cpf = new CPF("12345678909");

        Action act = () => ContaCorrenteEntity.Criar(123456789, null!, cpf, "senha123");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Inativar_DeveAlterarStatusParaInativo()
    {
        var cpf = new CPF("12345678909");
        var conta = ContaCorrenteEntity.Criar(123456789, "João Silva", cpf, "senha123");

        conta.Inativar();

        conta.Ativo.Should().BeFalse();
    }

    [Fact]
    public void ValidarSenha_ComSenhaCorreta_DeveRetornarTrue()
    {
        var cpf = new CPF("12345678909");
        var senha = "senha123";
        var conta = ContaCorrenteEntity.Criar(123456789, "João Silva", cpf, senha);

        var resultado = conta.ValidarSenha(senha);

        resultado.Should().BeTrue();
    }

    [Fact]
    public void ValidarSenha_ComSenhaIncorreta_DeveRetornarFalse()
    {
        var cpf = new CPF("12345678909");
        var conta = ContaCorrenteEntity.Criar(123456789, "João Silva", cpf, "senha123");

        var resultado = conta.ValidarSenha("senhaErrada");

        resultado.Should().BeFalse();
    }

    [Fact]
    public void Constructor_ComParametros_DeveCriarInstancia()
    {
        var cpf = new CPF("12345678909");
        var senhaHash = BCrypt.Net.BCrypt.HashPassword("senha123");

        var conta = new ContaCorrenteEntity(1, 123456789, "João Silva", cpf, senhaHash, true);

        conta.Id.Should().Be(1);
        conta.Numero.Should().Be(123456789);
        conta.Nome.Should().Be("João Silva");
        conta.Cpf.Should().Be(cpf);
        conta.Senha.Should().Be(senhaHash);
        conta.Ativo.Should().BeTrue();
    }
}
