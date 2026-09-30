using SuiviPortefolio.Consultations.ConsultationsModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;



namespace SuiviPortefolio.Consultations.ConsultationsViewModel;

public class PositionViewModel : INotifyPropertyChanged
{
    private bool _isSelected;

    public ConsultPosition Model { get; }

    public int Id => Model.Id;
    public string Ticker => Model.Ticker;
    public string Name => Model.Nom;
    public string Type => Model.Type;
    public decimal Quantity => Model.Quantite;
    public decimal Price => Model.PrixMoyen;
    public DateTime Date => Model.Date;

    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); }
    }

    public PositionViewModel(ConsultPosition model)
    {
        Model = model;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // Pour mettre à jour le cours sans changer la référence
    public void UpdatePrice(decimal newPrice, DateTime date)
    {
        // On ne modifie pas Model directement si tu veux garder le modèle immutable,
        // mais ici on suppose que Model est mutable.
        Model.PrixMoyen = newPrice;
        Model.Date = date;

        OnPropertyChanged(nameof(Price));
        OnPropertyChanged(nameof(Date));
    }
}

