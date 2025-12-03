using BankMore.ContaCorrente.API.Domain.ValueObjects;

namespace BankMore.ContaCorrente.API.Domain.Entities;

public class ContaCorrente
{
    public int Id { get; set; }
    public int Numero { get; private set; }
    public string Nome { get; private set; }
    public CPF Cpf { get; private set; }
    public string Senha { get; private set; }
    public bool Ativo { get; private set; }

    private ContaCorrente() 
    { 
        Cpf = null!;
        Nome = null!;
        Senha = null!;
    }

    public ContaCorrente(int id, int numero, string nome, CPF cpf, string senha, bool ativo)
    {
        Id = id;
        Numero = numero;
        Nome = nome;
        Cpf = cpf;
        Senha = senha;
        Ativo = ativo;
    }

    public static ContaCorrente Criar(int numero, string nome, CPF cpf, string senha)
    {
        var senhaHash = BCrypt.Net.BCrypt.HashPassword(senha);
        
        return new ContaCorrente
        {
            Numero = numero,
            Cpf = cpf ?? throw new ArgumentNullException(nameof(cpf)),
            Nome = nome ?? throw new ArgumentNullException(nameof(nome)),
            Ativo = true,
            Senha = senhaHash
        };
    }

    public void Inativar()
    {
        Ativo = false;
    }

    public bool ValidarSenha(string senha)
    {
        return BCrypt.Net.BCrypt.Verify(senha, Senha);
    }
}
