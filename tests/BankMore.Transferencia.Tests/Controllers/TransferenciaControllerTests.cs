using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using BankMore.Transferencia.API.Controllers;
using BankMore.Transferencia.API.Application.Commands;
using BankMore.Transferencia.API.DTOs;
using BankMore.Transferencia.API.Domain.Repositories;
using TransferenciaEntity = BankMore.Transferencia.API.Domain.Entities.Transferencia;

namespace BankMore.Transferencia.Tests.Controllers;

public class TransferenciaControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ITransferenciaRepository> _mockRepository;
    private readonly Mock<ILogger<TransferenciaController>> _mockLogger;
    private readonly TransferenciaController _controller;

    public TransferenciaControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockRepository = new Mock<ITransferenciaRepository>();
        _mockLogger = new Mock<ILogger<TransferenciaController>>();
        _controller = new TransferenciaController(_mockMediator.Object, _mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task RealizarTransferencia_ComDadosValidos_DeveRetornar204()
    {
        var request = new RealizarTransferenciaRequest(
            Guid.NewGuid().ToString(),
            2,
            100m
        );

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        _mockRepository.Setup(r => r.ObterPorIdRequisicaoAsync(request.IdRequisicao))
            .ReturnsAsync((TransferenciaEntity?)null);

        _mockMediator.Setup(m => m.Send(It.IsAny<RealizarTransferenciaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _controller.RealizarTransferencia(request);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task RealizarTransferencia_SemToken_DeveRetornar401()
    {
        var request = new RealizarTransferenciaRequest(
            Guid.NewGuid().ToString(),
            2,
            100m
        );

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = await _controller.RealizarTransferencia(request);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task RealizarTransferencia_ComValorInvalido_DeveRetornar400()
    {
        var request = new RealizarTransferenciaRequest(
            Guid.NewGuid().ToString(),
            2,
            -100m
        );

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        _mockRepository.Setup(r => r.ObterPorIdRequisicaoAsync(request.IdRequisicao))
            .ReturnsAsync((TransferenciaEntity?)null);

        _mockMediator.Setup(m => m.Send(It.IsAny<RealizarTransferenciaCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Valor da transferência deve ser positivo"));

        var result = await _controller.RealizarTransferencia(request);

        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = (BadRequestObjectResult)result;
        var errorResponse = badRequest.Value as ErrorResponse;
        errorResponse.Should().NotBeNull();
        errorResponse!.Message.Should().Be("Valor da transferência deve ser positivo");
    }

    [Fact]
    public async Task RealizarTransferencia_ComFalha_DeveRetornar400()
    {
        var request = new RealizarTransferenciaRequest(
            Guid.NewGuid().ToString(),
            2,
            100m
        );

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        _mockRepository.Setup(r => r.ObterPorIdRequisicaoAsync(request.IdRequisicao))
            .ReturnsAsync((TransferenciaEntity?)null);

        _mockMediator.Setup(m => m.Send(It.IsAny<RealizarTransferenciaCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Falha ao realizar débito na conta origem"));

        var result = await _controller.RealizarTransferencia(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
