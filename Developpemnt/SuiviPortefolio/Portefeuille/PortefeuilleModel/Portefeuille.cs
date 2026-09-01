namespace SuiviPortefolio.Portefeuille.PortefeuilleModel;

public class monPortefeuille
{
    public int PtfId { get; set; }
    public string PtfNom { get; set; } = string.Empty;
    public decimal PtfSolde { get; set; }
    public bool PtfEstDefaut { get; set; }

}