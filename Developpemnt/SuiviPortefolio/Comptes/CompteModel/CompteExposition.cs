namespace SuiviPortefolio.Comptes.CompteModel;

public class CompteExposition
{
    public int CpteId { get; init; }
    public string CpteNom { get; init; } = string.Empty;
    public string CpteType { get; init; } = string.Empty;
    public string CpteDevise { get; init; } = string.Empty;
    public decimal CpteSolde { get; init; }
    public string? ActifDevise { get; init; }
    public decimal ValorisationActifs { get; init; }
}
