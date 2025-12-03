using MediatR;

namespace BankMore.ContaCorrente.API.Application.Queries;

public record ObterSaldoQuery(int Id) : IRequest<SaldoResult>;

public record SaldoResult(int Id, int Numero, string Nome, decimal Saldo);
