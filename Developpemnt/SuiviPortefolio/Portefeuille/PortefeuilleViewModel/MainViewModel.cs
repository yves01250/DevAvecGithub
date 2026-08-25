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

    
    private string _nomPortefeuille;

    public string NomPortefeuille
    {
        get
        {
            return _nomPortefeuille;
        }
        set
        {
            if (_nomPortefeuille != value)
            {
                _nomPortefeuille = value;
                OnPropertyChanged();
            }
        }
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

/*
public class MainViewModel()
{
    public class MonPortefeuille
    { 
        MonPortefeuille portefeuille = new MonPortefeuille();


    };

}
    
    
public class PortefeuilleViewModel
    {
        
        public class MonPortefeuille
        {
          
            MonPortefeuille portefeuille = new MonPortefeuille();
                

        };
        /*
         
        public PortefeuilleModel.Portefeuille { get; set; }

        public decimal ValeurTotale
        {
            get
            {
                decimal total = Portefeuille.Liquidites;

                foreach (var actif in Portefeuille.Actifs)
                {
                    total += actif.Prix * actif.Quantite;
                }

                return total;
            }
        }
      
    }
    
}
*/




