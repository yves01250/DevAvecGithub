using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SuiviPortefolio.Comptes.CompteModel;
using SuiviPortefolio.Comptes.CompteRepository;
using SuiviPortefolio.Data;
using SuiviPortefolio.Transactions.MouvementModel;
using IMouvementRepositoryType = SuiviPortefolio.Transactions.MouvementRepository.IMouvementRepository;
using MouvementRepositoryType = SuiviPortefolio.Transactions.MouvementRepository.MouvementRepository;

namespace SuiviPortefolio.Transactions.MouvementViewModel;

[ObservableObject]
public partial class MouvementViewModel
{
    private readonly IMouvementRepositoryType _mouvementRepository;
    private readonly ICompteRepository _compteRepository;

    public ObservableCollection<Compte> Comptes { get; } = new();
    public ObservableCollection<MouvementEspece> Mouvements { get; } = new();
    public IReadOnlyList<TypeMouvement> TypesMouvement { get; } =
        Enum.GetValues(typeof(TypeMouvement)).Cast<TypeMouvement>().ToList();

    [ObservableProperty]
    private Compte? _compteSelectionne;

    [ObservableProperty]
    private Compte? _compteDestination;

    [ObservableProperty]
    private TypeMouvement _typeMouvement = TypeMouvement.Depot;

    [ObservableProperty]
    private string _montantTexte = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private DateTime _dateMouvement = DateTime.Today;

    [ObservableProperty]
    private MouvementEspece? _mouvementSelectionne;

    public MouvementViewModel(IMouvementRepositoryType? mouvementRepository = null,
                             ICompteRepository? compteRepository = null)
    {
        var dbPath = DatabaseLocation.DatabasePath;
        _mouvementRepository = mouvementRepository ?? new MouvementRepositoryType(dbPath);
        _compteRepository = compteRepository ?? new CompteRepository(dbPath);

        foreach (var compte in _compteRepository.GetAll())
            Comptes.Add(compte);

        CompteSelectionne = Comptes.FirstOrDefault(c => c.CpteEstDefaut) ?? Comptes.FirstOrDefault();
    }

    partial void OnCompteSelectionneChanged(Compte? value)
    {
        ChargerMouvements();
        EnregistrerCommand.NotifyCanExecuteChanged();
        SupprimerCommand.NotifyCanExecuteChanged();
    }

    partial void OnCompteDestinationChanged(Compte? value)
    {
        EnregistrerCommand.NotifyCanExecuteChanged();
    }

    partial void OnTypeMouvementChanged(TypeMouvement value)
    {
        if (value != TypeMouvement.Virement)
            CompteDestination = null;
        EnregistrerCommand.NotifyCanExecuteChanged();
    }

    partial void OnMontantTexteChanged(string value)
    {
        EnregistrerCommand.NotifyCanExecuteChanged();
    }

    partial void OnMouvementSelectionneChanged(MouvementEspece? value)
    {
        SupprimerCommand.NotifyCanExecuteChanged();

        if (value != null)
        {
            TypeMouvement = value.MouvType;
            MontantTexte = Math.Abs(value.MouvMontant).ToString("N2", CultureInfo.CurrentCulture);
            Description = value.MouvDescription;
            DateMouvement = value.MouvDate;
            CompteSelectionne = Comptes.FirstOrDefault(c => c.CpteId == value.MouvCpteId);

            if (value.MouvType == TypeMouvement.Virement && value.MouvCpteDestId.HasValue)
                CompteDestination = Comptes.FirstOrDefault(c => c.CpteId == value.MouvCpteDestId);
        }
    }

    [RelayCommand(CanExecute = nameof(CanEnregistrer))]
    private void Enregistrer()
    {
        if (CompteSelectionne == null) return;

        var montant = decimal.Parse(MontantTexte, NumberStyles.Number, CultureInfo.CurrentCulture);
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

    private bool CanEnregistrer()
    {
        if (CompteSelectionne == null) return false;
        if (!decimal.TryParse(MontantTexte, NumberStyles.Number, CultureInfo.CurrentCulture, out var montant))
            return false;
        if (montant <= 0) return false;
        if (TypeMouvement == TypeMouvement.Virement && CompteDestination == null) return false;
        return true;
    }

    [RelayCommand]
    private void Annuler()
    {
        MouvementSelectionne = null;
        MontantTexte = string.Empty;
        Description = string.Empty;
        CompteDestination = null;
        DateMouvement = DateTime.Today;
        TypeMouvement = TypeMouvement.Depot;
    }

    [RelayCommand(CanExecute = nameof(CanSupprimer))]
    private void Supprimer()
    {
        if (MouvementSelectionne == null) return;
        _mouvementRepository.Delete(MouvementSelectionne.MouvId);
        ChargerMouvements();
        Annuler();
    }

    private bool CanSupprimer() => MouvementSelectionne != null;

    private void ChargerMouvements()
    {
        if (CompteSelectionne == null)
        {
            Mouvements.Clear();
            return;
        }

        Mouvements.Clear();
        foreach (var mouvement in _mouvementRepository.GetAllByCompte(CompteSelectionne.CpteId))
            Mouvements.Add(mouvement);
    }
}