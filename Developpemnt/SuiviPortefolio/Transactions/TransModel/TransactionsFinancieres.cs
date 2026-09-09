namespace SuiviPortefolio.Transactions.TransModel;

public class TransactionFinanciere
{
    public long TransId { get; set; }
    public string TransType { get; set; } = "Achat";
    public decimal TransQte { get; set; }
    public DateTime TransDateTransac { get; set; }
    public int TransCpteId { get; set; }
    public long TransActifId { get; set; }
    public decimal TransPrix { get; set; }
    public decimal TransFrais { get; set; }
    public string CompteNom { get; set; } = string.Empty;
}