
using Microsoft.Data.Sqlite;
using System.Data;
using SuiviPortefolio.Consultations.ConsultationsModel;

namespace SuiviPortefolio.Data;

public class ConsultPositionRepository
{
    private readonly string _connectionString;

    public ConsultPositionRepository(string dbPath)
    {
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath
        }.ToString();
    }

    public async Task<List<ConsultPosition>> GetAllAsync()
    {
        var list = new List<ConsultPosition>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                p.PosId,
                a.ActifTicker,
                c.CpteType,
                p.PosQte,
                p.PosPrixMoyen,
                a.ActifDateDernierCours
            FROM Position p
            INNER JOIN Actif a ON a.ActifId = p.PosActifId
            INNER JOIN (
                SELECT TransActifId, TransCpteId
                FROM (
                    SELECT
                        TransActifId,
                        TransCpteId,
                        ROW_NUMBER() OVER (
                            PARTITION BY TransActifId
                            ORDER BY TransDateTransac DESC, TransId DESC
                        ) AS RowNumber
                    FROM TransacFin
                )
                WHERE RowNumber = 1
            ) latestTransaction ON latestTransaction.TransActifId = p.PosActifId
            INNER JOIN Compte c ON c.CpteId = latestTransaction.TransCpteId
            ORDER BY a.ActifTicker, c.CpteType";

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.Default);
        while (await reader.ReadAsync())
        {
            list.Add(new ConsultPosition
            {
                Id = reader.GetInt32(0),
                Ticker = reader.GetString(1),
                Type = reader.GetString(2),
                Quantite = reader.GetDecimal(3),
                PrixMoyen = reader.GetDecimal(4),
                Date = DateTime.Parse(reader.GetString(5))
            });
        }

        return list;
    }

    public async Task UpdatePriceAsync(int id, decimal newPrice, DateTime date)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Actif
            SET ActifCoursActuel = $price, ActifDateDernierCours = $date
            WHERE ActifId = (
                SELECT PosActifId
                FROM Position
                WHERE PosId = $id
            )";

        command.Parameters.AddWithValue("$price", newPrice);
        command.Parameters.AddWithValue("$date", date);
        command.Parameters.AddWithValue("$id", id);

        await command.ExecuteNonQueryAsync();
    }
}