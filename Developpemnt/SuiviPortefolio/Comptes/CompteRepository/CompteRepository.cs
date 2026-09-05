using Microsoft.Data.Sqlite;
using SuiviPortefolio.Comptes.CompteModel;

namespace SuiviPortefolio.Comptes.CompteRepository;

public class CompteRepository : ICompteRepository
{
    private readonly string _connectionString;

    public CompteRepository(string dbPath)
    {
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Compte (
                CpteId INTEGER PRIMARY KEY AUTOINCREMENT,
                CpteNom TEXT NOT NULL,
                CpteType TEXT NOT NULL,
                CpteDevise TEXT NOT NULL,
                CpteSolde DECIMAL(18, 2) NOT NULL DEFAULT 0,
                CpteEstDefaut INTEGER NOT NULL DEFAULT 0,
                CptePtfId INTEGER NOT NULL DEFAULT 1
            );
        ";
        command.ExecuteNonQuery();
    }

    public List<Compte> GetAll()
    {
        var result = new List<Compte>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT CpteId, CpteNom, CpteType, CpteDevise, CpteSolde, CpteEstDefaut, CptePtfId FROM Compte";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Compte
            {
                CpteId = reader.GetInt32(0),
                CpteNom = reader.GetString(1),
                CpteType = Enum.Parse<TypeCompte>(reader.GetString(2)),
                CpteDevise = reader.GetString(3),
                CpteSolde = reader.GetDecimal(4),
                CpteEstDefaut = reader.GetBoolean(5),
                CptePtfId = reader.GetInt32(6)
            });
        }

        return result;
    }

    public int Add(Compte compte)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Compte (CpteNom, CpteType, CpteDevise, CpteSolde, CpteEstDefaut, CptePtfId)
            VALUES ($nom, $type, $devise, $solde, $estDefaut, $ptfId);
            SELECT last_insert_rowid();
        ";

        command.Parameters.AddWithValue("$nom", compte.CpteNom);
        command.Parameters.AddWithValue("$type", compte.CpteType.ToString());
        command.Parameters.AddWithValue("$devise", compte.CpteDevise);
        command.Parameters.AddWithValue("$solde", compte.CpteSolde);
        command.Parameters.AddWithValue("$estDefaut", compte.CpteEstDefaut ? 1 : 0);
        command.Parameters.AddWithValue("$ptfId", compte.CptePtfId);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void Update(Compte compte)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Compte
            SET CpteNom = $nom,
                CpteType = $type,
                CpteDevise = $devise,
                CpteSolde = $solde,
                CpteEstDefaut = $estDefaut,
                CptePtfId = $ptfId
            WHERE CpteId = $id;
        ";

        command.Parameters.AddWithValue("$id", compte.CpteId);
        command.Parameters.AddWithValue("$nom", compte.CpteNom);
        command.Parameters.AddWithValue("$type", compte.CpteType.ToString());
        command.Parameters.AddWithValue("$devise", compte.CpteDevise);
        command.Parameters.AddWithValue("$solde", compte.CpteSolde);
        command.Parameters.AddWithValue("$estDefaut", compte.CpteEstDefaut ? 1 : 0);
        command.Parameters.AddWithValue("$ptfId", compte.CptePtfId);

        command.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Compte WHERE CpteId = $id;";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    public void SetDefault(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        using (var cmdReset = connection.CreateCommand())
        {
            cmdReset.Transaction = transaction;
            cmdReset.CommandText = "UPDATE Compte SET CpteEstDefaut = 0;";
            cmdReset.ExecuteNonQuery();
        }

        using (var cmdSetDefault = connection.CreateCommand())
        {
            cmdSetDefault.Transaction = transaction;
            cmdSetDefault.CommandText = "UPDATE Compte SET CpteEstDefaut = 1 WHERE CpteId = $id;";
            cmdSetDefault.Parameters.AddWithValue("$id", id);
            cmdSetDefault.ExecuteNonQuery();
        }

        transaction.Commit();
    }
}
