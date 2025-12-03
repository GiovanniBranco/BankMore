using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using BankMore.ContaCorrente.API.Domain.Repositories;
using BankMore.ContaCorrente.API.Domain.ValueObjects;
using ContaCorrenteEntity = BankMore.ContaCorrente.API.Domain.Entities.ContaCorrente;

namespace BankMore.ContaCorrente.API.Infrastructure.Repositories;

public class ContaCorrenteRepository : IContaCorrenteRepository
{
    private readonly string _connectionString;
    private const int MaxRetryAttempts = 3;

    public ContaCorrenteRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException(nameof(configuration), "String de conexão não pode ser nula");
    }

    private IDbConnection CreateConnection() => new SqliteConnection(_connectionString);

    public async Task<ContaCorrenteEntity?> ObterPorIdAsync(int id)
    {
        using var connection = CreateConnection();
        const string sql = @"
            SELECT idcontacorrente, numero, nome, cpf, senha, ativo 
            FROM contacorrente 
            WHERE idcontacorrente = @Id";
        
        var data = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Id = id });
        
        return data == null ? null : MapToDomain(data);
    }

    public async Task<ContaCorrenteEntity?> ObterPorCpfAsync(string cpf)
    {
        using var connection = CreateConnection();
        const string sql = @"
            SELECT idcontacorrente, numero, nome, cpf, senha, ativo 
            FROM contacorrente 
            WHERE cpf = @Cpf";
        
        var data = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Cpf = cpf });
        
        return data == null ? null : MapToDomain(data);
    }

    public async Task<ContaCorrenteEntity?> ObterPorNumeroAsync(int numero)
    {
        using var connection = CreateConnection();
        const string sql = @"
            SELECT idcontacorrente, numero, nome, cpf, senha, ativo 
            FROM contacorrente 
            WHERE numero = @Numero";
        
        var data = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Numero = numero });
        
        return data == null ? null : MapToDomain(data);
    }

    public async Task<int> GerarNumeroContaUnicoAsync()
    {
        var random = new Random();
        
        for (int attempt = 0; attempt < MaxRetryAttempts; attempt++)
        {
            var numeroGerado = random.Next(100000000, 999999999);
            
            if (!await ExisteNumeroContaAsync(numeroGerado))
                return numeroGerado;
        }
        
        throw new InvalidOperationException("Falha ao gerar número de conta único após o número máximo de tentativas");
    }

    public async Task<bool> ExisteNumeroContaAsync(int numero)
    {
        using var connection = CreateConnection();
        const string sql = "SELECT COUNT(1) FROM contacorrente WHERE numero = @Numero";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Numero = numero });
        return count > 0;
    }

    public async Task AdicionarAsync(ContaCorrenteEntity conta)
    {
        if (conta == null)
            throw new ArgumentNullException(nameof(conta));

        using var connection = CreateConnection();
        const string sql = @"
            INSERT INTO contacorrente (numero, nome, cpf, senha, ativo) 
            VALUES (@Numero, @Nome, @Cpf, @Senha, @Ativo);
            SELECT last_insert_rowid();";
        
        var id = await connection.ExecuteScalarAsync<int>(sql, new
        {
            conta.Numero,
            conta.Nome,
            Cpf = conta.Cpf.Valor,
            conta.Senha,
            Ativo = conta.Ativo ? 1 : 0
        });
        
        conta.Id = id;
    }

    public async Task AtualizarAsync(ContaCorrenteEntity conta)
    {
        if (conta == null)
            throw new ArgumentNullException(nameof(conta));

        using var connection = CreateConnection();
        const string sql = @"
            UPDATE contacorrente 
            SET nome = @Nome, senha = @Senha, ativo = @Ativo 
            WHERE idcontacorrente = @Id";
        
        await connection.ExecuteAsync(sql, new
        {
            Id = conta.Id,
            conta.Nome,
            conta.Senha,
            Ativo = conta.Ativo ? 1 : 0
        });
    }

    public async Task<bool> ExisteCpfAsync(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            throw new ArgumentException("CPF não pode ser nulo ou vazio", nameof(cpf));

        using var connection = CreateConnection();
        const string sql = "SELECT COUNT(1) FROM contacorrente WHERE cpf = @Cpf";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Cpf = cpf });
        return count > 0;
    }

    private static ContaCorrenteEntity MapToDomain(dynamic data)
    {
        return new ContaCorrenteEntity(
            (int)data.idcontacorrente,
            (int)data.numero,
            (string)data.nome,
            new CPF((string)data.cpf),
            (string)data.senha,
            Convert.ToBoolean(data.ativo)
        );
    }
}
