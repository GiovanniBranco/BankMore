using System.Text.RegularExpressions;
using BankMore.ContaCorrente.API.Domain.Exceptions;

namespace BankMore.ContaCorrente.API.Domain.ValueObjects;

public class CPF
{
    public string Valor { get; private set; }

    public CPF(string numero)
    {
        if (!Validar(numero))
            throw new InvalidDocumentException("CPF inválido");

        Valor = Regex.Replace(numero, @"[^\d]", "");
    }

    public static CPF Criar(string cpf)
    {
        if (!Validar(cpf))
            throw new InvalidDocumentException("CPF inválido");

        var cpfLimpo = Regex.Replace(cpf, @"[^\d]", "");
        return new CPF(cpfLimpo);
    }

    public static bool Validar(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        cpf = Regex.Replace(cpf, @"[^\d]", "");

        if (cpf.Length != 11)
            return false;


        if (cpf.Distinct().Count() == 1)
            return false;

        int[] multiplicador1 = [10, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] multiplicador2 = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];

        string tempCpf = cpf.Substring(0, 9);
        int soma = 0;

        for (int i = 0; i < 9; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

        int resto = soma % 11;
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;

        string digito = resto.ToString();
        tempCpf += digito;
        soma = 0;

        for (int i = 0; i < 10; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

        resto = soma % 11;
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;

        digito += resto.ToString();

        return cpf.EndsWith(digito);
    }

    public override string ToString() => Valor;
}
