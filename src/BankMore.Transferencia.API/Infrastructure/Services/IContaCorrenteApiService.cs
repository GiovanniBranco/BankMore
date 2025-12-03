namespace BankMore.Transferencia.API.Infrastructure.Services;

public interface IContaCorrenteApiService
{
    Task<bool> RealizarDebitoAsync(int idConta, decimal valor, string idRequisicao);
    Task<bool> RealizarCreditoAsync(int idConta, decimal valor, string idRequisicao);
}
