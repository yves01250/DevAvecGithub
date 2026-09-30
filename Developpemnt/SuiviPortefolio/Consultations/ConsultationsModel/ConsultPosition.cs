
namespace SuiviPortefolio.Consultations.ConsultationsModel;


public class ConsultPosition
{
    public int Id { get; set; }
    public string Ticker { get; set; }
    public string Nom { get; set; }
    public string Type { get; set; } // "Stock", "Crypto", "ETF"...
    public decimal Quantite { get; set; }
    public decimal PrixMoyen { get; set; }
    public DateTime Date { get; set; }
}