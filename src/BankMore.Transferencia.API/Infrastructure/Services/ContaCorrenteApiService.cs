using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace BankMore.Transferencia.API.Infrastructure.Services;

public class ContaCorrenteApiService : IContaCorrenteApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ContaCorrenteApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> RealizarDebitoAsync(int idConta, decimal valor, string idRequisicao)
    {
        return await RealizarMovimentacaoAsync(idConta, valor, 'D', idRequisicao);
    }

    public async Task<bool> RealizarCreditoAsync(int idConta, decimal valor, string idRequisicao)
    {
        return await RealizarMovimentacaoAsync(idConta, valor, 'C', idRequisicao);
    }

    private async Task<bool> RealizarMovimentacaoAsync(int idConta, decimal valor, char tipo, string idRequisicao)
    {
        try
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"/api/contacorrente/{idConta}/movimentacao");
            
            if (!string.IsNullOrEmpty(token))
            {
                requestMessage.Headers.Add("Authorization", token);
            }

            var requestBody = new
            {
                IdRequisicao = idRequisicao,
                Valor = valor,
                Tipo = tipo
            };

            requestMessage.Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.SendAsync(requestMessage);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
