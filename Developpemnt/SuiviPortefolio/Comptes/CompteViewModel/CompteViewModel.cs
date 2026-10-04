using System.Collections.ObjectModel;
using System.Windows;
using System.Net.Http;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SuiviPortefolio.Data;
using SuiviPortefolio.Comptes.CompteModel;
using SuiviPortefolio.Comptes.CompteRepository;
using SuiviPortefolio.Comptes.CompteView;

namespace SuiviPortefolio.Comptes.CpteViewModel;

// TODO: Créeer ViewModel pour CompteEditWindow (Découmpler logique de la view)

[ObservableObject]
public partial class CompteViewModel
{
    private readonly ICompteRepository _repository;
    private readonly CotationFetcher _cotationFetcher = new();

    public CompteViewModel(ICompteRepository? repository = null)
    {
        _repository = repository ?? new CompteRepository.CompteRepository(
            DatabaseLocation.DatabasePath);

        ChargerComptes();
    }

    // ✅ Collections (pas de changement nécessaire)
    public ObservableCollection<Compte> Comptes { get; } = new();
    public ObservableCollection<CompteValorisation> ValorisationComptes { get; } = new();

    // ✅ Propriétés avec [ObservableProperty]
    [ObservableProperty]
    private Compte? _compteSelectionne;

    [ObservableProperty]
    private string _valorisationStatus =
        "Sélectionnez l’onglet Comptes pour calculer les valorisations.";

    [ObservableProperty]
    private bool _isRefreshingValorisations;

    // ✅ Commandes avec [RelayCommand]
    [RelayCommand]
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
        CompteSelectionne = Comptes.FirstOrDefault(c => c.CpteId == nouveauCompteId);
        await ChargerValorisationsAsync();
    }

    [RelayCommand(CanExecute = nameof(CanModifyCompte))]
    private async Task ModifierCompteAsync()
    {
        if (CompteSelectionne == null) return;

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
        CompteSelectionne = Comptes.FirstOrDefault(c => c.CpteId == compteModifie.CpteId);
        await ChargerValorisationsAsync();
    }

    private bool CanModifyCompte() => CompteSelectionne != null;

    [RelayCommand(CanExecute = nameof(CanSetCompteDefaut))]
    private async Task DefinirCompteDefautAsync()
    {
        if (CompteSelectionne == null) return;

        _repository.SetDefault(CompteSelectionne.CpteId);
        ChargerComptes();
        await ChargerValorisationsAsync();
    }

    private bool CanSetCompteDefaut() => CompteSelectionne != null;

    [RelayCommand(CanExecute = nameof(CanRefreshValorisations))]
    public async Task ChargerValorisationsAsync()
    {
        if (IsRefreshingValorisations) return;

        IsRefreshingValorisations = true;
        ValorisationComptes.Clear();
        ValorisationStatus = "Récupération des taux de change Yahoo Finance...";

        try
        {
            var expositions = _repository.GetExpositions();
            var paires = expositions
                .Where(exposition => !string.IsNullOrWhiteSpace(exposition.ActifDevise)
                    && !string.Equals(exposition.ActifDevise, exposition.CpteDevise, StringComparison.OrdinalIgnoreCase))
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
                    var cotation = await _cotationFetcher.FetchSnapshotAsync($"{paire.Depuis}{paire.Vers}=X");

                    if (cotation == null || !double.IsFinite(cotation.Close) || cotation.Close <= 0)
                        throw new InvalidOperationException("Yahoo Finance n’a pas retourné de taux valide.");

                    if (!string.Equals(cotation.Devise, paire.Vers, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException($"Yahoo Finance a retourné le taux en {cotation.Devise ?? "devise inconnue"}.");

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
                    if (string.IsNullOrWhiteSpace(exposition.ActifDevise)) continue;

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
                        var erreur = erreursTaux.TryGetValue(paire, out var details) ? details : "taux de change indisponible";
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
                    SoldeTotal = valorisationDisponible ? compteReference.CpteSolde + valorisationActions : null,
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
            IsRefreshingValorisations = false;
        }
    }

    private bool CanRefreshValorisations() => !IsRefreshingValorisations;

    public void ChargerComptes()
    {
        Comptes.Clear();
        foreach (var compte in _repository.GetAll())
        {
            Comptes.Add(compte);
        }

        ValorisationComptes.Clear();
        CompteSelectionne = Comptes.FirstOrDefault(compte => compte.CpteEstDefaut) ?? Comptes.FirstOrDefault();
    }

    private static bool EstCodeDeviseValide(string devise) =>
        devise.Length == 3 && devise.All(char.IsAsciiLetter);
}