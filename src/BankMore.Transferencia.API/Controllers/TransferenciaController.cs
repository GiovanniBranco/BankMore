using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BankMore.Transferencia.API.Application.Commands;
using BankMore.Transferencia.API.DTOs;
using BankMore.Transferencia.API.Domain.Repositories;

namespace BankMore.Transferencia.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransferenciaController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITransferenciaRepository _repository;
    private readonly ILogger<TransferenciaController> _logger;

    public TransferenciaController(
        IMediator mediator, 
        ITransferenciaRepository repository,
        ILogger<TransferenciaController> logger)
    {
        _mediator = mediator;
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Realiza transferência entre contas da mesma instituição
    /// </summary>
    /// <param name="request">Dados da transferência</param>
    /// <returns>Status da operação</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RealizarTransferencia([FromBody] RealizarTransferenciaRequest request)
    {
        try
        {
            var idContaOrigemClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idContaOrigemClaim) || !int.TryParse(idContaOrigemClaim, out var idContaOrigem))
            {
                _logger.LogWarning("Token inválido ou ausente ao tentar realizar transferência");
                return Unauthorized(new ErrorResponse(
                    "Token inválido ou ausente",
                    "INVALID_TOKEN"
                ));
            }

            var transferenciaExistente = await _repository.ObterPorIdRequisicaoAsync(request.IdRequisicao);
            if (transferenciaExistente != null)
            {
                _logger.LogInformation("Requisição duplicada detectada: {IdRequisicao}", request.IdRequisicao);
                
                var responseExistente = new TransferenciaResponse(
                    transferenciaExistente.Id,
                    transferenciaExistente.IdRequisicao,
                    transferenciaExistente.IdContaOrigem,
                    transferenciaExistente.IdContaDestino,
                    transferenciaExistente.Valor,
                    transferenciaExistente.DataTransferencia,
                    transferenciaExistente.Status.ToString()
                );

                return Ok(responseExistente);
            }

            var command = new RealizarTransferenciaCommand(
                request.IdRequisicao,
                idContaOrigem,
                request.IdContaDestino,
                request.Valor
            );

            var transferenciaId = await _mediator.Send(command);

            _logger.LogInformation(
                "Transferência realizada com sucesso. ID: {TransferenciaId}, Origem: {IdOrigem}, Destino: {IdDestino}, Valor: {Valor}",
                transferenciaId, idContaOrigem, request.IdContaDestino, request.Valor
            );

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao realizar transferência");
            return BadRequest(new ErrorResponse(
                ex.Message,
                "VALIDATION_ERROR"
            ));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Erro de operação ao realizar transferência");
            return BadRequest(new ErrorResponse(
                ex.Message,
                "OPERATION_ERROR"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao realizar transferência");
            return StatusCode(500, new ErrorResponse(
                "Erro interno do servidor",
                "INTERNAL_ERROR"
            ));
        }
    }

    /// <summary>
    /// Consulta transferência por ID de requisição (idempotência)
    /// </summary>
    /// <param name="idRequisicao">ID da requisição</param>
    /// <returns>Dados da transferência</returns>
    [HttpGet("{idRequisicao}")]
    [ProducesResponseType(typeof(TransferenciaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorIdRequisicao(string idRequisicao)
    {
        var transferencia = await _repository.ObterPorIdRequisicaoAsync(idRequisicao);
        
        if (transferencia == null)
            return NotFound();

        var response = new TransferenciaResponse(
            transferencia.Id,
            transferencia.IdRequisicao,
            transferencia.IdContaOrigem,
            transferencia.IdContaDestino,
            transferencia.Valor,
            transferencia.DataTransferencia,
            transferencia.Status.ToString()
        );

        return Ok(response);
    }
}

