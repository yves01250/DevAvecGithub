namespace SuiviPortefolio.Comptes.CompteModel;

public class CompteValorisation
{
    public string CpteNom { get; init; } = string.Empty;
    public string CpteType { get; init; } = string.Empty;
    public string CpteDevise { get; init; } = string.Empty;
    public decimal SoldeEspeces { get; init; }
    public decimal? ValorisationActions { get; init; }
    public decimal? SoldeTotal { get; init; }
    public string Statut { get; init; } = string.Empty;
}
