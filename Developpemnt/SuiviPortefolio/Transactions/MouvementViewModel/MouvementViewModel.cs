using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SuiviPortefolio.Comptes.CompteModel;
using SuiviPortefolio.Comptes.CompteRepository;
using SuiviPortefolio.Data;
using SuiviPortefolio.Transactions.MouvementModel;
using IMouvementRepositoryType = SuiviPortefolio.Transactions.MouvementRepository.IMouvementRepository;
using MouvementRepositoryType = SuiviPortefolio.Transactions.MouvementRepository.MouvementRepository;

namespace SuiviPortefolio.Transactions.MouvementViewModel;

public class MouvementViewModel : INotifyPropertyChanged
{
    private readonly IMouvementRepositoryType _mouvementRepository;
    private readonly ICompteRepository _compteRepository;
    
    private Compte? _compteSelectionne;
    private Compte? _compteDestination;
    private TypeMouvement _typeMouvement = TypeMouvement.Depot;
    private string _montantTexte = string.Empty;
    private string _description = string.Empty;
    private DateTime _dateMouvement = DateTime.Today;
    private MouvementEspece? _mouvementSelectionne;
    
    public MouvementViewModel(IMouvementRepositoryType? mouvementRepository = null,
                             ICompteRepository? compteRepository = null)
    {
        var dbPath = DatabaseLocation.DatabasePath;
        _mouvementRepository = mouvementRepository ?? new MouvementRepositoryType(dbPath);
        _compteRepository = compteRepository ?? new CompteRepository(dbPath);
        
        // Charger les comptes
        foreach (var compte in _compteRepository.GetAll())
            Comptes.Add(compte);

        // Commandes
        EnregistrerCommand = new RelayCommand(_ => Enregistrer(), CanEnregistrer);
        AnnulerCommand = new RelayCommand(_ => Annuler());
        SupprimerCommand = new RelayCommand(_ => Supprimer(), CanSupprimer);

        CompteSelectionne = Comptes.FirstOrDefault(c => c.CpteEstDefaut) ?? Comptes.FirstOrDefault();
    }

    public ObservableCollection<Compte> Comptes { get; } = new();
    public ObservableCollection<MouvementEspece> Mouvements { get; } = new();
    
    public Compte? CompteSelectionne
    {
        get => _compteSelectionne;
        set
        {
            if (_compteSelectionne != value)
            {
                _compteSelectionne = value;
                OnPropertyChanged();
                ChargerMouvements();
                ((RelayCommand)EnregistrerCommand).RaiseCanExecuteChanged();
                ((RelayCommand)SupprimerCommand).RaiseCanExecuteChanged();
            }
        }
    }
    
    public Compte? CompteDestination
    {
        get => _compteDestination;
        set
        {
            if (_compteDestination != value)
            {
                _compteDestination = value;
                OnPropertyChanged();
                ((RelayCommand)EnregistrerCommand).RaiseCanExecuteChanged();
            }
        }
    }
    
    public TypeMouvement TypeMouvement
    {
        get => _typeMouvement;
        set
        {
            if (_typeMouvement != value)
            {
                _typeMouvement = value;
                OnPropertyChanged();
                // Pour les virements, on a besoin d'un compte destination
                if (value != TypeMouvement.Virement)
                    CompteDestination = null;
                ((RelayCommand)EnregistrerCommand).RaiseCanExecuteChanged();
            }
        }
    }
    
    public string MontantTexte
    {
        get => _montantTexte;
        set
        {
            if (_montantTexte != value)
            {
                _montantTexte = value;
                OnPropertyChanged();
                ((RelayCommand)EnregistrerCommand).RaiseCanExecuteChanged();
            }
        }
    }
    
    public string Description
    {
        get => _description;
        set
        {
            if (_description != value)
            {
                _description = value;
                OnPropertyChanged();
            }
        }
    }
    
    public DateTime DateMouvement
    {
        get => _dateMouvement;
        set
        {
            if (_dateMouvement != value)
            {
                _dateMouvement = value;
                OnPropertyChanged();
            }
        }
    }
    
    public MouvementEspece? MouvementSelectionne
    {
        get => _mouvementSelectionne;
        set
        {
            if (_mouvementSelectionne != value)
            {
                _mouvementSelectionne = value;
                OnPropertyChanged();
                ((RelayCommand)SupprimerCommand).RaiseCanExecuteChanged();
                
                if (value != null)
                {
                    // Pré-remplir les champs pour modification
                    TypeMouvement = value.MouvType;
                    MontantTexte = Math.Abs(value.MouvMontant).ToString("N2", CultureInfo.CurrentCulture);
                    Description = value.MouvDescription;
                    DateMouvement = value.MouvDate;
                    
                    // Trouver le compte source
                    CompteSelectionne = Comptes.FirstOrDefault(c => c.CpteId == value.MouvCpteId);
                    
                    // Si virement, trouver le compte destination
                    if (value.MouvType == TypeMouvement.Virement && value.MouvCpteDestId.HasValue)
                    {
                        CompteDestination = Comptes.FirstOrDefault(c => c.CpteId == value.MouvCpteDestId);
                    }
                }
            }
        }
    }
    
    public IReadOnlyList<TypeMouvement> TypesMouvement { get; } = Enum.GetValues(typeof(TypeMouvement)).Cast<TypeMouvement>().ToList();
    
    public ICommand EnregistrerCommand { get; }
    public ICommand AnnulerCommand { get; }
    public ICommand SupprimerCommand { get; }

    private bool CanEnregistrer(object? arg)
    {
        if (CompteSelectionne == null) return false;
        if (!decimal.TryParse(MontantTexte, NumberStyles.Number, CultureInfo.CurrentCulture, out var montant)) return false;
        if (montant <= 0) return false;
        if (TypeMouvement == TypeMouvement.Virement && CompteDestination == null) return false;
        return true;
    }
    
    private bool CanSupprimer(object? arg)
    {
        return MouvementSelectionne != null;
    }
    
    private void Enregistrer()
    {
        if (CompteSelectionne == null) return;
        
        var montant = decimal.Parse(MontantTexte, NumberStyles.Number, CultureInfo.CurrentCulture);
        
        // Appliquer le signe selon le type
        var montantAvecSigne = TypeMouvement switch
        {
            TypeMouvement.Retrait => -montant,
            TypeMouvement.Virement => -montant,
            _ => montant
        };
        
        var mouvement = new MouvementEspece
        {
            MouvId = MouvementSelectionne?.MouvId ?? 0,
            MouvType = TypeMouvement,
            MouvMontant = montantAvecSigne,
            MouvDate = DateMouvement,
            MouvCpteId = CompteSelectionne.CpteId,
            MouvCpteDestId = TypeMouvement == TypeMouvement.Virement ? CompteDestination?.CpteId : null,
            MouvDescription = Description,
            CompteNom = CompteSelectionne.CpteNom,
            CompteDestNom = CompteDestination?.CpteNom
        };
        
        _mouvementRepository.Save(mouvement);
        ChargerMouvements();
        Annuler();
    }
    
    private void Supprimer()
    {
        if (MouvementSelectionne == null) return;
        
        _mouvementRepository.Delete(MouvementSelectionne.MouvId);
        ChargerMouvements();
        Annuler();
    }
    
    private void Annuler()
    {
        MouvementSelectionne = null;
        MontantTexte = string.Empty;
        Description = string.Empty;
        CompteDestination = null;
        DateMouvement = DateTime.Today;
        TypeMouvement = TypeMouvement.Depot;
    }
    
    private void ChargerMouvements()
    {
        if (CompteSelectionne == null)
        {
            Mouvements.Clear();
            return;
        }
        
        Mouvements.Clear();
        foreach (var mouvement in _mouvementRepository.GetAllByCompte(CompteSelectionne.CpteId))
        {
            Mouvements.Add(mouvement);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object? parameter) => _execute(parameter);

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
