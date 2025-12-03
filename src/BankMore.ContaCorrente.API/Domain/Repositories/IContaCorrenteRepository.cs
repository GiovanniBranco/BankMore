namespace BankMore.ContaCorrente.API.Domain.Repositories;

public interface IContaCorrenteRepository
{
    Task<Entities.ContaCorrente?> ObterPorIdAsync(int id);
    Task<Entities.ContaCorrente?> ObterPorCpfAsync(string cpf);
    Task<Entities.ContaCorrente?> ObterPorNumeroAsync(int numero);
    Task<int> GerarNumeroContaUnicoAsync();
    Task AdicionarAsync(Entities.ContaCorrente conta);
    Task AtualizarAsync(Entities.ContaCorrente conta);
    Task<bool> ExisteCpfAsync(string cpf);
    Task<bool> ExisteNumeroContaAsync(int numero);
}
