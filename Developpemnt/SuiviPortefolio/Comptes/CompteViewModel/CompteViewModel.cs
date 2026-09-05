using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SuiviPortefolio.Comptes.CompteModel;
using SuiviPortefolio.Comptes.CompteRepository;
using SuiviPortefolio.Comptes.CompteView;

namespace SuiviPortefolio.Comptes.CpteViewModel;


public class CompteViewModel : INotifyPropertyChanged
{
    private readonly ICompteRepository _repository;
    private Compte? _compteSelectionne;

    public CompteViewModel(ICompteRepository? repository = null)
    {
        _repository = repository ?? new CompteRepository.CompteRepository(
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SuiviPortefeuille.sqlite"));

        AjouterCompteCommand = new RelayCommand(_ => AjouterCompte());
        ModifierCompteCommand = new RelayCommand(_ => ModifierCompte(),
            _ => CompteSelectionne != null);
        SetCompteDefautCommand = new RelayCommand(_ => DefinirCompteDefaut(), _ => CompteSelectionne != null);

        ChargerComptes();
    }

    public ObservableCollection<Compte> Comptes { get; } = new();

    public Compte? CompteSelectionne
    {
        get => _compteSelectionne;
        set
        {
            if (_compteSelectionne != value)
            {
                _compteSelectionne = value;
                OnPropertyChanged();
                ((RelayCommand)ModifierCompteCommand).RaiseCanExecuteChanged();
                ((RelayCommand)SetCompteDefautCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand AjouterCompteCommand { get; }
    public ICommand ModifierCompteCommand { get; }
    public ICommand SetCompteDefautCommand { get; }

    public void ChargerComptes()
    {
        Comptes.Clear();
        foreach (var compte in _repository.GetAll())
        {
            Comptes.Add(compte);
        }

        CompteSelectionne = Comptes.FirstOrDefault(compte => compte.CpteEstDefaut)
            ?? Comptes.FirstOrDefault();
    }

    private void AjouterCompte()
    {
        var nouveauCompte = new Compte
        {
            CpteNom = "Nouveau compte",
            CpteType = TypeCompte.CompteTitre,
            CpteDevise = "EUR",
            CpteSolde = 0m,
            CpteEstDefaut = false,
            CptePtfId = 1
        };

        var dialog = new CompteEditWindow(nouveauCompte, nouveau: true);
        if (dialog.ShowDialog() != true)
            return;

        var nouveauCompteId = _repository.Add(nouveauCompte);
        if (nouveauCompte.CpteEstDefaut)
            _repository.SetDefault(nouveauCompteId);

        ChargerComptes();
        CompteSelectionne = Comptes.FirstOrDefault(compte => compte.CpteId == nouveauCompteId);
    }

    private void ModifierCompte()
    {
        if (CompteSelectionne == null)
            return;

        var compteModifie = new Compte
        {
            CpteId = CompteSelectionne.CpteId,
            CpteNom = CompteSelectionne.CpteNom,
            CpteType = CompteSelectionne.CpteType,
            CpteDevise = CompteSelectionne.CpteDevise,
            CpteSolde = CompteSelectionne.CpteSolde,
            CpteEstDefaut = CompteSelectionne.CpteEstDefaut,
            CptePtfId = CompteSelectionne.CptePtfId
        };

        var dialog = new CompteEditWindow(compteModifie, nouveau: false);
        if (dialog.ShowDialog() != true)
            return;

        _repository.Update(compteModifie);
        if (compteModifie.CpteEstDefaut)
            _repository.SetDefault(compteModifie.CpteId);

        ChargerComptes();
        CompteSelectionne = Comptes.FirstOrDefault(compte => compte.CpteId == compteModifie.CpteId);
    }

    private void DefinirCompteDefaut()
    {
        if (CompteSelectionne == null)
            return;

        _repository.SetDefault(CompteSelectionne.CpteId);
        ChargerComptes();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
