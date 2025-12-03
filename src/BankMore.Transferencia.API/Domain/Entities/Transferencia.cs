using BankMore.Transferencia.API.Domain.Enums;

namespace BankMore.Transferencia.API.Domain.Entities;

public class Transferencia
{
    public int Id { get; private set; }
    public string IdRequisicao { get; private set; }
    public int IdContaOrigem { get; private set; }
    public int IdContaDestino { get; private set; }
    public decimal Valor { get; private set; }
    public DateTime DataTransferencia { get; private set; }
    public StatusTransferencia Status { get; private set; }

    public Transferencia(string idRequisicao, int idContaOrigem, int idContaDestino, decimal valor)
    {
        if (valor <= 0)
            throw new ArgumentException("Valor da transferência deve ser positivo");

        if (idContaOrigem == idContaDestino)
            throw new ArgumentException("Conta origem e destino não podem ser iguais");

        IdRequisicao = idRequisicao;
        IdContaOrigem = idContaOrigem;
        IdContaDestino = idContaDestino;
        Valor = valor;
        DataTransferencia = DateTime.UtcNow;
        Status = StatusTransferencia.Processada;
    }

    public void MarcarComoEstornada()
    {
        Status = StatusTransferencia.Estornada;
    }

    public void MarcarComoFalha()
    {
        Status = StatusTransferencia.Falha;
    }
}
