using BankMore.Transferencia.API.Domain.Entities;
using BankMore.Transferencia.API.Domain.Enums;

namespace BankMore.Transferencia.API.Domain.Repositories;

public interface ITransferenciaRepository
{
    Task<Entities.Transferencia?> ObterPorIdRequisicaoAsync(string idRequisicao);
    Task<Entities.Transferencia?> ObterPorIdAsync(int id);
    Task<int> AdicionarAsync(Entities.Transferencia transferencia);
    Task AtualizarStatusAsync(int id, StatusTransferencia status);
}
