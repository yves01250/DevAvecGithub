using SuiviPortefolio.Portefeuille.PortefeuilleModel;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SuiviPortefolio.Data;

namespace SuiviPortefolio.Portefeuille.PortefeuilleViewModel;

[ObservableObject]
public partial class MainViewModel
{
    private readonly SqliteRepository _database;

    public ObservableCollection<monPortefeuille> ListePortefeuilles { get; } = new();

    [ObservableProperty]
    private monPortefeuille? _portefeuilleSelectionne;

    [ObservableProperty]
    private monPortefeuille? _portefeuille;

    public decimal Solde
    {
        get => Portefeuille?.PtfSolde ?? 0m;
        set
        {
            if (Portefeuille != null && Portefeuille.PtfSolde != value)
            {
                Portefeuille.PtfSolde = value;
                OnPropertyChanged(nameof(Solde)); // ✅ OnPropertyChanged est fourni par ObservableObject
            }
        }
    }

    partial void OnPortefeuilleSelectionneChanged(monPortefeuille? value)
    {
        Portefeuille = value;
        ModifierPortefeuilleCommand.NotifyCanExecuteChanged();
        SupprimerPortefeuilleCommand.NotifyCanExecuteChanged();
        DefinirPortefeuilleDefautCommand.NotifyCanExecuteChanged();
    }

    public MainViewModel()
    {
        _database = new SqliteRepository(DatabaseLocation.DatabasePath);
        ChargerPortefeuilles();
    }

    [RelayCommand]
    private void AjouterPortefeuille()
    {
        var portefeuille = new monPortefeuille();
        var dialog = new PortefeuilleView.EditMainWindow(portefeuille, true)
        { Owner = Application.Current?.MainWindow };
        if (dialog.ShowDialog() != true) return;
        var id = _database.Ajouter(portefeuille);
        if (portefeuille.PtfEstDefaut) _database.SetPortefeuilleDefaut(id);
        ChargerPortefeuilles();
        PortefeuilleSelectionne = ListePortefeuilles.FirstOrDefault(p => p.PtfId == id);
    }

    [RelayCommand(CanExecute = nameof(CanModifyPortefeuille))]
    private void ModifierPortefeuille()
    {
        if (PortefeuilleSelectionne == null) return;
        var portefeuille = Copier(PortefeuilleSelectionne);
        var dialog = new PortefeuilleView.EditMainWindow(portefeuille, false)
        { Owner = Application.Current?.MainWindow };
        if (dialog.ShowDialog() != true) return;
        _database.Modifier(portefeuille);
        if (portefeuille.PtfEstDefaut) _database.SetPortefeuilleDefaut(portefeuille.PtfId);
        ChargerPortefeuilles();
        PortefeuilleSelectionne = ListePortefeuilles.FirstOrDefault(p => p.PtfId == portefeuille.PtfId);
    }

    private bool CanModifyPortefeuille() => PortefeuilleSelectionne != null;

    [RelayCommand(CanExecute = nameof(CanSupprimerPortefeuille))]
    private void SupprimerPortefeuille()
    {
        if (PortefeuilleSelectionne == null) return;
        if (MessageBox.Show($"Supprimer « {PortefeuilleSelectionne.PtfNom} » ?", "Portefeuille",
            MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        _database.Supprimer(PortefeuilleSelectionne.PtfId);
        ChargerPortefeuilles();
    }

    private bool CanSupprimerPortefeuille() => PortefeuilleSelectionne != null;

    [RelayCommand(CanExecute = nameof(CanDefinirPortefeuilleDefaut))]
    private void DefinirPortefeuilleDefaut()
    {
        if (PortefeuilleSelectionne == null) return;
        _database.SetPortefeuilleDefaut(PortefeuilleSelectionne.PtfId);
        ChargerPortefeuilles();
    }

    private bool CanDefinirPortefeuilleDefaut() => PortefeuilleSelectionne != null;

    public void ChargerPortefeuilles()
    {
        var tous = _database.GetAllPortefeuilles();
        ListePortefeuilles.Clear();
        foreach (var p in tous)
            ListePortefeuilles.Add(p);

        PortefeuilleSelectionne = ListePortefeuilles.FirstOrDefault(p => p.PtfEstDefaut)
            ?? ListePortefeuilles.FirstOrDefault();
    }

    private static monPortefeuille Copier(monPortefeuille p) => new()
    {
        PtfId = p.PtfId, PtfNom = p.PtfNom, PtfType = p.PtfType, PtfDevise = p.PtfDevise,
        PtfSolde = p.PtfSolde, PtfEstDefaut = p.PtfEstDefaut
    };
}