using MediatR;

namespace BankMore.ContaCorrente.API.Application.Queries;

public record ObterExtratoQuery(int IdConta) : IRequest<ExtratoResult>;

public record ExtratoResult(int IdConta, int Numero, string Nome, decimal SaldoAtual, List<MovimentoItem> Movimentos);

public record MovimentoItem(int Id, DateTime DataMovimento, char Tipo, decimal Valor, string Descricao);
