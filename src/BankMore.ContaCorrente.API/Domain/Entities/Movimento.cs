namespace BankMore.ContaCorrente.API.Domain.Entities;

public class Movimento
{
    public int Id { get; set; }
    public int IdContaCorrente { get; private set; }
    public DateTime DataMovimento { get; private set; }
    public char Tipo { get; private set; }
    public decimal Valor { get; private set; }

    private Movimento() { }

    public Movimento(int id, int idContaCorrente, DateTime dataMovimento, char tipo, decimal valor)
    {
        Id = id;
        IdContaCorrente = idContaCorrente;
        DataMovimento = dataMovimento;
        Tipo = tipo;
        Valor = valor;
    }

    public static Movimento Criar(int idContaCorrente, char tipo, decimal valor)
    {
        if (tipo != 'C' && tipo != 'D')
            throw new ArgumentException("Tipo de movimento inválido. Use 'C' para crédito ou 'D' para débito.");

        if (valor <= 0)
            throw new ArgumentException("Valor deve ser positivo.");

        return new Movimento
        {
            IdContaCorrente = idContaCorrente,
            DataMovimento = DateTime.UtcNow,
            Tipo = tipo,
            Valor = valor
        };
    }

    public bool IsCredito() => Tipo == 'C';
    public bool IsDebito() => Tipo == 'D';
}
