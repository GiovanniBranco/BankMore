using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BankMore.ContaCorrente.API.Domain.Repositories;
using BankMore.ContaCorrente.API.DTOs;
using BankMore.ContaCorrente.API.Infrastructure.Services;
using BankMore.ContaCorrente.API.Domain.Exceptions;

namespace BankMore.ContaCorrente.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IContaCorrenteRepository _repository;
    private readonly IJwtService _jwtService;

    public AuthController(IContaCorrenteRepository repository, IJwtService jwtService)
    {
        _repository = repository;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var identificador = request.GetIdentificador();
            
            if (string.IsNullOrWhiteSpace(identificador))
                throw new UnauthorizedUserException("CPF ou número da conta é obrigatório");
            
            Domain.Entities.ContaCorrente? conta = null;

            if (int.TryParse(identificador, out int numeroConta))
            {
                conta = await _repository.ObterPorNumeroAsync(numeroConta);
            }
            
            if (conta == null)
            {
                conta = await _repository.ObterPorCpfAsync(identificador);
            }
            
            if (conta == null || !conta.ValidarSenha(request.Senha))
                throw new UnauthorizedUserException("CPF/Conta ou senha inválidos");

            if (!conta.Ativo)
                throw new UnauthorizedUserException("Conta inativa");

            var token = _jwtService.GenerateToken(conta.Id, conta.Nome, conta.Cpf.Valor);
            
            var response = new LoginResponse(token, conta.Id, conta.Numero, conta.Nome);
            
            return Ok(response);
        }
        catch (UnauthorizedUserException ex)
        {
            return Unauthorized(new ErrorResponse(ex.Message, ex.Type));
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorResponse("Erro interno do servidor", "INTERNAL_ERROR"));
        }
    }
}
