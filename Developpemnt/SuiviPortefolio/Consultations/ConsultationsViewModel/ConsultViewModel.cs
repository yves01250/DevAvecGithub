using CommunityToolkit.Mvvm.ComponentModel;
using SuiviPortefolio.Consultations.ConsultationsModel;

namespace SuiviPortefolio.Consultations.ConsultationsViewModel;

[ObservableObject]
public partial class PositionViewModel
{
    public ConsultPosition Model { get; }

    // Propriétés calculées (read-only, basées sur Model)
    public int Id => Model.Id;
    public string Ticker => Model.Ticker;
    public string Name => Model.Nom;
    public string Type => Model.Type;
    public decimal Quantity => Model.Quantite;
    public decimal Price => Model.PrixMoyen;
    public DateTime Date => Model.Date;

    // ✅ Propriété avec notification automatique
    [ObservableProperty]
    private bool _isSelected;

    public PositionViewModel(ConsultPosition model)
    {
        Model = model;
    }

    // Méthode de mise à jour
    public void UpdatePrice(decimal newPrice, DateTime date)
    {
        Model.PrixMoyen = newPrice;
        Model.Date = date;

        // ✅ OnPropertyChanged est fourni par ObservableObject
        OnPropertyChanged(nameof(Price));
        OnPropertyChanged(nameof(Date));
    }
}