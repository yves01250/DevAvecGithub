using Microsoft.Data.Sqlite;
using SuiviPortefolio.Transactions.TransModel;
using SuiviPortefolio.Transactions.TransRepository;
using Xunit;

namespace SuiviPortefolio.Tests.TransactionsTests;

public sealed class CompteSoldeTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        $"suivi-portefolio-tests-{Guid.NewGuid():N}.sqlite");

    public CompteSoldeTests()
    {
        using var connection = new SqliteConnection(
            new SqliteConnectionStringBuilder { DataSource = _databasePath }.ToString());
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE Compte (
                CpteId INTEGER PRIMARY KEY,
                CpteNom TEXT NOT NULL,
                CpteType TEXT NOT NULL,
                CpteDevise TEXT NOT NULL,
                CpteSolde DECIMAL(18, 2) NOT NULL,
                CpteEstDefaut INTEGER NOT NULL,
                CptePtfId INTEGER NOT NULL
            );
            INSERT INTO Compte
                (CpteId, CpteNom, CpteType, CpteDevise, CpteSolde, CpteEstDefaut, CptePtfId)
            VALUES (1, 'Compte de test', 'CompteCourant', 'EUR', 1000, 0, 1);
            """;
        command.ExecuteNonQuery();
    }

    [Fact]
    public void EnregistrerAchat_DebiteLeCompteDuMontantEtDesFrais()
    {
        var repository = new TransRepository(_databasePath);

        repository.Save(
            new TransactionFinanciere
            {
                TransType = "Achat",
                TransQte = 2,
                TransPrix = 100,
                TransFrais = 3,
                TransDateTransac = new DateTime(2026, 1, 1),
                TransCpteId = 1
            },
            CreateCotation());

        Assert.Equal(797m, ReadBalance());
    }

    [Fact]
    public void EnregistrerVente_CrediteLeCompteApresDeductionDesFrais()
    {
        var repository = new TransRepository(_databasePath);

        repository.Save(
            new TransactionFinanciere
            {
                TransType = "Vente",
                TransQte = 2,
                TransPrix = 100,
                TransFrais = 3,
                TransDateTransac = new DateTime(2026, 1, 1),
                TransCpteId = 1
            },
            CreateCotation());

        Assert.Equal(1197m, ReadBalance());
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_databasePath))
            File.Delete(_databasePath);
    }

    private decimal ReadBalance()
    {
        using var connection = new SqliteConnection(
            new SqliteConnectionStringBuilder { DataSource = _databasePath }.ToString());
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT CpteSolde FROM Compte WHERE CpteId = 1;";
        return Convert.ToDecimal(command.ExecuteScalar());
    }

    private static Cotation CreateCotation() =>
        new()
        {
            Symbol = "TEST.PA",
            Name = "Actif de test",
            Instrument = "ETF",
            Marche = "Paris",
            Devise = "EUR",
            Close = 100,
            Date = new DateTime(2026, 1, 1)
        };
}
