using Microsoft.Data.Sqlite;
using SuiviPortefolio.Data;
using SuiviPortefolio.Transactions.TransModel;

namespace SuiviPortefolio.Transactions.TransRepository;

public sealed class TransRepository : ITransRepository
{
    private readonly string _connectionString;

    public TransRepository(string dbPath)
    {
        _connectionString = new SqliteConnectionStringBuilder { DataSource = dbPath, Mode = SqliteOpenMode.ReadWriteCreate }.ToString();
        _ = new SqliteRepository(dbPath);
    }

    public IReadOnlyList<Cotation> GetCotations()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT ActifId, ActifNom, ActifTicker, ActifIsin, ActifMarche,
                   ActifDevise, ActifCoursActuel, ActifDateDernierCours
            FROM Actif
            ORDER BY ActifTicker;
            """;

        var result = new List<Cotation>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Cotation
            {
                Id = reader.GetInt64(0),
                Name = reader.GetString(1),
                Symbol = reader.GetString(2),
                Isin = reader.GetString(3),
                Marche = reader.GetString(4),
                Devise = reader.GetString(5),
                Close = reader.GetDouble(6),
                Date = DateTime.Parse(reader.GetString(7))
            });
        }

        return result;
    }

    public IReadOnlyList<TransactionFinanciere> GetRecent(int actifId, int count = 5)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT t.TransId, t.TransType, t.TransQte, t.TransDateTransac,
                   t.TransCpteId, t.TransActifId, t.TransPrix, t.TransFrais, c.CpteNom,
                   a.ActifTicker, a.ActifIsin
            FROM TransacFin t LEFT JOIN Compte c ON c.CpteId = t.TransCpteId
            LEFT JOIN Actif a ON a.ActifId = t.TransActifId
            WHERE t.TransActifId = $actifId
            ORDER BY datetime(t.TransDateTransac) DESC, t.TransId DESC LIMIT $count;
            """;
        command.Parameters.AddWithValue("$actifId", actifId);
        command.Parameters.AddWithValue("$count", count);

        var result = new List<TransactionFinanciere>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new TransactionFinanciere
            {
                TransId = reader.GetInt64(0), TransType = reader.GetString(1),
                TransQte = reader.GetDecimal(2), TransDateTransac = DateTime.Parse(reader.GetString(3)),
                TransCpteId = reader.GetInt32(4), TransActifId = reader.GetInt64(5),
                TransPrix = reader.GetDecimal(6), TransFrais = reader.GetDecimal(7),
                CompteNom = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                Symbol = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                Isin = reader.IsDBNull(10) ? string.Empty : reader.GetString(10)
            });
        }
        return result;
    }

    public int EnsureActif(Cotation cotation)
    {
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();
        var id = EnsureActif(connection, transaction, cotation);
        transaction.Commit();
        return id;
    }

    public void Save(TransactionFinanciere item, Cotation cotation, decimal? oldQuantity = null)
    {
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();
        var actifId = EnsureActif(connection, transaction, cotation);
        var ancienneTransaction = item.TransId == 0
            ? null
            : GetTransaction(connection, transaction, item.TransId);

        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = item.TransId == 0
            ? "INSERT INTO TransacFin (TransType,TransQte,TransDateTransac,TransCpteId,TransActifId,TransPrix,TransFrais) VALUES ($type,$qte,$date,$cpte,$actif,$prix,$frais); SELECT last_insert_rowid();"
            : "UPDATE TransacFin SET TransType=$type,TransQte=$qte,TransDateTransac=$date,TransCpteId=$cpte,TransPrix=$prix,TransFrais=$frais WHERE TransId=$id;";
        if (item.TransId != 0)
            command.Parameters.AddWithValue("$id", item.TransId);
        AddTransactionParameters(command, item, actifId);
        if (item.TransId == 0)
            item.TransId = Convert.ToInt64(command.ExecuteScalar());
        else
            command.ExecuteNonQuery();

        var signedQuantity = item.TransType == "Vente" ? -item.TransQte : item.TransQte;
        if (oldQuantity.HasValue)
            signedQuantity -= item.TransType == "Vente" ? -oldQuantity.Value : oldQuantity.Value;

        using var position = connection.CreateCommand();
        position.Transaction = transaction;
        position.CommandText = """
            INSERT INTO Position (PosActifId,PosQte,PosPrixMoyen,PosCpteId)
            VALUES ($actif,$qte,$prix,$cpte)
            ON CONFLICT(PosCpteId,PosActifId) DO UPDATE SET
              PosQte=PosQte+excluded.PosQte,
              PosPrixMoyen=CASE WHEN PosQte+excluded.PosQte>0
                THEN ((PosQte*PosPrixMoyen)+(excluded.PosQte*excluded.PosPrixMoyen))/(PosQte+excluded.PosQte)
                ELSE 0 END;
            """;
        position.Parameters.AddWithValue("$actif", actifId);
        position.Parameters.AddWithValue("$qte", signedQuantity);
        position.Parameters.AddWithValue("$prix", item.TransPrix);
        position.Parameters.AddWithValue("$cpte", item.TransCpteId);
        position.ExecuteNonQuery();

        if (ancienneTransaction != null)
        {
            MettreAJourSoldeCompte(
                connection,
                transaction,
                ancienneTransaction.TransCpteId,
                -CalculerVariationSolde(ancienneTransaction));
        }

        MettreAJourSoldeCompte(
            connection,
            transaction,
            item.TransCpteId,
            CalculerVariationSolde(item));

        transaction.Commit();
        item.TransActifId = actifId;
    }

    private static decimal CalculerVariationSolde(TransactionFinanciere transaction)
    {
        var montant = transaction.TransQte * transaction.TransPrix;
        return transaction.TransType == "Vente"
            ? montant - transaction.TransFrais
            : -montant - transaction.TransFrais;
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

    private static TransactionFinanciere? GetTransaction(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long transactionId)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT TransId, TransType, TransQte, TransDateTransac, TransCpteId, TransActifId,
                   TransPrix, TransFrais
            FROM TransacFin
            WHERE TransId = $id;
            """;
        command.Parameters.AddWithValue("$id", transactionId);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
            throw new InvalidOperationException($"La transaction {transactionId} est introuvable.");

        return new TransactionFinanciere
        {
            TransId = reader.GetInt64(0),
            TransType = reader.GetString(1),
            TransQte = reader.GetDecimal(2),
            TransDateTransac = DateTime.Parse(reader.GetString(3)),
            TransCpteId = reader.GetInt32(4),
            TransActifId = reader.GetInt64(5),
            TransPrix = reader.GetDecimal(6),
            TransFrais = reader.GetDecimal(7)
        };
    }

    private static int EnsureActif(SqliteConnection connection, SqliteTransaction transaction, Cotation cotation)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT ActifId FROM Actif WHERE ActifTicker=$ticker LIMIT 1;";
        command.Parameters.AddWithValue("$ticker", cotation.Symbol ?? string.Empty);
        var existingId = command.ExecuteScalar();
        command.Parameters.Clear();

        command.CommandText = existingId == null
            ? """
              INSERT INTO Actif
                (ActifNom,ActifTicker,ActifIsin,ActifMarche,ActifDevise,ActifCoursActuel,ActifDateDernierCours,ActifTransID)
              VALUES ($nom,$ticker,$isin,$marche,$devise,$cours,$date,0);
              SELECT last_insert_rowid();
              """
            : """
              UPDATE Actif SET ActifNom=$nom, ActifIsin=$isin, ActifMarche=$marche, ActifDevise=$devise,
                ActifCoursActuel=$cours, ActifDateDernierCours=$date
              WHERE ActifId=$id;
              SELECT ActifId FROM Actif WHERE ActifId=$id;
              """;
        if (existingId != null)
            command.Parameters.AddWithValue("$id", existingId);
        command.Parameters.AddWithValue("$nom", cotation.Name ?? string.Empty);
        command.Parameters.AddWithValue("$ticker", cotation.Symbol ?? string.Empty);
        command.Parameters.AddWithValue("$isin", cotation.Isin ?? string.Empty);
        command.Parameters.AddWithValue("$marche", cotation.Marche ?? string.Empty);
        command.Parameters.AddWithValue("$devise", cotation.Devise ?? string.Empty);
        command.Parameters.AddWithValue("$cours", cotation.Close);
        command.Parameters.AddWithValue("$date", cotation.Date.ToString("O"));
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static void AddTransactionParameters(SqliteCommand command, TransactionFinanciere item, int actifId)
    {
        command.Parameters.AddWithValue("$type", item.TransType);
        command.Parameters.AddWithValue("$qte", item.TransQte);
        command.Parameters.AddWithValue("$date", item.TransDateTransac.ToString("O"));
        command.Parameters.AddWithValue("$cpte", item.TransCpteId);
        command.Parameters.AddWithValue("$actif", actifId);
        command.Parameters.AddWithValue("$prix", item.TransPrix);
        command.Parameters.AddWithValue("$frais", item.TransFrais);
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
