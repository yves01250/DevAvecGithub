using Microsoft.Data.Sqlite;
using SuiviPortefolio.Comptes.CompteRepository;
using Xunit;

namespace SuiviPortefolio.Tests.ComptesTests;

public sealed class CompteRepositoryValuationTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        $"suivi-portefolio-valuation-tests-{Guid.NewGuid():N}.sqlite");

    public CompteRepositoryValuationTests()
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
                CpteSolde DECIMAL(18, 2) NOT NULL
            );
            CREATE TABLE Position (
                PosId INTEGER PRIMARY KEY,
                PosActifId INTEGER NOT NULL,
                PosQte DECIMAL(18, 2) NOT NULL,
                PosCpteId INTEGER NOT NULL
            );
            CREATE TABLE Actif (
                ActifId INTEGER PRIMARY KEY,
                ActifDevise TEXT NOT NULL,
                ActifCoursActuel DECIMAL(18, 2) NOT NULL
            );
            INSERT INTO Compte VALUES
                (1, 'Compte EUR', 'PEA', 'EUR', 100),
                (2, 'Compte USD', 'CompteTitre', 'USD', 50);
            INSERT INTO Actif VALUES
                (1, 'EUR', 20),
                (2, 'USD', 50);
            INSERT INTO Position VALUES
                (1, 1, 10, 1),
                (2, 2, 2, 1);
            """;
        command.ExecuteNonQuery();
    }

    [Fact]
    public void GetExpositions_AggregatesPositionsPerAccountAndAssetCurrency()
    {
        var repository = new CompteRepository(_databasePath);

        var expositions = repository.GetExpositions();

        Assert.Collection(
            expositions,
            euroAccount =>
            {
                Assert.Equal(1, euroAccount.CpteId);
                Assert.Equal("EUR", euroAccount.CpteDevise);
                Assert.Equal("EUR", euroAccount.ActifDevise);
                Assert.Equal(100m, euroAccount.CpteSolde);
                Assert.Equal(200m, euroAccount.ValorisationActifs);
            },
            dollarPosition =>
            {
                Assert.Equal(1, dollarPosition.CpteId);
                Assert.Equal("USD", dollarPosition.ActifDevise);
                Assert.Equal(100m, dollarPosition.ValorisationActifs);
            },
            accountWithoutPositions =>
            {
                Assert.Equal(2, accountWithoutPositions.CpteId);
                Assert.Null(accountWithoutPositions.ActifDevise);
                Assert.Equal(0m, accountWithoutPositions.ValorisationActifs);
            });
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_databasePath))
            File.Delete(_databasePath);
    }
}
