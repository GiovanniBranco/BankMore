using System.Security.Claims;
using BankMore.ContaCorrente.API.Application.Commands;
using BankMore.ContaCorrente.API.Application.Queries;
using BankMore.ContaCorrente.API.Controllers;
using BankMore.ContaCorrente.API.DTOs;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BankMore.ContaCorrente.Tests.Controllers;

public class ContaCorrenteControllerAuthorizationTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ContaCorrenteController _controller;

    public ContaCorrenteControllerAuthorizationTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new ContaCorrenteController(_mediatorMock.Object);
    }

    private void SetupUserClaims(int userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task ObterSaldo_UsuarioAutorizadoParaSuaConta_DeveRetornarOk()
    {
        var userId = 1;
        SetupUserClaims(userId);

        var saldoResult = new SaldoResult(1, 123456789, "João Silva", 1000.00m);
        _mediatorMock.Setup(x => x.Send(It.IsAny<ObterSaldoQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(saldoResult);

        var result = await _controller.ObterSaldo(userId);

        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ObterSaldo_UsuarioTentandoAcessarContaDeOutro_DeveRetornar403()
    {
        var userId = 1;
        var contaIdDiferente = 2;
        SetupUserClaims(userId);

        var result = await _controller.ObterSaldo(contaIdDiferente);

        var statusCodeResult = result.Result as ObjectResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult!.StatusCode.Should().Be(403);
        
        var errorResponse = statusCodeResult.Value as ErrorResponse;
        errorResponse.Should().NotBeNull();
        errorResponse!.Type.Should().Be("FORBIDDEN");
        errorResponse.Message.Should().Contain("Acesso negado");
    }

    [Fact]
    public async Task InativarConta_UsuarioAutorizadoParaSuaConta_DeveRetornarNoContent()
    {
        var userId = 1;
        SetupUserClaims(userId);

        _mediatorMock.Setup(x => x.Send(It.IsAny<InativarContaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        var request = new InativarContaRequest("senha123");
        var result = await _controller.InativarConta(userId, request);

        var noContentResult = result as NoContentResult;
        noContentResult.Should().NotBeNull();
    }

    [Fact]
    public async Task InativarConta_UsuarioTentandoInativarContaDeOutro_DeveRetornar403()
    {
        var userId = 1;
        var contaIdDiferente = 2;
        SetupUserClaims(userId);

        var request = new InativarContaRequest("senha123");
        var result = await _controller.InativarConta(contaIdDiferente, request);

        var statusCodeResult = result as ObjectResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult!.StatusCode.Should().Be(403);
        
        var errorResponse = statusCodeResult.Value as ErrorResponse;
        errorResponse.Should().NotBeNull();
        errorResponse!.Type.Should().Be("FORBIDDEN");
    }

    [Fact]
    public async Task RealizarMovimentacao_Debito_UsuarioAutorizadoParaSuaConta_DeveProcessar()
    {
        var userId = 1;
        SetupUserClaims(userId);

        var movimentacaoResult = new MovimentacaoResult(1, 1, DateTime.UtcNow, 'D', 100.00m, 900.00m);
        _mediatorMock.Setup(x => x.Send(It.IsAny<RealizarMovimentacaoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(movimentacaoResult);

        var request = new MovimentacaoRequest("req-123", "D", 100.00m);
        var result = await _controller.RealizarMovimentacao(userId, request);

        result.Result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task RealizarMovimentacao_Credito_QualquerUsuario_DeveProcessar()
    {
        var userId = 1;
        var contaIdDiferente = 2;
        SetupUserClaims(userId);

        var movimentacaoResult = new MovimentacaoResult(1, contaIdDiferente, DateTime.UtcNow, 'C', 100.00m, 100.00m);
        _mediatorMock.Setup(x => x.Send(It.IsAny<RealizarMovimentacaoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(movimentacaoResult);

        var request = new MovimentacaoRequest("req-123", "C", 100.00m);
        var result = await _controller.RealizarMovimentacao(contaIdDiferente, request);

        result.Result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task RealizarMovimentacao_Debito_UsuarioTentandoMovimentarContaDeOutro_DeveRetornar403()
    {
        var userId = 1;
        var contaIdDiferente = 2;
        SetupUserClaims(userId);

        var request = new MovimentacaoRequest("req-123", "D", 100.00m);
        var result = await _controller.RealizarMovimentacao(contaIdDiferente, request);

        var statusCodeResult = result.Result as ObjectResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult!.StatusCode.Should().Be(403);
        
        var errorResponse = statusCodeResult.Value as ErrorResponse;
        errorResponse.Should().NotBeNull();
        errorResponse!.Type.Should().Be("FORBIDDEN");
    }

    [Fact]
    public async Task ObterExtrato_UsuarioAutorizadoParaSuaConta_DeveRetornarOk()
    {
        var userId = 1;
        SetupUserClaims(userId);

        var extratoResult = new ExtratoResult(1, 123456789, "João Silva", 1000.00m, new List<MovimentoItem>());
        _mediatorMock.Setup(x => x.Send(It.IsAny<ObterExtratoQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(extratoResult);

        var result = await _controller.ObterExtrato(userId);

        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ObterExtrato_UsuarioTentandoAcessarExtratoDeOutro_DeveRetornar403()
    {
        var userId = 1;
        var contaIdDiferente = 2;
        SetupUserClaims(userId);

        var result = await _controller.ObterExtrato(contaIdDiferente);

        var statusCodeResult = result.Result as ObjectResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult!.StatusCode.Should().Be(403);
        
        var errorResponse = statusCodeResult.Value as ErrorResponse;
        errorResponse.Should().NotBeNull();
        errorResponse!.Type.Should().Be("FORBIDDEN");
        errorResponse.Message.Should().Contain("Acesso negado");
    }

    [Fact]
    public async Task ObterSaldo_SemClaimDeUsuario_DeveRetornar403()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        var result = await _controller.ObterSaldo(1);

        var statusCodeResult = result.Result as ObjectResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult!.StatusCode.Should().Be(403);
    }
}
