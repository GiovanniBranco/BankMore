namespace BankMore.ContaCorrente.API.DTOs;

public record CriarContaRequest(string Nome, string Cpf, string Senha);

public record CriarContaResponse(int Id, int Numero, string Nome, string Cpf);

public class LoginRequest
{
    public string? Cpf { get; set; }
    public string? CpfOuNumeroConta { get; set; }
    public string Senha { get; set; } = string.Empty;
    
    public string GetIdentificador() => CpfOuNumeroConta ?? Cpf ?? string.Empty;
}

public record LoginResponse(string Token, int Id, int Numero, string Nome);

public record SaldoResponse(int Id, int Numero, string Nome, decimal Saldo);

public record InativarContaRequest(string Senha);

public record MovimentacaoRequest(string? IdRequisicao, string Tipo, decimal Valor);

public record MovimentacaoResponse(int Id, int IdConta, DateTime DataMovimento, char Tipo, decimal Valor, decimal NovoSaldo);

public record ExtratoResponse(int IdConta, int Numero, string Nome, decimal SaldoAtual, List<MovimentoItemDto> Movimentos);

public record MovimentoItemDto(int Id, DateTime DataMovimento, char Tipo, decimal Valor, string Descricao);

public record ErrorResponse(string Message, string? Type = null, string? Detail = null);
