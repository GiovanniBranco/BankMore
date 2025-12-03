using BankMore.ContaCorrente.API.Application.Commands;
using BankMore.ContaCorrente.API.Domain.Exceptions;
using BankMore.ContaCorrente.API.Domain.Repositories;
using ContaCorrenteEntity = BankMore.ContaCorrente.API.Domain.Entities.ContaCorrente;
using FluentAssertions;
using Moq;
using Xunit;

namespace BankMore.ContaCorrente.Tests.Application.Commands;

public class CriarContaCommandHandlerTests
{
    private readonly Mock<IContaCorrenteRepository> _repositoryMock;
    private readonly CriarContaCommandHandler _handler;

    public CriarContaCommandHandlerTests()
    {
        _repositoryMock = new Mock<IContaCorrenteRepository>();
        _handler = new CriarContaCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ComDadosValidos_DeveCriarConta()
    {
        var command = new CriarContaCommand("João Silva", "12345678909", "senha123");
        _repositoryMock.Setup(x => x.ExisteCpfAsync(It.IsAny<string>())).ReturnsAsync(false);
        _repositoryMock.Setup(x => x.GerarNumeroContaUnicoAsync()).ReturnsAsync(123456789);
        _repositoryMock.Setup(x => x.AdicionarAsync(It.IsAny<ContaCorrenteEntity>()))
            .Callback<ContaCorrenteEntity>(c => c.Id = 1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Numero.Should().Be(123456789);
        result.Nome.Should().Be("João Silva");
        result.Cpf.Should().Be("12345678909");
        _repositoryMock.Verify(x => x.AdicionarAsync(It.IsAny<ContaCorrenteEntity>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ComCPFInvalido_DeveLancarExcecao()
    {
        var command = new CriarContaCommand("João Silva", "11111111111", "senha123");

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidDocumentException>();
    }

    [Fact]
    public async Task Handle_ComCPFJaCadastrado_DeveLancarExcecao()
    {
        var command = new CriarContaCommand("João Silva", "12345678909", "senha123");
        _repositoryMock.Setup(x => x.ExisteCpfAsync(It.IsAny<string>())).ReturnsAsync(true);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CPF já cadastrado");
    }
}
