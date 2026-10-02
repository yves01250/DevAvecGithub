using Microsoft.Data.Sqlite;
using SuiviPortefolio.Data;
using SuiviPortefolio.Transactions.MouvementModel;

namespace SuiviPortefolio.Transactions.MouvementRepository;

public sealed class MouvementRepository : IMouvementRepository
{
    private readonly string _connectionString;

    public MouvementRepository(string dbPath)
    {
        _connectionString = new SqliteConnectionStringBuilder { DataSource = dbPath, Mode = SqliteOpenMode.ReadWriteCreate }.ToString();
        _ = new SqliteConnection(_connectionString);
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS MouvementEspece (
                MouvId INTEGER PRIMARY KEY AUTOINCREMENT,
                MouvType TEXT NOT NULL,
                MouvMontant DECIMAL(18,2) NOT NULL,
                MouvDate TEXT NOT NULL,
                MouvCpteId INTEGER NOT NULL,
                MouvCpteDestId INTEGER,
                MouvDescription TEXT,
                FOREIGN KEY (MouvCpteId) REFERENCES Compte(CpteId),
                FOREIGN KEY (MouvCpteDestId) REFERENCES Compte(CpteId)
            );
            """;
        command.ExecuteNonQuery();

        command.CommandText = """
            CREATE INDEX IF NOT EXISTS IX_MouvementEspece_CpteId_Date 
            ON MouvementEspece(MouvCpteId, MouvDate DESC);
            """;
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<MouvementEspece> GetRecent(int compteId, int count = 10)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT m.MouvId, m.MouvType, m.MouvMontant, m.MouvDate,
                   m.MouvCpteId, m.MouvCpteDestId, m.MouvDescription,
                   c1.CpteNom, c2.CpteNom
            FROM MouvementEspece m
            LEFT JOIN Compte c1 ON c1.CpteId = m.MouvCpteId
            LEFT JOIN Compte c2 ON c2.CpteId = m.MouvCpteDestId
            WHERE m.MouvCpteId = $compteId OR m.MouvCpteDestId = $compteId
            ORDER BY datetime(m.MouvDate) DESC, m.MouvId DESC LIMIT $count;
            """;
        command.Parameters.AddWithValue("$compteId", compteId);
        command.Parameters.AddWithValue("$count", count);

        var result = new List<MouvementEspece>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new MouvementEspece
            {
                MouvId = reader.GetInt64(0),
                MouvType = Enum.Parse<TypeMouvement>(reader.GetString(1)),
                MouvMontant = reader.GetDecimal(2),
                MouvDate = DateTime.Parse(reader.GetString(3)),
                MouvCpteId = reader.GetInt32(4),
                MouvCpteDestId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                MouvDescription = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                CompteNom = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                CompteDestNom = reader.IsDBNull(8) ? null : reader.GetString(8)
            });
        }
        return result;
    }

    public IReadOnlyList<MouvementEspece> GetAllByCompte(int compteId)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT m.MouvId, m.MouvType, m.MouvMontant, m.MouvDate,
                   m.MouvCpteId, m.MouvCpteDestId, m.MouvDescription,
                   c1.CpteNom, c2.CpteNom
            FROM MouvementEspece m
            LEFT JOIN Compte c1 ON c1.CpteId = m.MouvCpteId
            LEFT JOIN Compte c2 ON c2.CpteId = m.MouvCpteDestId
            WHERE m.MouvCpteId = $compteId OR m.MouvCpteDestId = $compteId
            ORDER BY datetime(m.MouvDate) DESC, m.MouvId DESC;
            """;
        command.Parameters.AddWithValue("$compteId", compteId);

        var result = new List<MouvementEspece>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new MouvementEspece
            {
                MouvId = reader.GetInt64(0),
                MouvType = Enum.Parse<TypeMouvement>(reader.GetString(1)),
                MouvMontant = reader.GetDecimal(2),
                MouvDate = DateTime.Parse(reader.GetString(3)),
                MouvCpteId = reader.GetInt32(4),
                MouvCpteDestId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                MouvDescription = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                CompteNom = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                CompteDestNom = reader.IsDBNull(8) ? null : reader.GetString(8)
            });
        }
        return result;
    }

    public void Save(MouvementEspece mouvement)
    {
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;

            // Calculer la variation de solde pour le compte source
            decimal variationSource = mouvement.MouvType switch
            {
                TypeMouvement.Depot => mouvement.MouvMontant,
                TypeMouvement.Retrait => -mouvement.MouvMontant,
                TypeMouvement.Virement => -mouvement.MouvMontant,
                _ => 0m
            };

            if (mouvement.MouvId == 0)
            {
                // Insertion
                command.CommandText = """
                    INSERT INTO MouvementEspece 
                    (MouvType, MouvMontant, MouvDate, MouvCpteId, MouvCpteDestId, MouvDescription)
                    VALUES ($type, $montant, $date, $cpteId, $cpteDestId, $description);
                    SELECT last_insert_rowid();
                    """;
                command.Parameters.AddWithValue("$type", mouvement.MouvType.ToString());
                command.Parameters.AddWithValue("$montant", mouvement.MouvMontant);
                command.Parameters.AddWithValue("$date", mouvement.MouvDate.ToString("O"));
                command.Parameters.AddWithValue("$cpteId", mouvement.MouvCpteId);
                command.Parameters.AddWithValue("$cpteDestId", (object)mouvement.MouvCpteDestId ?? DBNull.Value);
                command.Parameters.AddWithValue("$description", mouvement.MouvDescription ?? string.Empty);

                mouvement.MouvId = Convert.ToInt64(command.ExecuteScalar());
            }
            else
            {
                // Mise à jour : annuler l'ancien impact sur le solde
                var ancienMouvement = GetById(connection, transaction, mouvement.MouvId);
                if (ancienMouvement != null)
                {
                    var ancienneVariation = ancienMouvement.MouvType switch
                    {
                        TypeMouvement.Depot => -ancienMouvement.MouvMontant,
                        TypeMouvement.Retrait => ancienMouvement.MouvMontant,
                        TypeMouvement.Virement => ancienMouvement.MouvMontant,
                        _ => 0m
                    };
                    MettreAJourSoldeCompte(connection, transaction, ancienMouvement.MouvCpteId, ancienneVariation);
                    
                    if (ancienMouvement.MouvCpteDestId.HasValue)
                    {
                        MettreAJourSoldeCompte(connection, transaction, ancienMouvement.MouvCpteDestId.Value, -ancienMouvement.MouvMontant);
                    }
                }

                command.CommandText = """
                    UPDATE MouvementEspece 
                    SET MouvType = $type, MouvMontant = $montant, MouvDate = $date,
                        MouvCpteId = $cpteId, MouvCpteDestId = $cpteDestId, MouvDescription = $description
                    WHERE MouvId = $id;
                    """;
                command.Parameters.AddWithValue("$id", mouvement.MouvId);
                command.Parameters.AddWithValue("$type", mouvement.MouvType.ToString());
                command.Parameters.AddWithValue("$montant", mouvement.MouvMontant);
                command.Parameters.AddWithValue("$date", mouvement.MouvDate.ToString("O"));
                command.Parameters.AddWithValue("$cpteId", mouvement.MouvCpteId);
                command.Parameters.AddWithValue("$cpteDestId", (object)mouvement.MouvCpteDestId ?? DBNull.Value);
                command.Parameters.AddWithValue("$description", mouvement.MouvDescription ?? string.Empty);

                command.ExecuteNonQuery();
            }

            // Appliquer la nouvelle variation
            MettreAJourSoldeCompte(connection, transaction, mouvement.MouvCpteId, variationSource);

            // Pour les virements, créditer le compte destination
            if (mouvement.MouvType == TypeMouvement.Virement && mouvement.MouvCpteDestId.HasValue)
            {
                MettreAJourSoldeCompte(connection, transaction, mouvement.MouvCpteDestId.Value, mouvement.MouvMontant);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void Delete(long mouvementId)
    {
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            var mouvement = GetById(connection, transaction, mouvementId);
            if (mouvement == null)
                throw new InvalidOperationException($"Le mouvement {mouvementId} est introuvable.");

            // Annuler l'impact sur le solde
            var variation = mouvement.MouvType switch
            {
                TypeMouvement.Depot => -mouvement.MouvMontant,
                TypeMouvement.Retrait => mouvement.MouvMontant,
                TypeMouvement.Virement => mouvement.MouvMontant,
                _ => 0m
            };

            MettreAJourSoldeCompte(connection, transaction, mouvement.MouvCpteId, variation);

            if (mouvement.MouvType == TypeMouvement.Virement && mouvement.MouvCpteDestId.HasValue)
            {
                MettreAJourSoldeCompte(connection, transaction, mouvement.MouvCpteDestId.Value, -mouvement.MouvMontant);
            }

            // Supprimer le mouvement
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "DELETE FROM MouvementEspece WHERE MouvId = $id;";
            command.Parameters.AddWithValue("$id", mouvementId);
            command.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public decimal GetSoldeCompte(int compteId)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT CpteSolde FROM Compte WHERE CpteId = $compteId;";
        command.Parameters.AddWithValue("$compteId", compteId);
        
        var result = command.ExecuteScalar();
        if (result == null || result == DBNull.Value)
            throw new InvalidOperationException($"Le compte {compteId} est introuvable.");
        
        return Convert.ToDecimal(result);
    }

    private static void MettreAJourSoldeCompte(
        SqliteConnection connection,
        SqliteTransaction transaction,
        int compteId,
        decimal variation)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            UPDATE Compte
            SET CpteSolde = CpteSolde + $variation
            WHERE CpteId = $compteId;
            """;
        command.Parameters.AddWithValue("$variation", variation);
        command.Parameters.AddWithValue("$compteId", compteId);

        if (command.ExecuteNonQuery() != 1)
            throw new InvalidOperationException($"Le compte {compteId} est introuvable.");
    }

    private static MouvementEspece? GetById(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long mouvementId)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT MouvId, MouvType, MouvMontant, MouvDate, MouvCpteId, MouvCpteDestId, MouvDescription
            FROM MouvementEspece
            WHERE MouvId = $id;
            """;
        command.Parameters.AddWithValue("$id", mouvementId);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
            return null;

        return new MouvementEspece
        {
            MouvId = reader.GetInt64(0),
            MouvType = Enum.Parse<TypeMouvement>(reader.GetString(1)),
            MouvMontant = reader.GetDecimal(2),
            MouvDate = DateTime.Parse(reader.GetString(3)),
            MouvCpteId = reader.GetInt32(4),
            MouvCpteDestId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
            MouvDescription = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
        };
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON;";
        command.ExecuteNonQuery();
        return connection;
    }
}
