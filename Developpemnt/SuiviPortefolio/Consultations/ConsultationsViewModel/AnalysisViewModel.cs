// ViewModels/AnalysisViewModel.cs
using System.Collections.ObjectModel;
using System.Linq;
using SuiviPortefolio.Consultations.ConsultationsModel;

public class AnalysisViewModel
{
    public ObservableCollection<PositionSummary> SelectedPositions { get; }
    public int Count { get; }
    public decimal TotalValue { get; }
    public decimal AvgPrice { get; }
    public string TypesSummary { get; }

    public AnalysisViewModel(System.Collections.Generic.IList<ConsultPosition> positions)
    {
        SelectedPositions = new ObservableCollection<PositionSummary>(
            positions.Select(p => new PositionSummary(p))
        );

        Count = positions.Count;
        TotalValue = positions.Sum(p => p.PrixMoyen * p.Quantite);
        AvgPrice = positions.Any() ? positions.Average(p => p.PrixMoyen) : 0;

        TypesSummary = string.Join(
            ", ",
            positions.GroupBy(p => p.Type)
                     .Select(g => $"{g.Key}: {g.Count()}")
        );
    }
}

public class PositionSummary
{
    public string Ticker { get; }
    public string Type { get; }
    public decimal Quantity { get; }
    public decimal Price { get; }
    public decimal Value { get; }

    public PositionSummary(ConsultPosition p)
    {
        Ticker = p.Ticker;
        Type = p.Type;
        Quantity = p.Quantite;
        Price = p.PrixMoyen;
        Value = p.PrixMoyen * p.Quantite;

    }
}