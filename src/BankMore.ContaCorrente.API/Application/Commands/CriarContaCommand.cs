using MediatR;

namespace BankMore.ContaCorrente.API.Application.Commands;

public record CriarContaCommand(string Nome, string Cpf, string Senha) : IRequest<CriarContaResult>;

public record CriarContaResult(int Id, int Numero, string Nome, string Cpf);
