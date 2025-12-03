using BankMore.ContaCorrente.API.Application.Commands;
using BankMore.ContaCorrente.API.Domain.Entities;
using BankMore.ContaCorrente.API.Domain.Exceptions;
using BankMore.ContaCorrente.API.Domain.Repositories;
using BankMore.ContaCorrente.API.Domain.ValueObjects;
using ContaCorrenteEntity = BankMore.ContaCorrente.API.Domain.Entities.ContaCorrente;
using FluentAssertions;
using Moq;
using Xunit;

namespace BankMore.ContaCorrente.Tests.Application.Commands;

public class RealizarMovimentacaoCommandHandlerTests
{
    private readonly Mock<IContaCorrenteRepository> _contaRepositoryMock;
    private readonly Mock<IMovimentoRepository> _movimentoRepositoryMock;
    private readonly RealizarMovimentacaoCommandHandler _handler;

    public RealizarMovimentacaoCommandHandlerTests()
    {
        _contaRepositoryMock = new Mock<IContaCorrenteRepository>();
        _movimentoRepositoryMock = new Mock<IMovimentoRepository>();
        _handler = new RealizarMovimentacaoCommandHandler(
            _contaRepositoryMock.Object,
            _movimentoRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ComCredito_DeveRealizarMovimentacao()
    {
        var cpf = new CPF("12345678909");
        var conta = new ContaCorrenteEntity(1, 123456789, "João Silva", cpf, "hash", true);
        var command = new RealizarMovimentacaoCommand("req-123", 1, "C", 100.00m);

        _contaRepositoryMock.Setup(x => x.ObterPorIdAsync(1)).ReturnsAsync(conta);
        _movimentoRepositoryMock.Setup(x => x.ObterPorIdRequisicaoAsync(It.IsAny<string>())).ReturnsAsync((Movimento?)null);
        _movimentoRepositoryMock.Setup(x => x.ObterSaldoAsync(1)).ReturnsAsync(100.00m);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Valor.Should().Be(100.00m);
        result.Tipo.Should().Be('C');
        _movimentoRepositoryMock.Verify(x => x.AdicionarAsync(It.IsAny<Movimento>(), "req-123"), Times.Once);
    }

    [Fact]
    public async Task Handle_ComDebito_DeveLancarExcecaoSeInsuficiente()
    {
        var cpf = new CPF("12345678909");
        var conta = new ContaCorrenteEntity(1, 123456789, "João Silva", cpf, "hash", true);
        var command = new RealizarMovimentacaoCommand("req-456", 1, "D", 100.00m);

        _contaRepositoryMock.Setup(x => x.ObterPorIdAsync(1)).ReturnsAsync(conta);
        _movimentoRepositoryMock.Setup(x => x.ObterPorIdRequisicaoAsync(It.IsAny<string>())).ReturnsAsync((Movimento?)null);
        _movimentoRepositoryMock.Setup(x => x.ObterSaldoAsync(1)).ReturnsAsync(50.00m);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InsufficientBalanceException>()
            .WithMessage("Saldo insuficiente");
    }

    [Fact]
    public async Task Handle_ComContaInativa_DeveLancarExcecao()
    {
        var cpf = new CPF("12345678909");
        var conta = new ContaCorrenteEntity(1, 123456789, "João Silva", cpf, "hash", false);
        var command = new RealizarMovimentacaoCommand("req-789", 1, "C", 100.00m);

        _contaRepositoryMock.Setup(x => x.ObterPorIdAsync(1)).ReturnsAsync(conta);
        _movimentoRepositoryMock.Setup(x => x.ObterPorIdRequisicaoAsync(It.IsAny<string>())).ReturnsAsync((Movimento?)null);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InactiveAccountException>()
            .WithMessage("Conta inativa");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(-100.50)]
    public async Task Handle_ComValorInvalido_DeveLancarExcecao(decimal valorInvalido)
    {
        var command = new RealizarMovimentacaoCommand("req-valor", 1, "C", valorInvalido);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidValueException>()
            .WithMessage("Valor deve ser positivo");
    }

    [Fact]
    public async Task Handle_ComRequisicaoDuplicada_DeveLancarExcecao()
    {
        var movimento = new Movimento(1, 1, DateTime.UtcNow, 'C', 100.00m);
        var command = new RealizarMovimentacaoCommand("req-duplicada", 1, "C", 100.00m);

        _movimentoRepositoryMock.Setup(x => x.ObterPorIdRequisicaoAsync("req-duplicada")).ReturnsAsync(movimento);
        _movimentoRepositoryMock.Setup(x => x.ObterSaldoAsync(1)).ReturnsAsync(100.00m);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DuplicateRequestException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("X")]
    [InlineData("CD")]
    public async Task Handle_ComTipoInvalido_DeveLancarExcecao(string tipoInvalido)
    {
        var command = new RealizarMovimentacaoCommand("req-999", 1, tipoInvalido, 100.00m);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidTypeException>()
            .WithMessage("Tipo deve ser 'C' (crédito) ou 'D' (débito)");
    }
}
