using Microsoft.Data.Sqlite;
using SuiviPortefolio.Data;
using SuiviPortefolio.Portefeuille.PortefeuilleModel;
using Xunit;

namespace SuiviPortefolio.Tests.PortefolioServicesTests;

public sealed class PortefeuilleRepositoryIntegrationTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        $"suivi-portefolio-portefeuille-tests-{Guid.NewGuid():N}.sqlite");

    [Fact]
    public void AjouterPuisRelirePortefeuille_PreserveLesDonneesDansSqlite()
    {
        var portefeuille = new monPortefeuille
        {
            PtfNom = "Portefeuille d'intégration",
            PtfSolde = 2450.75m
        };

        var repository = new SqliteRepository(_databasePath);
        repository.Ajouter(portefeuille);

        var reouvertureRepository = new SqliteRepository(_databasePath);
        var portefeuilles = reouvertureRepository.GetAllPortefeuilles().ToList();

        var portefeuilleRelu = Assert.Single(portefeuilles);
        Assert.NotEqual(0, portefeuilleRelu.PtfId);
        Assert.Equal(portefeuille.PtfNom, portefeuilleRelu.PtfNom);
        Assert.Equal(portefeuille.PtfSolde, portefeuilleRelu.PtfSolde);
    }

    [Fact]
    public void AjouterEtf_EnregistreLeDetailDansLaTableEft()
    {
        _ = new SqliteRepository(_databasePath);

        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = _databasePath,
            Mode = SqliteOpenMode.ReadWrite
        }.ToString());
        connection.Open();

        int actifId;
        using (var insertActif = connection.CreateCommand())
        {
            insertActif.CommandText = @"
                INSERT INTO Actif (
                    ActifNom,
                    ActifTicker,
                    ActifIsin,
                    ActifMarche,
                    ActifDevise,
                    ActifCoursActuel,
                    ActifDateDernierCours,
                    ActifTransID)
                VALUES (
                    $nom,
                    $ticker,
                    $isin,
                    $marche,
                    $devise,
                    $cours,
                    $dateCours,
                    0);
                SELECT last_insert_rowid();
            ";

            insertActif.Parameters.AddWithValue("$nom", "iShares Core MSCI World");
            insertActif.Parameters.AddWithValue("$ticker", "IWDA");
            insertActif.Parameters.AddWithValue("$isin", "IE00BYX2Q5M1");
            insertActif.Parameters.AddWithValue("$marche", "XETRA");
            insertActif.Parameters.AddWithValue("$devise", "EUR");
            insertActif.Parameters.AddWithValue("$cours", 98.42m);
            insertActif.Parameters.AddWithValue("$dateCours", "2026-09-10");

            actifId = Convert.ToInt32(insertActif.ExecuteScalar());
        }

        using (var insertEtf = connection.CreateCommand())
        {
            insertEtf.CommandText = @"
                INSERT INTO ETF (
                    ActifEtfId,
                    ActifEtfNom,
                    ActifEtfIndiceSuivi,
                    ActifEtfCapit,
                    ActifEtfFraisGestion)
                VALUES (
                    $actifId,
                    $nomEft,
                    $indice,
                    $capitalise,
                    $frais);
            ";

            insertEtf.Parameters.AddWithValue("$actifId", actifId);
            insertEtf.Parameters.AddWithValue("$nomEft", "iShares Core MSCI World");
            insertEtf.Parameters.AddWithValue("$indice", "MSCI World");
            insertEtf.Parameters.AddWithValue("$capitalise", 1);
            insertEtf.Parameters.AddWithValue("$frais", 0.12m);

            insertEtf.ExecuteNonQuery();
        }

        using (var check = connection.CreateCommand())
        {
            check.CommandText = @"
                SELECT a.ActifNom,
                       a.ActifTicker,
                       e.ActifEtfNom,
                       e.ActifEtfIndiceSuivi,
                       e.ActifEtfCapit,
                       e.ActifEtfFraisGestion
                FROM ETF e
                INNER JOIN Actif a ON a.ActifId = e.ActifEtfId;
            ";

            using var reader = check.ExecuteReader();
            Assert.True(reader.Read());

            Assert.Equal("iShares Core MSCI World", reader.GetString(0));
            Assert.Equal("IWDA", reader.GetString(1));
            Assert.Equal("iShares Core MSCI World", reader.GetString(2));
            Assert.Equal("MSCI World", reader.GetString(3));
            Assert.Equal(1, reader.GetInt32(4));
            Assert.Equal(0.12m, reader.GetDecimal(5));
        }
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_databasePath))
            File.Delete(_databasePath);
    }
}
