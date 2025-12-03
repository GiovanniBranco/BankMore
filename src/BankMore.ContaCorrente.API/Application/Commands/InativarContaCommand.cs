using MediatR;

namespace BankMore.ContaCorrente.API.Application.Commands;

public record InativarContaCommand(int Id, string Senha) : IRequest<Unit>;
