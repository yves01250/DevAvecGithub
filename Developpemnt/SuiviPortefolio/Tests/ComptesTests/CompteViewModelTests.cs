using Moq;
using SuiviPortefolio.Comptes.CompteModel;
using SuiviPortefolio.Comptes.CompteRepository;
using SuiviPortefolio.Comptes.CpteViewModel;
using Xunit;

namespace SuiviPortefolio.Tests.ComptesTests;

/// <summary>
/// Tests unitaires pour CompteViewModel.
/// Utilise Moq pour simuler les dépendances.
/// </summary>
public sealed class CompteViewModelTests : IDisposable
{
    private readonly Mock<ICompteRepository> _mockRepository;
    private readonly Mock<ICotationFetcher> _mockCotationFetcher;
    private readonly CompteViewModel _viewModel;

    public CompteViewModelTests()
    {
        _mockRepository = new Mock<ICompteRepository>();
        _mockCotationFetcher = new Mock<ICotationFetcher>();
        _viewModel = new CompteViewModel(_mockRepository.Object, _mockCotationFetcher.Object);
    }

    // ========================================================================
    // Tests pour le constructeur et l'initialisation
    // ========================================================================

    [Fact]
    public void Constructor_InitialiseLesCommandes()
    {
        // Assert
        Assert.NotNull(_viewModel.AjouterCompteCommand);
        Assert.NotNull(_viewModel.ModifierCompteCommand);
        Assert.NotNull(_viewModel.SetCompteDefautCommand);
        Assert.NotNull(_viewModel.ActualiserValorisationsCommand);
    }

    [Fact]
    public void Constructor_ChargeLesComptesDepuisLeRepository()
    {
        // Arrange
        var expectedComptes = new List<Compte>
        {
            new Compte { CpteId = 1, CpteNom = "Compte 1", CpteType = TypeCompte.CompteCourant },
            new Compte { CpteId = 2, CpteNom = "Compte 2", CpteType = TypeCompte.PEA }
        };
        _mockRepository.Setup(r => r.GetAll()).Returns(expectedComptes);

        // Act
        var viewModel = new CompteViewModel(_mockRepository.Object, _mockCotationFetcher.Object);

        // Assert
        Assert.Equal(2, viewModel.Comptes.Count);
        Assert.Equal("Compte 1", viewModel.Comptes[0].CpteNom);
    }

    // ========================================================================
    // Tests pour les propriétés et CanExecute
    // ========================================================================

    [Fact]
    public void ModifierCompteCommand_CanExecute_RetourneFalseSiAucunCompteSelectionne()
    {
        // Arrange
        _viewModel.CompteSelectionne = null;

        // Act
        var canExecute = _viewModel.ModifierCompteCommand.CanExecute(null);

        // Assert
        Assert.False(canExecute);
    }

    [Fact]
    public void ModifierCompteCommand_CanExecute_RetourneTrueSiCompteSelectionne()
    {
        // Arrange
        _viewModel.CompteSelectionne = new Compte { CpteId = 1 };

        // Act
        var canExecute = _viewModel.ModifierCompteCommand.CanExecute(null);

        // Assert
        Assert.True(canExecute);
    }

    [Fact]
    public void SetCompteDefautCommand_CanExecute_RetourneFalseSiAucunCompteSelectionne()
    {
        // Arrange
        _viewModel.CompteSelectionne = null;

        // Act
        var canExecute = _viewModel.SetCompteDefautCommand.CanExecute(null);

        // Assert
        Assert.False(canExecute);
    }

    [Fact]
    public void SetCompteDefautCommand_CanExecute_RetourneTrueSiCompteSelectionne()
    {
        // Arrange
        _viewModel.CompteSelectionne = new Compte { CpteId = 1 };

        // Act
        var canExecute = _viewModel.SetCompteDefautCommand.CanExecute(null);

        // Assert
        Assert.True(canExecute);
    }

    [Fact]
    public void CompteSelectionne_Setter_MetAJourCanExecuteDesCommandes()
    {
        // Arrange
        var compte = new Compte { CpteId = 1 };

        // Act
        _viewModel.CompteSelectionne = compte;

        // Assert
        Assert.True(_viewModel.ModifierCompteCommand.CanExecute(null));
        Assert.True(_viewModel.SetCompteDefautCommand.CanExecute(null));
    }

    // ========================================================================
    // Tests pour DefinirCompteDefautAsync
    // ========================================================================

    [Fact]
    public async Task DefinirCompteDefautAsync_NeFaitRienSiAucunCompteSelectionne()
    {
        // Arrange
        _viewModel.CompteSelectionne = null;

        // Act
        await _viewModel.DefinirCompteDefautAsync();

        // Assert
        _mockRepository.Verify(r => r.SetDefault(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DefinirCompteDefautAsync_AppelleRepositorySetDefault()
    {
        // Arrange
        var compteSelectionne = new Compte { CpteId = 1, CpteNom = "Compte par défaut" };
        _viewModel.CompteSelectionne = compteSelectionne;

        _mockRepository.Setup(r => r.SetDefault(1)).Verifiable();
        _mockRepository.Setup(r => r.GetAll()).Returns(new List<Compte> { compteSelectionne });

        // Act
        await _viewModel.DefinirCompteDefautAsync();

        // Assert
        _mockRepository.Verify(r => r.SetDefault(1), Times.Once);
    }

    // ========================================================================
    // Tests pour ActualiserValorisationsCommand CanExecute
    // ========================================================================

    [Fact]
    public void ActualiserValorisationsCommand_CanExecute_RetourneTrueInitialement()
    {
        // Act & Assert
        Assert.True(_viewModel.ActualiserValorisationsCommand.CanExecute(null));
    }

    public void Dispose()
    {
        _mockRepository.Reset();
        _mockCotationFetcher.Reset();
    }
}

/// <summary>
/// Interface pour CotationFetcher (à ajouter dans ton projet si elle n'existe pas)
/// </summary>
public interface ICotationFetcher
{
    Task<Cotation?> FetchSnapshotAsync(string symbol);
}
