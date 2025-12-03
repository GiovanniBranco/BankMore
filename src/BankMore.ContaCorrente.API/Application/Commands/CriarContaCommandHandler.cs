using MediatR;
using BankMore.ContaCorrente.API.Domain.Repositories;
using BankMore.ContaCorrente.API.Domain.ValueObjects;

namespace BankMore.ContaCorrente.API.Application.Commands;

public class CriarContaCommandHandler : IRequestHandler<CriarContaCommand, CriarContaResult>
{
    private readonly IContaCorrenteRepository _repository;

    public CriarContaCommandHandler(IContaCorrenteRepository repository)
    {
        _repository = repository;
    }

    public async Task<CriarContaResult> Handle(CriarContaCommand request, CancellationToken cancellationToken)
    {
        var cpf = new CPF(request.Cpf);
        
        if (await _repository.ExisteCpfAsync(cpf.Valor))
            throw new InvalidOperationException("CPF já cadastrado");

        var numeroConta = await _repository.GerarNumeroContaUnicoAsync();
        
        var conta = Domain.Entities.ContaCorrente.Criar(
            numeroConta,
            request.Nome,
            cpf,
            request.Senha
        );

        await _repository.AdicionarAsync(conta);

        return new CriarContaResult(conta.Id, conta.Numero, conta.Nome, conta.Cpf.Valor);
    }
}
