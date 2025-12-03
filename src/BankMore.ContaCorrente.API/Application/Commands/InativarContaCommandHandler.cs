using MediatR;
using BankMore.ContaCorrente.API.Domain.Repositories;
using BankMore.ContaCorrente.API.Domain.Exceptions;

namespace BankMore.ContaCorrente.API.Application.Commands;

public class InativarContaCommandHandler : IRequestHandler<InativarContaCommand, Unit>
{
    private readonly IContaCorrenteRepository _repository;

    public InativarContaCommandHandler(IContaCorrenteRepository repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(InativarContaCommand request, CancellationToken cancellationToken)
    {
        var conta = await _repository.ObterPorIdAsync(request.Id)
            ?? throw new InvalidOperationException("Conta não encontrada");

        if (!conta.Ativo)
            throw new InvalidOperationException("Conta já está inativa");

        if (!conta.ValidarSenha(request.Senha))
            throw new UnauthorizedUserException("Senha inválida");

        conta.Inativar();
        await _repository.AtualizarAsync(conta);

        return Unit.Value;
    }
}
