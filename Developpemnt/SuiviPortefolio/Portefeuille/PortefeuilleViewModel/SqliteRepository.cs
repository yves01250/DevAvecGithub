/*
using Microsoft.Data.Sqlite;

namespace SuiviPortefolio.Portefeuille.PortefeuilleViewModel;

public class SqliteRepository
{
    private readonly string _connectionString;

    public SqliteRepository(string dbPath)
    {
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();

        Initialize();
    }

    public void Initialize()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS TickerAnalyses (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Ticker TEXT NOT NULL,
                Period TEXT NOT NULL,
                Date TEXT,
                Close REAL,
                NormalizedValue REAL,
                PerformancePct REAL,
                CreatedAt TEXT NOT NULL
            );
        ";
        command.ExecuteNonQuery();
    }

}  
*/
