using FluentAssertions;
using Moq;
using BankMore.Transferencia.API.Application.Commands;
using BankMore.Transferencia.API.Domain.Repositories;
using BankMore.Transferencia.API.Infrastructure.Services;
using BankMore.Transferencia.API.Domain.Enums;
using TransferenciaEntity = BankMore.Transferencia.API.Domain.Entities.Transferencia;

namespace BankMore.Transferencia.Tests.Application.Commands;

public class RealizarTransferenciaCommandHandlerTests
{
    private readonly Mock<ITransferenciaRepository> _mockRepository;
    private readonly Mock<IContaCorrenteApiService> _mockApiService;
    private readonly RealizarTransferenciaCommandHandler _handler;

    public RealizarTransferenciaCommandHandlerTests()
    {
        _mockRepository = new Mock<ITransferenciaRepository>();
        _mockApiService = new Mock<IContaCorrenteApiService>();
        _handler = new RealizarTransferenciaCommandHandler(_mockRepository.Object, _mockApiService.Object);
    }

    [Fact]
    public async Task Handle_ComDadosValidos_DeveRealizarTransferencia()
    {
        var command = new RealizarTransferenciaCommand(
            Guid.NewGuid().ToString(),
            1,
            2,
            100m
        );

        _mockRepository.Setup(r => r.ObterPorIdRequisicaoAsync(command.IdRequisicao))
            .ReturnsAsync((TransferenciaEntity?)null);
        _mockApiService.Setup(s => s.RealizarDebitoAsync(command.IdContaOrigem, command.Valor, It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockApiService.Setup(s => s.RealizarCreditoAsync(command.IdContaDestino, command.Valor, It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockRepository.Setup(r => r.AdicionarAsync(It.IsAny<TransferenciaEntity>()))
            .ReturnsAsync(1);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Should().BeGreaterThan(0);
        _mockApiService.Verify(s => s.RealizarDebitoAsync(command.IdContaOrigem, command.Valor, It.IsAny<string>()), Times.Once);
        _mockApiService.Verify(s => s.RealizarCreditoAsync(command.IdContaDestino, command.Valor, It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(r => r.AdicionarAsync(It.Is<TransferenciaEntity>(t => 
            t.Status == StatusTransferencia.Processada &&
            t.IdRequisicao == command.IdRequisicao
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ComIdRequisicaoDuplicado_DeveRetornarTransferenciaExistente()
    {
        var command = new RealizarTransferenciaCommand(
            Guid.NewGuid().ToString(),
            1,
            2,
            100m
        );

        var transferenciaExistente = new TransferenciaEntity(command.IdRequisicao, 1, 2, 100m);
        typeof(TransferenciaEntity).GetProperty("Id")!.SetValue(transferenciaExistente, 999);

        _mockRepository.Setup(r => r.ObterPorIdRequisicaoAsync(command.IdRequisicao))
            .ReturnsAsync(transferenciaExistente);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Should().Be(999);
        _mockApiService.Verify(s => s.RealizarDebitoAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>()), Times.Never);
        _mockApiService.Verify(s => s.RealizarCreditoAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComFalhaNoDebito_DeveLancarExcecao()
    {
        var command = new RealizarTransferenciaCommand(
            Guid.NewGuid().ToString(),
            1,
            2,
            100m
        );

        _mockRepository.Setup(r => r.ObterPorIdRequisicaoAsync(command.IdRequisicao))
            .ReturnsAsync((TransferenciaEntity?)null);
        _mockApiService.Setup(s => s.RealizarDebitoAsync(command.IdContaOrigem, command.Valor, It.IsAny<string>()))
            .ReturnsAsync(false);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Falha ao realizar débito na conta origem");
        _mockApiService.Verify(s => s.RealizarCreditoAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>()), Times.Never);
        _mockRepository.Verify(r => r.AdicionarAsync(It.IsAny<TransferenciaEntity>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComFalhaNoCredito_DeveEstornarDebito()
    {
        var command = new RealizarTransferenciaCommand(
            Guid.NewGuid().ToString(),
            1,
            2,
            100m
        );

        _mockRepository.Setup(r => r.ObterPorIdRequisicaoAsync(command.IdRequisicao))
            .ReturnsAsync((TransferenciaEntity?)null);
        _mockApiService.Setup(s => s.RealizarDebitoAsync(command.IdContaOrigem, command.Valor, It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockApiService.Setup(s => s.RealizarCreditoAsync(command.IdContaDestino, command.Valor, It.IsAny<string>()))
            .ReturnsAsync(false);
        _mockApiService.Setup(s => s.RealizarCreditoAsync(command.IdContaOrigem, command.Valor, It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockRepository.Setup(r => r.AdicionarAsync(It.IsAny<TransferenciaEntity>()))
            .ReturnsAsync(1);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Falha ao realizar crédito na conta destino");
        
        _mockApiService.Verify(s => s.RealizarCreditoAsync(command.IdContaOrigem, command.Valor, It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(r => r.AdicionarAsync(It.Is<TransferenciaEntity>(t => 
            t.Status == StatusTransferencia.Estornada
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ComValorNegativo_DeveLancarExcecao()
    {
        var command = new RealizarTransferenciaCommand(
            Guid.NewGuid().ToString(),
            1,
            2,
            -100m
        );

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Valor da transferência deve ser positivo");
    }

    [Fact]
    public async Task Handle_ComContaOrigemIgualDestino_DeveLancarExcecao()
    {
        var command = new RealizarTransferenciaCommand(
            Guid.NewGuid().ToString(),
            1,
            1,
            100m
        );

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Conta origem e destino não podem ser iguais");
    }

    [Fact]
    public async Task Handle_ComValorZero_DeveLancarExcecao()
    {
        var command = new RealizarTransferenciaCommand(
            Guid.NewGuid().ToString(),
            1,
            2,
            0m
        );

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Valor da transferência deve ser positivo");
    }
}
