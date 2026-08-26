using Microsoft.Data.Sqlite;
using SuiviPortefolio.Portefeuille.PortefeuilleModel;
using System;
using System.ComponentModel;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Windows.Media.Animation;

namespace SuiviPortefolio.Portefeuille.PortefeuilleViewModel;

public class MainViewModel : INotifyPropertyChanged
{
    //public string NomPortefeuille { get; set; } = "Mon Portefeuille";

    
    private monPortefeuille _portefeuille;

    public monPortefeuille Portefeuille
    {
        get
        {
            return _portefeuille;
        }
        set
        {
            if (_portefeuille != value)
            {
                _portefeuille = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NomPortefeuille));
            }
        }
    }
     public string NomPortefeuille
    {
        get => Portefeuille.NomPortefeuille;
        set
        {
            if (Portefeuille.NomPortefeuille != value)
            {
                Portefeuille.NomPortefeuille = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal Solde
    {
                get => Portefeuille.Solde;
        set
        {
            if (Portefeuille.Solde != value)
            {
                Portefeuille.Solde = value;
                OnPropertyChanged();
            }
        }
    }
        public MainViewModel()
    {
        Portefeuille = new monPortefeuille();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}

