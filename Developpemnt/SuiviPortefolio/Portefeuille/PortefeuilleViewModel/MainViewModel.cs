using SuiviPortefolio.Portefeuille.PortefeuilleModel;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SuiviPortefolio.Data;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;


namespace SuiviPortefolio.Portefeuille.PortefeuilleViewModel;

public class MainViewModel : INotifyPropertyChanged
{

    private readonly SqliteRepository _database;
    private monPortefeuille? _portefeuille;


    public MainViewModel()
    {

        var dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SuiviPortefeuille.sqlite");
            _database = new SqliteRepository(dbPath);

        AjouterPortefeuilleCommand = new RelayCommand(_ => AjouterPortefeuille());
        ModifierPortefeuilleCommand = new RelayCommand(_ => ModifierPortefeuille(), _ => PortefeuilleSelectionne != null);
        SupprimerPortefeuilleCommand = new RelayCommand(_ => SupprimerPortefeuille(), _ => PortefeuilleSelectionne != null);
        DefinirPortefeuilleDefautCommand = new RelayCommand(_ => DefinirPortefeuilleDefaut(), _ => PortefeuilleSelectionne != null);
        ChargerPortefeuilles();
    }
            public ObservableCollection<monPortefeuille> ListePortefeuilles { get; } = new();

        private monPortefeuille? _portefeuilleSelectionne;

        public monPortefeuille? PortefeuilleSelectionne
        {

            get => _portefeuilleSelectionne;
            set
            {
                if (_portefeuilleSelectionne != value)
                {
                    _portefeuilleSelectionne = value;
                    OnPropertyChanged();
                    // Quand on change de sélection, on en fait le portefeuille courant
                    Portefeuille = value;
                    ((RelayCommand)ModifierPortefeuilleCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)SupprimerPortefeuilleCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DefinirPortefeuilleDefautCommand).RaiseCanExecuteChanged();
                }
            }
        }

        

        public void ChargerPortefeuilles()
        {
            // À implémenter dans SqliteRepository : IEnumerable<monPortefeuille> GetAll()
            var tous = _database.GetAllPortefeuilles(); 
            ListePortefeuilles.Clear();
            foreach (var p in tous)
                ListePortefeuilles.Add(p);

            // Optionnel : choisir un “défaut” (ex. le premier, ou celui avec un flag)
            PortefeuilleSelectionne = ListePortefeuilles.FirstOrDefault(p => p.PtfEstDefaut)
                ?? ListePortefeuilles.FirstOrDefault();
        }
    public monPortefeuille? Portefeuille
    {
        get
        {
            return _portefeuille;
        }
        set
        {
            if (_portefeuille != value)
            {
                _portefeuille = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal Solde
    {
                get => Portefeuille?.PtfSolde ?? 0m;
        set
        {
            if (Portefeuille != null && Portefeuille.PtfSolde != value)
            {
                Portefeuille.PtfSolde = value;
                OnPropertyChanged();
            }
        }
    }
    //private readonly SqliteRepository _database = new SqliteRepository("SuiviPortefeuille.sqlite");
    public ICommand AjouterPortefeuilleCommand { get; }
    public ICommand ModifierPortefeuilleCommand { get; }
    public ICommand SupprimerPortefeuilleCommand { get; }
    public ICommand DefinirPortefeuilleDefautCommand { get; }

    private void AjouterPortefeuille()
    {
        var portefeuille = new monPortefeuille();
        var dialog = new PortefeuilleView.EditMainWindow(portefeuille, true) { Owner = Application.Current?.MainWindow };
        if (dialog.ShowDialog() != true) return;
        var id = _database.Ajouter(portefeuille);
        if (portefeuille.PtfEstDefaut) _database.SetPortefeuilleDefaut(id);
        ChargerPortefeuilles();
        PortefeuilleSelectionne = ListePortefeuilles.FirstOrDefault(p => p.PtfId == id);
    }

    private void ModifierPortefeuille()
    {
        if (PortefeuilleSelectionne == null) return;
        var portefeuille = Copier(PortefeuilleSelectionne);
        var dialog = new PortefeuilleView.EditMainWindow(portefeuille, false) { Owner = Application.Current?.MainWindow };
        if (dialog.ShowDialog() != true) return;
        _database.Modifier(portefeuille);
        if (portefeuille.PtfEstDefaut) _database.SetPortefeuilleDefaut(portefeuille.PtfId);
        ChargerPortefeuilles();
        PortefeuilleSelectionne = ListePortefeuilles.FirstOrDefault(p => p.PtfId == portefeuille.PtfId);
    }

    private void SupprimerPortefeuille()
    {
        if (PortefeuilleSelectionne == null) return;
        if (MessageBox.Show($"Supprimer « {PortefeuilleSelectionne.PtfNom} » ?", "Portefeuille",
            MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        _database.Supprimer(PortefeuilleSelectionne.PtfId);
        ChargerPortefeuilles();
    }

    private void DefinirPortefeuilleDefaut()
    {
        if (PortefeuilleSelectionne == null) return;
        _database.SetPortefeuilleDefaut(PortefeuilleSelectionne.PtfId);
        ChargerPortefeuilles();
    }

    private static monPortefeuille Copier(monPortefeuille p) => new()
    {
        PtfId = p.PtfId, PtfNom = p.PtfNom, PtfType = p.PtfType, PtfDevise = p.PtfDevise,
        PtfSolde = p.PtfSolde, PtfEstDefaut = p.PtfEstDefaut
    };

    private sealed class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
            => (_execute, _canExecute) = (execute, canExecute);
        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _execute(parameter);
        public event EventHandler? CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }    
}
