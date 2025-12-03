using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using BankMore.ContaCorrente.API.Domain.Entities;
using BankMore.ContaCorrente.API.Domain.Repositories;

namespace BankMore.ContaCorrente.API.Infrastructure.Repositories;

public class MovimentoRepository : IMovimentoRepository
{
    private readonly string _connectionString;

    public MovimentoRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException(nameof(configuration), "String de conexão não pode ser nula");
    }

    private IDbConnection CreateConnection() => new SqliteConnection(_connectionString);

    public async Task AdicionarAsync(Movimento movimento, string? idRequisicao = null)
    {
        if (movimento == null)
            throw new ArgumentNullException(nameof(movimento));

        using var connection = CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string sqlMovimento = @"
                INSERT INTO movimento (idcontacorrente, datamovimento, tipo, valor) 
                VALUES (@IdContaCorrente, @DataMovimento, @Tipo, @Valor);
                SELECT last_insert_rowid();";
            
            var id = await connection.ExecuteScalarAsync<int>(sqlMovimento, new
            {
                movimento.IdContaCorrente,
                DataMovimento = movimento.DataMovimento.ToString("yyyy-MM-dd HH:mm:ss"),
                Tipo = movimento.Tipo.ToString(),
                movimento.Valor
            }, transaction);
            
            movimento.Id = id;

            if (!string.IsNullOrWhiteSpace(idRequisicao))
            {
                const string sqlIdempotencia = @"
                    INSERT INTO idempotencia (idrequisicao, idmovimento, datacriacao) 
                    VALUES (@IdRequisicao, @IdMovimento, @DataCriacao)";
                
                await connection.ExecuteAsync(sqlIdempotencia, new
                {
                    IdRequisicao = idRequisicao,
                    IdMovimento = movimento.Id,
                    DataCriacao = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                }, transaction);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<Movimento?> ObterPorIdRequisicaoAsync(string idRequisicao)
    {
        if (string.IsNullOrWhiteSpace(idRequisicao))
            throw new ArgumentException("ID de requisição não pode ser nulo ou vazio", nameof(idRequisicao));

        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT m.idmovimento, m.idcontacorrente, m.datamovimento, m.tipo, m.valor 
            FROM movimento m
            INNER JOIN idempotencia i ON i.idmovimento = m.idmovimento
            WHERE i.idrequisicao = @IdRequisicao
            LIMIT 1";
        
        var data = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { IdRequisicao = idRequisicao });
        
        return data == null ? null : MapToDomain(data);
    }

    public async Task<decimal> ObterSaldoAsync(int idContaCorrente)
    {
        using var connection = CreateConnection();
        const string sql = @"
            SELECT COALESCE(SUM(CASE WHEN tipo = 'C' THEN valor ELSE -valor END), 0)
            FROM movimento 
            WHERE idcontacorrente = @IdContaCorrente";
        
        return await connection.ExecuteScalarAsync<decimal>(sql, new { IdContaCorrente = idContaCorrente });
    }

    public async Task<IEnumerable<Movimento>> ObterPorContaAsync(int idContaCorrente)
    {
        using var connection = CreateConnection();
        const string sql = @"
            SELECT idmovimento, idcontacorrente, datamovimento, tipo, valor 
            FROM movimento 
            WHERE idcontacorrente = @IdContaCorrente 
            ORDER BY datamovimento DESC";
        
        var data = await connection.QueryAsync<dynamic>(sql, new { IdContaCorrente = idContaCorrente });
        
        return data.Select(MapToDomain);
    }

    private static Movimento MapToDomain(dynamic data)
    {
        return new Movimento(
            (int)data.idmovimento,
            (int)data.idcontacorrente,
            DateTime.Parse((string)data.datamovimento),
            ((string)data.tipo)[0],
            (decimal)Convert.ToDouble(data.valor)
        );
    }
}
