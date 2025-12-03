using MediatR;

namespace BankMore.ContaCorrente.API.Application.Commands;

public record RealizarMovimentacaoCommand(string? IdRequisicao, int IdConta, string Tipo, decimal Valor) : IRequest<MovimentacaoResult>;

public record MovimentacaoResult(int Id, int IdConta, DateTime DataMovimento, char Tipo, decimal Valor, decimal NovoSaldo);
