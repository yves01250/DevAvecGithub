using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using System.Net.Http;
using System.Text.Json;
using SuiviPortefolio.Data;
using SuiviPortefolio.Comptes.CompteModel;
using SuiviPortefolio.Comptes.CompteRepository;
using SuiviPortefolio.Comptes.CompteView;

namespace SuiviPortefolio.Comptes.CpteViewModel;


public class CompteViewModel : INotifyPropertyChanged
{
    private readonly ICompteRepository _repository;
    private readonly CotationFetcher _cotationFetcher = new();
    private Compte? _compteSelectionne;
    private string _valorisationStatus =
        "Sélectionnez l’onglet Comptes pour calculer les valorisations.";
    private bool _isRefreshingValorisations;

    public CompteViewModel(ICompteRepository? repository = null)
    {
        _repository = repository ?? new CompteRepository.CompteRepository(
            DatabaseLocation.DatabasePath);

        AjouterCompteCommand = new RelayCommand(async _ => await AjouterCompteAsync());
        ModifierCompteCommand = new RelayCommand(async _ => await ModifierCompteAsync(),
            _ => CompteSelectionne != null);
        SetCompteDefautCommand = new RelayCommand(async _ => await DefinirCompteDefautAsync(), _ => CompteSelectionne != null);
        ActualiserValorisationsCommand = new RelayCommand(
            async _ => await ChargerValorisationsAsync(),
            _ => !_isRefreshingValorisations);

        ChargerComptes();
    }

    public ObservableCollection<Compte> Comptes { get; } = new();
    public ObservableCollection<CompteValorisation> ValorisationComptes { get; } = new();

    public string ValorisationStatus
    {
        get => _valorisationStatus;
        private set
        {
            if (_valorisationStatus == value)
                return;

            _valorisationStatus = value;
            OnPropertyChanged();
        }
    }

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
    public ICommand ActualiserValorisationsCommand { get; }

    public void ChargerComptes()
    {
        Comptes.Clear();
        foreach (var compte in _repository.GetAll())
        {
            Comptes.Add(compte);
        }

        ValorisationComptes.Clear();
        CompteSelectionne = Comptes.FirstOrDefault(compte => compte.CpteEstDefaut)
            ?? Comptes.FirstOrDefault();
    }

    public async Task ChargerValorisationsAsync()
    {
        if (_isRefreshingValorisations)
            return;

        _isRefreshingValorisations = true;
        ((RelayCommand)ActualiserValorisationsCommand).RaiseCanExecuteChanged();
        ValorisationComptes.Clear();
        ValorisationStatus = "Récupération des taux de change Yahoo Finance...";

        try
        {
            var expositions = _repository.GetExpositions();
            var paires = expositions
                .Where(exposition => !string.IsNullOrWhiteSpace(exposition.ActifDevise)
                    && !string.Equals(
                        exposition.ActifDevise,
                        exposition.CpteDevise,
                        StringComparison.OrdinalIgnoreCase))
                .Select(exposition => (Depuis: exposition.ActifDevise!.ToUpperInvariant(),
                    Vers: exposition.CpteDevise.ToUpperInvariant()))
                .Distinct()
                .ToList();

            var taux = new Dictionary<(string Depuis, string Vers), decimal>();
            var erreursTaux = new Dictionary<(string Depuis, string Vers), string>();

            foreach (var paire in paires)
            {
                if (!EstCodeDeviseValide(paire.Depuis) || !EstCodeDeviseValide(paire.Vers))
                {
                    erreursTaux[paire] = "code de devise invalide";
                    continue;
                }

                try
                {
                    var cotation = await _cotationFetcher.FetchSnapshotAsync(
                        $"{paire.Depuis}{paire.Vers}=X");

                    if (cotation == null || !double.IsFinite(cotation.Close) || cotation.Close <= 0)
                        throw new InvalidOperationException("Yahoo Finance n’a pas retourné de taux valide.");

                    if (!string.Equals(cotation.Devise, paire.Vers, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            $"Yahoo Finance a retourné le taux en {cotation.Devise ?? "devise inconnue"}.");
                    }

                    taux[paire] = (decimal)cotation.Close;
                }
                catch (Exception exception) when (
                    exception is HttpRequestException
                    or TaskCanceledException
                    or InvalidOperationException
                    or JsonException
                    or IndexOutOfRangeException
                    or KeyNotFoundException
                    or FormatException
                    or OverflowException)
                {
                    erreursTaux[paire] = exception.Message;
                }
            }

            foreach (var compte in expositions.GroupBy(exposition => exposition.CpteId))
            {
                var lignes = compte.ToList();
                var erreursCompte = new List<string>();
                decimal valorisationActions = 0m;

                foreach (var exposition in lignes)
                {
                    if (string.IsNullOrWhiteSpace(exposition.ActifDevise))
                        continue;

                    var deviseActif = exposition.ActifDevise.ToUpperInvariant();
                    var deviseCompte = exposition.CpteDevise.ToUpperInvariant();
                    if (string.Equals(deviseActif, deviseCompte, StringComparison.OrdinalIgnoreCase))
                    {
                        valorisationActions += exposition.ValorisationActifs;
                    }
                    else if (taux.TryGetValue((deviseActif, deviseCompte), out var tauxChange))
                    {
                        valorisationActions += exposition.ValorisationActifs * tauxChange;
                    }
                    else
                    {
                        var paire = (Depuis: deviseActif, Vers: deviseCompte);
                        var erreur = erreursTaux.TryGetValue(paire, out var details)
                            ? details
                            : "taux de change indisponible";
                        erreursCompte.Add($"{deviseActif}/{deviseCompte} : {erreur}");
                    }
                }

                var compteReference = lignes[0];
                var statut = string.Join("; ", erreursCompte.Distinct());
                var valorisationDisponible = erreursCompte.Count == 0;
                ValorisationComptes.Add(new CompteValorisation
                {
                    CpteNom = compteReference.CpteNom,
                    CpteType = compteReference.CpteType,
                    CpteDevise = compteReference.CpteDevise,
                    SoldeEspeces = compteReference.CpteSolde,
                    ValorisationActions = valorisationDisponible ? valorisationActions : null,
                    SoldeTotal = valorisationDisponible
                        ? compteReference.CpteSolde + valorisationActions
                        : null,
                    Statut = statut
                });
            }

            var erreursAffichees = ValorisationComptes
                .Where(compte => !string.IsNullOrWhiteSpace(compte.Statut))
                .Select(compte => $"{compte.CpteNom} : {compte.Statut}")
                .ToList();

            ValorisationStatus = erreursAffichees.Count == 0
                ? "Valorisation aux derniers cours enregistrés. Taux Yahoo Finance utilisés pour les conversions nécessaires."
                : $"Conversion indisponible — total non calculé pour les comptes concernés. {string.Join(" | ", erreursAffichees)}";
        }
        finally
        {
            _isRefreshingValorisations = false;
            ((RelayCommand)ActualiserValorisationsCommand).RaiseCanExecuteChanged();
        }
    }

    private static bool EstCodeDeviseValide(string devise) =>
        devise.Length == 3 && devise.All(char.IsAsciiLetter);

    private async Task AjouterCompteAsync()
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
        await ChargerValorisationsAsync();
    }

    private async Task ModifierCompteAsync()
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
        await ChargerValorisationsAsync();
    }

    private async Task DefinirCompteDefautAsync()
    {
        if (CompteSelectionne == null)
            return;

        _repository.SetDefault(CompteSelectionne.CpteId);
        ChargerComptes();
        await ChargerValorisationsAsync();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private class RelayCommand : ICommand
    {
        private readonly Func<object?, Task> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Func<object?, Task> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
            : this(parameter =>
            {
                execute(parameter);
                return Task.CompletedTask;
            }, canExecute)
        {
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public async void Execute(object? parameter)
        {
            await _execute(parameter);
        }

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
