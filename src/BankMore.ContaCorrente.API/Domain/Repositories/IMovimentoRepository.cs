using BankMore.ContaCorrente.API.Domain.Entities;

namespace BankMore.ContaCorrente.API.Domain.Repositories;

public interface IMovimentoRepository
{
    Task AdicionarAsync(Movimento movimento, string? idRequisicao = null);
    Task<decimal> ObterSaldoAsync(int idContaCorrente);
    Task<IEnumerable<Movimento>> ObterPorContaAsync(int idContaCorrente);
    Task<Movimento?> ObterPorIdRequisicaoAsync(string idRequisicao);
}
