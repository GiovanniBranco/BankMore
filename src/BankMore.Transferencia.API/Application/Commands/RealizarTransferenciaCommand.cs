using MediatR;

namespace BankMore.Transferencia.API.Application.Commands;

public record RealizarTransferenciaCommand(
    string IdRequisicao,
    int IdContaOrigem,
    int IdContaDestino,
    decimal Valor
) : IRequest<int>;
