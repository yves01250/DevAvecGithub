using Microsoft.Data.Sqlite;
using SuiviPortefolio.Comptes.CompteModel;
using SuiviPortefolio.Comptes.CompteRepository;
using Xunit;

namespace SuiviPortefolio.Tests.ComptesTests;

/// <summary>
/// Tests unitaires pour les méthodes manquantes de CompteRepository.
/// Complète les tests existants dans CompteRepositoryValuationTests.cs
/// </summary>
public sealed class CompteRepositoryMissingMethodsTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        $"suivi-portefolio-compte-tests-{Guid.NewGuid():N}.sqlite");

    public CompteRepositoryMissingMethodsTests()
    {
        using var connection = new SqliteConnection(
            new SqliteConnectionStringBuilder { DataSource = _databasePath }.ToString());
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE Compte (
                CpteId INTEGER PRIMARY KEY AUTOINCREMENT,
                CpteNom TEXT NOT NULL,
                CpteType TEXT NOT NULL,
                CpteDevise TEXT NOT NULL,
                CpteSolde DECIMAL(18, 2) NOT NULL DEFAULT 0,
                CpteEstDefaut INTEGER NOT NULL DEFAULT 0,
                CptePtfId INTEGER NOT NULL DEFAULT 1
            );
            CREATE TABLE Position (
                PosId INTEGER PRIMARY KEY,
                PosActifId INTEGER NOT NULL,
                PosQte DECIMAL(18, 2) NOT NULL,
                PosCpteId INTEGER NOT NULL,
                FOREIGN KEY (PosCpteId) REFERENCES Compte(CpteId)
            );
        ";
        command.ExecuteNonQuery();
    }

    // ========================================================================
    // Tests pour GetById
    // ========================================================================

    [Fact]
    public void GetById_RetourneLeCompteCorrespondant()
    {
        // Arrange
        var repository = new CompteRepository(_databasePath);
        var expectedCompte = new Compte
        {
            CpteNom = "Compte Test",
            CpteType = TypeCompte.CompteTitre,
            CpteDevise = "EUR",
            CpteSolde = 1000m,
            CpteEstDefaut = false,
            CptePtfId = 1
        };
        var compteId = repository.Add(expectedCompte);

        // Act
        var result = repository.GetById(compteId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedCompte.CpteNom, result.CpteNom);
        Assert.Equal(expectedCompte.CpteType, result.CpteType);
        Assert.Equal(expectedCompte.CpteDevise, result.CpteDevise);
        Assert.Equal(expectedCompte.CpteSolde, result.CpteSolde);
    }

    [Fact]
    public void GetById_RetourneNullSiCompteInexistant()
    {
        // Arrange
        var repository = new CompteRepository(_databasePath);

        // Act
        var result = repository.GetById(999); // ID inexistant

        // Assert
        Assert.Null(result);
    }

    // ========================================================================
    // Tests pour Delete
    // ========================================================================

    [Fact]
    public void Delete_SupprimeLeCompteEtRetourneTrue()
    {
        // Arrange
        var repository = new CompteRepository(_databasePath);
        var compteId = repository.Add(new Compte
        {
            CpteNom = "À supprimer",
            CpteType = TypeCompte.CompteCourant,
            CpteDevise = "EUR",
            CpteSolde = 500m
        });

        // Act
        var result = repository.Delete(compteId);

        // Assert
        Assert.True(result);
        Assert.Null(repository.GetById(compteId));
    }

    [Fact]
    public void Delete_RetourneFalseSiCompteInexistant()
    {
        // Arrange
        var repository = new CompteRepository(_databasePath);

        // Act
        var result = repository.Delete(999); // ID inexistant

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Delete_LanceExceptionSiCompteEstReference()
    {
        // Arrange
        var repository = new CompteRepository(_databasePath);
        var compteId = repository.Add(new Compte
        {
            CpteNom = "Compte avec positions",
            CpteType = TypeCompte.CompteTitre,
            CpteDevise = "EUR",
            CpteSolde = 1000m
        });

        // Ajouter une position qui référence ce compte
        using var connection = new SqliteConnection(
            new SqliteConnectionStringBuilder { DataSource = _databasePath }.ToString());
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO Position (PosActifId, PosQte, PosCpteId) VALUES (1, 10, $compteId);";
        command.Parameters.AddWithValue("$compteId", compteId);
        command.ExecuteNonQuery();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => repository.Delete(compteId));
        Assert.Contains("ne peut pas être supprimé car il contient des positions", exception.Message);
    }

    // ========================================================================
    // Tests pour SetDefault
    // ========================================================================

    [Fact]
    public void SetDefault_DefinitLeCompteCommeDefautEtDesactiveLesAutres()
    {
        // Arrange
        var repository = new CompteRepository(_databasePath);
        var compte1Id = repository.Add(new Compte
        {
            CpteNom = "Compte 1",
            CpteType = TypeCompte.CompteCourant,
            CpteDevise = "EUR",
            CpteSolde = 1000m,
            CpteEstDefaut = true
        });
        var compte2Id = repository.Add(new Compte
        {
            CpteNom = "Compte 2",
            CpteType = TypeCompte.CompteTitre,
            CpteDevise = "USD",
            CpteSolde = 2000m,
            CpteEstDefaut = false
        });

        // Act
        repository.SetDefault(compte2Id);

        // Assert
        var compte1 = repository.GetById(compte1Id);
        var compte2 = repository.GetById(compte2Id);
        Assert.NotNull(compte1);
        Assert.NotNull(compte2);
        Assert.False(compte1.CpteEstDefaut);
        Assert.True(compte2.CpteEstDefaut);
    }

    [Fact]
    public void SetDefault_LanceExceptionSiCompteInexistant()
    {
        // Arrange
        var repository = new CompteRepository(_databasePath);

        // Act & Assert
        var exception = Assert.Throws<SqliteException>(() => repository.SetDefault(999));
    }

    // ========================================================================
    // Tests pour Exists
    // ========================================================================

    [Fact]
    public void Exists_RetourneTrueSiCompteExiste()
    {
        // Arrange
        var repository = new CompteRepository(_databasePath);
        var compteId = repository.Add(new Compte
        {
            CpteNom = "Compte Existant",
            CpteType = TypeCompte.CompteCourant
        });

        // Act & Assert
        Assert.True(repository.Exists(compteId));
    }

    [Fact]
    public void Exists_RetourneFalseSiCompteInexistant()
    {
        // Arrange
        var repository = new CompteRepository(_databasePath);

        // Act & Assert
        Assert.False(repository.Exists(999));
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_databasePath))
            File.Delete(_databasePath);
    }
}
