using System.ComponentModel.DataAnnotations;

namespace BankMore.Transferencia.API.DTOs;

public record RealizarTransferenciaRequest(
    [Required] string IdRequisicao,
    [Required] int IdContaDestino,
    [Required][Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")] decimal Valor
);

public record ErrorResponse(
    string Message,
    string Type
);

public record TransferenciaResponse(
    int Id,
    string IdRequisicao,
    int IdContaOrigem,
    int IdContaDestino,
    decimal Valor,
    DateTime DataTransferencia,
    string Status
);
