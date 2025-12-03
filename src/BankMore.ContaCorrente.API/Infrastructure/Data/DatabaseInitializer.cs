using Microsoft.Data.Sqlite;

namespace BankMore.ContaCorrente.API.Infrastructure.Data;

public static class DatabaseInitializer
{
    public static void Initialize(string connectionString)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var createTableCommand = connection.CreateCommand();
        createTableCommand.CommandText = @"
            CREATE TABLE IF NOT EXISTS contacorrente (
                idcontacorrente INTEGER PRIMARY KEY AUTOINCREMENT,
                numero INTEGER NOT NULL UNIQUE,
                nome TEXT(200) NOT NULL,
                cpf TEXT(11) NOT NULL UNIQUE,
                senha TEXT(255) NOT NULL,
                ativo INTEGER DEFAULT 1
            );

            CREATE TABLE IF NOT EXISTS movimento (
                idmovimento INTEGER PRIMARY KEY AUTOINCREMENT,
                idcontacorrente INTEGER NOT NULL,
                datamovimento TEXT NOT NULL,
                tipo TEXT(1) NOT NULL CHECK(tipo IN ('C','D')),
                valor REAL NOT NULL CHECK(valor > 0),
                FOREIGN KEY(idcontacorrente) REFERENCES contacorrente(idcontacorrente)
            );

            CREATE TABLE IF NOT EXISTS idempotencia (
                idrequisicao TEXT PRIMARY KEY,
                idmovimento INTEGER NOT NULL,
                datacriacao TEXT NOT NULL,
                FOREIGN KEY(idmovimento) REFERENCES movimento(idmovimento)
            );

            CREATE INDEX IF NOT EXISTS idx_contacorrente_cpf ON contacorrente(cpf);
            CREATE INDEX IF NOT EXISTS idx_contacorrente_numero ON contacorrente(numero);
            CREATE INDEX IF NOT EXISTS idx_movimento_idcontacorrente ON movimento(idcontacorrente);
            CREATE INDEX IF NOT EXISTS idx_idempotencia_idrequisicao ON idempotencia(idrequisicao);
        ";

        createTableCommand.ExecuteNonQuery();
    }
}
