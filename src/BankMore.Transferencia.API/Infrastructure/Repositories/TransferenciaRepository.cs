using System.Data.SQLite;
using Dapper;
using BankMore.Transferencia.API.Domain.Repositories;
using BankMore.Transferencia.API.Domain.Enums;
using TransferenciaEntity = BankMore.Transferencia.API.Domain.Entities.Transferencia;

namespace BankMore.Transferencia.API.Infrastructure.Repositories;

public class TransferenciaRepository : ITransferenciaRepository
{
    private readonly string _connectionString;

    public TransferenciaRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task InicializarBancoDeDadosAsync()
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            CREATE TABLE IF NOT EXISTS transferencia (
                idtransferencia INTEGER PRIMARY KEY AUTOINCREMENT,
                idrequisicao TEXT NOT NULL UNIQUE,
                idcontaorigem INTEGER NOT NULL,
                idcontadestino INTEGER NOT NULL,
                valor DECIMAL(18, 2) NOT NULL,
                datatransferencia TEXT NOT NULL,
                status TEXT NOT NULL CHECK(status IN ('Processada', 'Estornada', 'Falha'))
            );

            CREATE INDEX IF NOT EXISTS idx_transferencia_idrequisicao ON transferencia(idrequisicao);
            CREATE INDEX IF NOT EXISTS idx_transferencia_origem ON transferencia(idcontaorigem);
            CREATE INDEX IF NOT EXISTS idx_transferencia_destino ON transferencia(idcontadestino);
            CREATE INDEX IF NOT EXISTS idx_transferencia_data ON transferencia(datatransferencia);
        ";

        await connection.ExecuteAsync(sql);
    }

    public async Task<TransferenciaEntity?> ObterPorIdRequisicaoAsync(string idRequisicao)
    {
        using var connection = new SQLiteConnection(_connectionString);
        
        var sql = @"
            SELECT 
                idtransferencia as Id,
                idrequisicao as IdRequisicao,
                idcontaorigem as IdContaOrigem,
                idcontadestino as IdContaDestino,
                valor as Valor,
                datatransferencia as DataTransferencia,
                status as Status
            FROM transferencia
            WHERE idrequisicao = @IdRequisicao
        ";

        var result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { IdRequisicao = idRequisicao });
        
        return result != null ? MapToDomain(result) : null;
    }

    public async Task<TransferenciaEntity?> ObterPorIdAsync(int id)
    {
        using var connection = new SQLiteConnection(_connectionString);
        
        var sql = @"
            SELECT 
                idtransferencia as Id,
                idrequisicao as IdRequisicao,
                idcontaorigem as IdContaOrigem,
                idcontadestino as IdContaDestino,
                valor as Valor,
                datatransferencia as DataTransferencia,
                status as Status
            FROM transferencia
            WHERE idtransferencia = @Id
        ";

        var result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = id });
        
        return result != null ? MapToDomain(result) : null;
    }

    public async Task<int> AdicionarAsync(TransferenciaEntity transferencia)
    {
        using var connection = new SQLiteConnection(_connectionString);
        
        var sql = @"
            INSERT INTO transferencia (idrequisicao, idcontaorigem, idcontadestino, valor, datatransferencia, status)
            VALUES (@IdRequisicao, @IdContaOrigem, @IdContaDestino, @Valor, @DataTransferencia, @Status);
            
            SELECT last_insert_rowid();
        ";

        var id = await connection.ExecuteScalarAsync<int>(sql, new
        {
            IdRequisicao = transferencia.IdRequisicao,
            IdContaOrigem = transferencia.IdContaOrigem,
            IdContaDestino = transferencia.IdContaDestino,
            Valor = transferencia.Valor,
            DataTransferencia = transferencia.DataTransferencia.ToString("o"),
            Status = transferencia.Status.ToString()
        });

        return id;
    }

    public async Task AtualizarStatusAsync(int id, StatusTransferencia status)
    {
        using var connection = new SQLiteConnection(_connectionString);
        
        var sql = @"
            UPDATE transferencia
            SET status = @Status
            WHERE idtransferencia = @Id
        ";

        await connection.ExecuteAsync(sql, new { Id = id, Status = status.ToString() });
    }

    private static TransferenciaEntity MapToDomain(dynamic data)
    {
        var transferencia = new TransferenciaEntity(
            (string)data.IdRequisicao,
            (int)(long)data.IdContaOrigem,
            (int)(long)data.IdContaDestino,
            (decimal)(double)data.Valor
        );

        typeof(TransferenciaEntity)
            .GetProperty("Id")!
            .SetValue(transferencia, (int)(long)data.Id);

        typeof(TransferenciaEntity)
            .GetProperty("DataTransferencia")!
            .SetValue(transferencia, DateTime.Parse((string)data.DataTransferencia));

        var status = Enum.Parse<StatusTransferencia>((string)data.Status);
        if (status != StatusTransferencia.Processada)
        {
            typeof(TransferenciaEntity)
                .GetProperty("Status")!
                .SetValue(transferencia, status);
        }

        return transferencia;
    }
}
