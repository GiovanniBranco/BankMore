using MediatR;
using BankMore.ContaCorrente.API.Domain.Repositories;

namespace BankMore.ContaCorrente.API.Application.Queries;

public class ObterExtratoQueryHandler : IRequestHandler<ObterExtratoQuery, ExtratoResult>
{
    private readonly IContaCorrenteRepository _contaRepository;
    private readonly IMovimentoRepository _movimentoRepository;

    public ObterExtratoQueryHandler(
        IContaCorrenteRepository contaRepository,
        IMovimentoRepository movimentoRepository)
    {
        _contaRepository = contaRepository;
        _movimentoRepository = movimentoRepository;
    }

    public async Task<ExtratoResult> Handle(ObterExtratoQuery request, CancellationToken cancellationToken)
    {
        var conta = await _contaRepository.ObterPorIdAsync(request.IdConta)
            ?? throw new InvalidOperationException("Conta não encontrada");

        var saldo = await _movimentoRepository.ObterSaldoAsync(request.IdConta);
        var movimentos = await _movimentoRepository.ObterPorContaAsync(request.IdConta);

        var movimentoItems = movimentos.Select(m => new MovimentoItem(
            m.Id,
            m.DataMovimento,
            m.Tipo,
            m.Valor,
            m.Tipo == 'C' ? "Crédito" : "Débito"
        )).ToList();

        return new ExtratoResult(conta.Id, conta.Numero, conta.Nome, saldo, movimentoItems);
    }
}
