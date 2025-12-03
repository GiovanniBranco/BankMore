using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BankMore.ContaCorrente.API.Application.Commands;
using BankMore.ContaCorrente.API.Application.Queries;
using BankMore.ContaCorrente.API.DTOs;
using BankMore.ContaCorrente.API.Domain.Exceptions;
using System.Security.Claims;

namespace BankMore.ContaCorrente.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContaCorrenteController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContaCorrenteController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private bool ValidarAutorizacao(int idConta)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userIdClaim != null && int.Parse(userIdClaim) == idConta;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<CriarContaResponse>> CriarConta([FromBody] CriarContaRequest request)
    {
        try
        {
            var command = new CriarContaCommand(request.Nome, request.Cpf, request.Senha);
            var result = await _mediator.Send(command);
            
            var response = new CriarContaResponse(
                result.Id,
                result.Numero,
                result.Nome,
                result.Cpf
            );
            
            return CreatedAtAction(nameof(ObterSaldo), new { id = result.Id }, response);
        }
        catch (InvalidDocumentException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message, ex.Type));
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }

    [HttpGet("{id}/saldo")]
    public async Task<ActionResult<SaldoResponse>> ObterSaldo(int id)
    {
        if (!ValidarAutorizacao(id))
            return StatusCode(403, new ErrorResponse("Acesso negado. Você não tem permissão para acessar esta conta.", "FORBIDDEN"));

        try
        {
            var query = new ObterSaldoQuery(id);
            var result = await _mediator.Send(query);
            
            var response = new SaldoResponse(
                result.Id,
                result.Numero,
                result.Nome,
                result.Saldo
            );
            
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> InativarConta(int id, [FromBody] InativarContaRequest request)
    {
        if (!ValidarAutorizacao(id))
            return StatusCode(403, new ErrorResponse("Acesso negado. Você não tem permissão para acessar esta conta.", "FORBIDDEN"));

        try
        {
            var command = new InativarContaCommand(id, request.Senha);
            await _mediator.Send(command);
            
            return NoContent();
        }
        catch (UnauthorizedUserException ex)
        {
            return StatusCode(403, new ErrorResponse(ex.Message, ex.Type));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }

    [HttpPost("{id}/movimentacao")]
    public async Task<ActionResult<MovimentacaoResponse>> RealizarMovimentacao(int id, [FromBody] MovimentacaoRequest request)
    {
        if (request.Tipo == "D" && !ValidarAutorizacao(id))
            return StatusCode(403, new ErrorResponse("Acesso negado. Você não tem permissão para acessar esta conta.", "FORBIDDEN"));

        try
        {
            var command = new RealizarMovimentacaoCommand(request.IdRequisicao, id, request.Tipo, request.Valor);
            var result = await _mediator.Send(command);
            
            return NoContent();
        }
        catch (DuplicateRequestException ex)
        {
            if (ex.PreviousResult is MovimentacaoResult previousResult)
            {
                var response = new MovimentacaoResponse(
                    previousResult.Id,
                    previousResult.IdConta,
                    previousResult.DataMovimento,
                    previousResult.Tipo,
                    previousResult.Valor,
                    previousResult.NovoSaldo
                );
                return Ok(response);
            }
            return BadRequest(new ErrorResponse(ex.Message, ex.Type));
        }
        catch (InsufficientBalanceException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message, ex.Type));
        }
        catch (InactiveAccountException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message, ex.Type));
        }
        catch (InvalidValueException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message, ex.Type));
        }
        catch (InvalidTypeException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message, ex.Type));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorResponse("Erro interno do servidor", "INTERNAL_ERROR"));
        }
    }

    [HttpGet("{id}/extrato")]
    public async Task<ActionResult<ExtratoResponse>> ObterExtrato(int id)
    {
        if (!ValidarAutorizacao(id))
            return StatusCode(403, new ErrorResponse("Acesso negado. Você não tem permissão para acessar esta conta.", "FORBIDDEN"));

        try
        {
            var query = new ObterExtratoQuery(id);
            var result = await _mediator.Send(query);
            
            var movimentos = result.Movimentos.Select(m => new MovimentoItemDto(
                m.Id,
                m.DataMovimento,
                m.Tipo,
                m.Valor,
                m.Descricao
            )).ToList();

            var response = new ExtratoResponse(
                result.IdConta,
                result.Numero,
                result.Nome,
                result.SaldoAtual,
                movimentos
            );
            
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }
}
