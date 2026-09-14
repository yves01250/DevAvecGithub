using System;

namespace SuiviPortefolio.Transactions.TransModel;

public class Cotation
{
    public long Id { get; set; }
    public string? Symbol { get; set; }   // ex. "PSP5.PA"
    public string? Isin { get; set; }
    public string? Name { get; set; }    // nom long (nom de l'Actif Financier)
    public string? Instrument { get; set; }  // type d'instrument (ETF, action, etc.)
    public string? Marche { get; set; }   // marché (ex. "Paris")
    public string? Devise { get; set; }   // devise (ex. "EUR")
    public double Close { get; set; }    // cours de clôture
    public DateTime Date { get; set; }   // date du cours
    public DateTime CreatedAt { get; set; }
}