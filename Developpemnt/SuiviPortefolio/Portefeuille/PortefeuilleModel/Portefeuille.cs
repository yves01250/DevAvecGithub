namespace SuiviPortefolio.Portefeuille.PortefeuilleModel;

public class monPortefeuille
{
    public int PtfId { get; set; }
    public string PtfNom { get; set; } = string.Empty;
    public string PtfType { get; set; } = "Général";
    public string PtfDevise { get; set; } = "EUR";
    public decimal PtfSolde { get; set; }
    public bool PtfEstDefaut { get; set; }
}