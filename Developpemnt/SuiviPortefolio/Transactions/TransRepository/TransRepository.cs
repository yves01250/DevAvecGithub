using Microsoft.Data.Sqlite;
using SuiviPortefolio.Transactions.TransModel;

namespace SuiviPortefolio.Transactions.TransRepository;

public sealed class TransRepository : ITransRepository
{
    private readonly string _connectionString;

    public TransRepository(string dbPath)
    {
        _connectionString = new SqliteConnectionStringBuilder { DataSource = dbPath, Mode = SqliteOpenMode.ReadWriteCreate }.ToString();
        EnsureSchema();
    }

    public IReadOnlyList<TransactionFinanciere> GetRecent(int actifId, int count = 5)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT t.TransId, t.TransType, t.TransQte, t.TransDateTransac,
                   t.TransCpteId, t.TransActifId, t.TransPrix, t.TransFrais, c.CpteNom
            FROM TransacFin t LEFT JOIN Compte c ON c.CpteId = t.TransCpteId
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
                CompteNom = reader.IsDBNull(8) ? string.Empty : reader.GetString(8)
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
            SELECT TransId, TransType, TransQte, TransCpteId, TransActifId,
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
            TransCpteId = reader.GetInt32(3),
            TransActifId = reader.GetInt64(4),
            TransPrix = reader.GetDecimal(5),
            TransFrais = reader.GetDecimal(6)
        };
    }

    private void EnsureSchema()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Actif (
              ActifId INTEGER PRIMARY KEY AUTOINCREMENT, ActifNom TEXT NOT NULL,
              ActifTicker TEXT NOT NULL UNIQUE, ActifIsin TEXT NOT NULL, ActifMarche TEXT NOT NULL,
              ActifDevise TEXT NOT NULL, ActifCoursActuel DECIMAL(18,2) NOT NULL,
              ActifDateDernierCours TEXT NOT NULL, ActifTransID INTEGER NOT NULL DEFAULT 0);
            CREATE TABLE IF NOT EXISTS TransacFin (
              TransId INTEGER PRIMARY KEY AUTOINCREMENT, TransType TEXT NOT NULL,
              TransQte DECIMAL(18,2) NOT NULL, TransDateTransac TEXT NOT NULL,
              TransCpteId INTEGER NOT NULL, TransActifId INTEGER NOT NULL,
              TransPrix DECIMAL(18,2) NOT NULL DEFAULT 0, TransFrais DECIMAL(18,2) NOT NULL DEFAULT 0);
            CREATE TABLE IF NOT EXISTS Position (
              PosId INTEGER PRIMARY KEY AUTOINCREMENT, PosActifId INTEGER NOT NULL,
              PosQte DECIMAL(18,2) NOT NULL, PosPrixMoyen DECIMAL(18,2) NOT NULL,
              PosCpteId INTEGER NOT NULL, UNIQUE(PosCpteId,PosActifId));
            """;
        command.ExecuteNonQuery();
        AddColumnIfMissing(connection, "TransacFin", "TransPrix", "DECIMAL(18,2) NOT NULL DEFAULT 0");
        AddColumnIfMissing(connection, "TransacFin", "TransFrais", "DECIMAL(18,2) NOT NULL DEFAULT 0");
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
              UPDATE Actif SET ActifNom=$nom, ActifMarche=$marche, ActifDevise=$devise,
                ActifCoursActuel=$cours, ActifDateDernierCours=$date
              WHERE ActifId=$id;
              SELECT ActifId FROM Actif WHERE ActifId=$id;
              """;
        if (existingId != null)
            command.Parameters.AddWithValue("$id", existingId);
        command.Parameters.AddWithValue("$nom", cotation.Name ?? string.Empty);
        command.Parameters.AddWithValue("$ticker", cotation.Symbol ?? string.Empty);
        command.Parameters.AddWithValue("$isin", cotation.Symbol ?? string.Empty);
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

    private static void AddColumnIfMissing(SqliteConnection connection, string table, string column, string definition)
    {
        using var check = connection.CreateCommand();
        check.CommandText = $"SELECT COUNT(*) FROM pragma_table_info('{table}') WHERE name=$column";
        check.Parameters.AddWithValue("$column", column);
        if (Convert.ToInt32(check.ExecuteScalar()) == 0)
        {
            using var alter = connection.CreateCommand();
            alter.CommandText = $"ALTER TABLE {table} ADD COLUMN {column} {definition}";
            alter.ExecuteNonQuery();
        }
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
