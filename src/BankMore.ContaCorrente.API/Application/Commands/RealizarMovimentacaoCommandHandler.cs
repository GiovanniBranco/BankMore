using MediatR;
using BankMore.ContaCorrente.API.Domain.Entities;
using BankMore.ContaCorrente.API.Domain.Repositories;
using BankMore.ContaCorrente.API.Domain.Exceptions;

namespace BankMore.ContaCorrente.API.Application.Commands;

public class RealizarMovimentacaoCommandHandler : IRequestHandler<RealizarMovimentacaoCommand, MovimentacaoResult>
{
    private readonly IContaCorrenteRepository _contaRepository;
    private readonly IMovimentoRepository _movimentoRepository;

    public RealizarMovimentacaoCommandHandler(
        IContaCorrenteRepository contaRepository,
        IMovimentoRepository movimentoRepository)
    {
        _contaRepository = contaRepository;
        _movimentoRepository = movimentoRepository;
    }

    public async Task<MovimentacaoResult> Handle(RealizarMovimentacaoCommand request, CancellationToken cancellationToken)
    {
        if (request.Valor <= 0)
            throw new InvalidValueException("Valor deve ser positivo");

        if (string.IsNullOrWhiteSpace(request.Tipo) || request.Tipo.Length != 1)
            throw new InvalidTypeException("Tipo deve ser 'C' (crédito) ou 'D' (débito)");
        
        char tipo = char.ToUpper(request.Tipo[0]);
        if (tipo != 'C' && tipo != 'D')
            throw new InvalidTypeException("Tipo deve ser 'C' (crédito) ou 'D' (débito)");
        
        if (!string.IsNullOrWhiteSpace(request.IdRequisicao))
        {
            var requisicaoExistente = await _movimentoRepository.ObterPorIdRequisicaoAsync(request.IdRequisicao);
            if (requisicaoExistente != null)
            {
                var saldoExistente = await _movimentoRepository.ObterSaldoAsync(request.IdConta);
                throw new DuplicateRequestException(
                    "Requisição já processada anteriormente",
                    new MovimentacaoResult(
                        requisicaoExistente.Id,
                        requisicaoExistente.IdContaCorrente,
                        requisicaoExistente.DataMovimento,
                        requisicaoExistente.Tipo,
                        requisicaoExistente.Valor,
                        saldoExistente
                    )
                );
            }
        }

        var conta = await _contaRepository.ObterPorIdAsync(request.IdConta)
            ?? throw new InvalidOperationException("Conta não encontrada");

        if (!conta.Ativo)
            throw new InactiveAccountException("Conta inativa");

        if (tipo == 'D')
        {
            var saldoAtual = await _movimentoRepository.ObterSaldoAsync(request.IdConta);
            
            if (saldoAtual < request.Valor)
                throw new InsufficientBalanceException("Saldo insuficiente");
        }

        var movimento = Movimento.Criar(request.IdConta, tipo, request.Valor);
        await _movimentoRepository.AdicionarAsync(movimento, request.IdRequisicao);

        var novoSaldo = await _movimentoRepository.ObterSaldoAsync(request.IdConta);

        return new MovimentacaoResult(
            movimento.Id,
            movimento.IdContaCorrente,
            movimento.DataMovimento,
            movimento.Tipo,
            movimento.Valor,
            novoSaldo
        );
    }
}
