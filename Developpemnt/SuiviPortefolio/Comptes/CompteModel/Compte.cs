namespace SuiviPortefolio.Comptes.CompteModel;

public enum TypeCompte
{
    PEA,
    CompteTitre,
    CompteCourant,
    Livret,
    Autre
}

public class Compte
{
    public int CpteId { get; set; }
    public string CpteNom { get; set; } = string.Empty;
    public TypeCompte CpteType { get; set; }
    public string CpteDevise { get; set; } = "EUR";
    public decimal CpteSolde { get; set; }
    public bool CpteEstDefaut { get; set; }
    public int CptePtfId { get; set; }
}
