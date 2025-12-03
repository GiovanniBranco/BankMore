using FluentAssertions;
using Moq;
using System.Data;
using BankMore.Transferencia.API.Domain.Repositories;
using BankMore.Transferencia.API.Infrastructure.Repositories;
using BankMore.Transferencia.API.Domain.Enums;
using TransferenciaEntity = BankMore.Transferencia.API.Domain.Entities.Transferencia;

namespace BankMore.Transferencia.Tests.Infrastructure.Repositories;

public class TransferenciaRepositoryTests : IDisposable
{
    private readonly string _dbPath;

    public TransferenciaRepositoryTests()
    {
        _dbPath = $"test_{Guid.NewGuid()}.db";
    }

    public void Dispose()
    {
        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }

    [Fact]
    public async Task AdicionarAsync_ComTransferenciaValida_DeveInserir()
    {
        var connectionString = $"Data Source={_dbPath}";
        var repository = new TransferenciaRepository(connectionString);
        var transferencia = new TransferenciaEntity(
            Guid.NewGuid().ToString(),
            1,
            2,
            100.50m
        );

        await repository.InicializarBancoDeDadosAsync();
        var id = await repository.AdicionarAsync(transferencia);

        id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ObterPorIdRequisicaoAsync_ComIdExistente_DeveRetornar()
    {
        var connectionString = $"Data Source={_dbPath}";
        var repository = new TransferenciaRepository(connectionString);
        var idRequisicao = Guid.NewGuid().ToString();
        var transferencia = new TransferenciaEntity(idRequisicao, 1, 2, 100m);

        await repository.InicializarBancoDeDadosAsync();
        await repository.AdicionarAsync(transferencia);
        var resultado = await repository.ObterPorIdRequisicaoAsync(idRequisicao);

        resultado.Should().NotBeNull();
        resultado!.IdRequisicao.Should().Be(idRequisicao);
        resultado.IdContaOrigem.Should().Be(1);
        resultado.IdContaDestino.Should().Be(2);
        resultado.Valor.Should().Be(100m);
    }

    [Fact]
    public async Task ObterPorIdRequisicaoAsync_ComIdInexistente_DeveRetornarNull()
    {
        var connectionString = $"Data Source={_dbPath}";
        var repository = new TransferenciaRepository(connectionString);
        var idRequisicao = Guid.NewGuid().ToString();

        await repository.InicializarBancoDeDadosAsync();
        var resultado = await repository.ObterPorIdRequisicaoAsync(idRequisicao);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task AtualizarStatusAsync_DeveAlterarStatus()
    {
        var connectionString = $"Data Source={_dbPath}";
        var repository = new TransferenciaRepository(connectionString);
        var transferencia = new TransferenciaEntity(Guid.NewGuid().ToString(), 1, 2, 100m);

        await repository.InicializarBancoDeDadosAsync();
        var id = await repository.AdicionarAsync(transferencia);
        
        await repository.AtualizarStatusAsync(id, StatusTransferencia.Estornada);
        var resultado = await repository.ObterPorIdAsync(id);

        resultado.Should().NotBeNull();
        resultado!.Status.Should().Be(StatusTransferencia.Estornada);
    }
}
