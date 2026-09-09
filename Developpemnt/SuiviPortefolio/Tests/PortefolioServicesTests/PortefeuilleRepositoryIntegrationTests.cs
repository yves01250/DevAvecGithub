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

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_databasePath))
            File.Delete(_databasePath);
    }
}
