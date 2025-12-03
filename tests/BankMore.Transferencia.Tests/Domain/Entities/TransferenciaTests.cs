using FluentAssertions;
using BankMore.Transferencia.API.Domain.Enums;
using TransferenciaEntity = BankMore.Transferencia.API.Domain.Entities.Transferencia;

namespace BankMore.Transferencia.Tests.Domain.Entities;

public class TransferenciaTests
{
    [Fact]
    public void Criar_ComDadosValidos_DeveCriarTransferencia()
    {
        var idRequisicao = Guid.NewGuid().ToString();
        var idContaOrigem = 1;
        var idContaDestino = 2;
        var valor = 100.50m;

        var transferencia = new TransferenciaEntity(idRequisicao, idContaOrigem, idContaDestino, valor);

        transferencia.IdRequisicao.Should().Be(idRequisicao);
        transferencia.IdContaOrigem.Should().Be(idContaOrigem);
        transferencia.IdContaDestino.Should().Be(idContaDestino);
        transferencia.Valor.Should().Be(valor);
        transferencia.Status.Should().Be(StatusTransferencia.Processada);
        transferencia.DataTransferencia.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Criar_ComValorNegativo_DeveLancarExcecao()
    {
        var idRequisicao = Guid.NewGuid().ToString();
        var idContaOrigem = 1;
        var idContaDestino = 2;
        var valor = -100.50m;

        var act = () => new TransferenciaEntity(idRequisicao, idContaOrigem, idContaDestino, valor);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Valor da transferência deve ser positivo");
    }

    [Fact]
    public void Criar_ComValorZero_DeveLancarExcecao()
    {
        var idRequisicao = Guid.NewGuid().ToString();
        var idContaOrigem = 1;
        var idContaDestino = 2;
        var valor = 0m;

        var act = () => new TransferenciaEntity(idRequisicao, idContaOrigem, idContaDestino, valor);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Valor da transferência deve ser positivo");
    }

    [Fact]
    public void Criar_ComContaOrigemIgualDestino_DeveLancarExcecao()
    {
        var idRequisicao = Guid.NewGuid().ToString();
        var idConta = 1;
        var valor = 100.50m;

        var act = () => new TransferenciaEntity(idRequisicao, idConta, idConta, valor);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Conta origem e destino não podem ser iguais");
    }

    [Fact]
    public void MarcarComoEstornada_DeveAlterarStatus()
    {
        var transferencia = new TransferenciaEntity(Guid.NewGuid().ToString(), 1, 2, 100m);

        transferencia.MarcarComoEstornada();

        transferencia.Status.Should().Be(StatusTransferencia.Estornada);
    }

    [Fact]
    public void MarcarComoFalha_DeveAlterarStatus()
    {
        var transferencia = new TransferenciaEntity(Guid.NewGuid().ToString(), 1, 2, 100m);

        transferencia.MarcarComoFalha();

        transferencia.Status.Should().Be(StatusTransferencia.Falha);
    }
}
