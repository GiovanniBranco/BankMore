using MediatR;
using BankMore.ContaCorrente.API.Domain.Repositories;
using BankMore.ContaCorrente.API.Infrastructure.Repositories;

namespace BankMore.ContaCorrente.API.Application.Queries;

public class ObterSaldoQueryHandler : IRequestHandler<ObterSaldoQuery, SaldoResult>
{
    private readonly IContaCorrenteRepository _contaRepository;
    private readonly IMovimentoRepository _movimentoRepository;

    public ObterSaldoQueryHandler(
        IContaCorrenteRepository contaRepository,
        IMovimentoRepository movimentoRepository)
    {
        _contaRepository = contaRepository;
        _movimentoRepository = movimentoRepository;
    }

    public async Task<SaldoResult> Handle(ObterSaldoQuery request, CancellationToken cancellationToken)
    {
        var conta = await _contaRepository.ObterPorIdAsync(request.Id)
            ?? throw new InvalidOperationException("Conta não encontrada");

        var saldo = await _movimentoRepository.ObterSaldoAsync(request.Id);

        return new SaldoResult(conta.Id, conta.Numero, conta.Nome, saldo);
    }
}
