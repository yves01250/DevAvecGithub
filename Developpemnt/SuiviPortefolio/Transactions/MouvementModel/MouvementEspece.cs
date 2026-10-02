namespace SuiviPortefolio.Transactions.MouvementModel;

public enum TypeMouvement
{
    Depot,
    Retrait,
    Virement
}

public class MouvementEspece
{
    public long MouvId { get; set; }
    public TypeMouvement MouvType { get; set; }
    public decimal MouvMontant { get; set; }
    public DateTime MouvDate { get; set; }
    public int MouvCpteId { get; set; }
    public int? MouvCpteDestId { get; set; }
    public string MouvDescription { get; set; } = string.Empty;
    public string CompteNom { get; set; } = string.Empty;
    public string? CompteDestNom { get; set; }
}
