using MediatR;
using BankMore.Transferencia.API.Domain.Repositories;
using BankMore.Transferencia.API.Infrastructure.Services;
using BankMore.Transferencia.API.Domain.Enums;
using TransferenciaEntity = BankMore.Transferencia.API.Domain.Entities.Transferencia;

namespace BankMore.Transferencia.API.Application.Commands;

public class RealizarTransferenciaCommandHandler : IRequestHandler<RealizarTransferenciaCommand, int>
{
    private readonly ITransferenciaRepository _repository;
    private readonly IContaCorrenteApiService _contaCorrenteApi;

    public RealizarTransferenciaCommandHandler(
        ITransferenciaRepository repository,
        IContaCorrenteApiService contaCorrenteApi)
    {
        _repository = repository;
        _contaCorrenteApi = contaCorrenteApi;
    }

    public async Task<int> Handle(RealizarTransferenciaCommand request, CancellationToken cancellationToken)
    {
        var transferenciaExistente = await _repository.ObterPorIdRequisicaoAsync(request.IdRequisicao);
        if (transferenciaExistente != null)
        {
            return transferenciaExistente.Id;
        }

        var transferencia = new TransferenciaEntity(
            request.IdRequisicao,
            request.IdContaOrigem,
            request.IdContaDestino,
            request.Valor
        );

        var debitoRealizado = await _contaCorrenteApi.RealizarDebitoAsync(
            request.IdContaOrigem,
            request.Valor,
            $"debito-{request.IdRequisicao}"
        );

        if (!debitoRealizado)
        {
            throw new InvalidOperationException("Falha ao realizar débito na conta origem");
        }

        var creditoRealizado = await _contaCorrenteApi.RealizarCreditoAsync(
            request.IdContaDestino,
            request.Valor,
            $"credito-{request.IdRequisicao}"
        );

        if (!creditoRealizado)
        {
            await _contaCorrenteApi.RealizarCreditoAsync(
                request.IdContaOrigem,
                request.Valor,
                $"estorno-{request.IdRequisicao}"
            );

            transferencia.MarcarComoEstornada();
            await _repository.AdicionarAsync(transferencia);

            throw new InvalidOperationException("Falha ao realizar crédito na conta destino");
        }

        var id = await _repository.AdicionarAsync(transferencia);
        return id;
    }
}
