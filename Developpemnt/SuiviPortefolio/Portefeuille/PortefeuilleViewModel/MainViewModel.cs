using System.Windows;
using Microsoft.Data.Sqlite;
using SuiviPortefolio.Portefeuille.PortefeuilleModel;
using System;
using System.ComponentModel;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Windows.Media.Animation;
using SuiviPortefolio.Data;
using System.Drawing.Text;
using System.IO;
using System.Collections.ObjectModel;


namespace SuiviPortefolio.Portefeuille.PortefeuilleViewModel;

public class MainViewModel : INotifyPropertyChanged
{

    private readonly SqliteRepository _database;
    private monPortefeuille _portefeuille;

    //private monPortefeuille _portefeuille;

    //private monPortefeuille Portefeuille { get; set; }

    public MainViewModel()
    {

        var dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SuiviPortefeuille.sqlite");
            _database = new SqliteRepository(dbPath);

        Portefeuille = new monPortefeuille();
            {
                Portefeuille.PtfNom = "Mon Portefeuille";
                Portefeuille.PtfSolde = 1500.50m;
            };
            
            ChargerPortefeuilles();
    }
            public ObservableCollection<monPortefeuille> ListePortefeuilles { get; } = new();

        private monPortefeuille _portefeuilleSelectionne;

        public monPortefeuille PortefeuilleSelectionne
        {

            get => _portefeuilleSelectionne;
            set
            {
                if (_portefeuilleSelectionne != value)
                {
                    _portefeuilleSelectionne = value;
                    OnPropertyChanged();
                    // Quand on change de sélection, on en fait le portefeuille courant
                    Portefeuille = value;
                }
            }
        }

        

        public void ChargerPortefeuilles()
        {
            // À implémenter dans SqliteRepository : IEnumerable<monPortefeuille> GetAll()
            var tous = _database.GetAllPortefeuilles(); 
            ListePortefeuilles.Clear();
            foreach (var p in tous)
                ListePortefeuilles.Add(p);

            // Optionnel : choisir un “défaut” (ex. le premier, ou celui avec un flag)
            PortefeuilleSelectionne = ListePortefeuilles.FirstOrDefault();
        }
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
                OnPropertyChanged(nameof(Portefeuille));
            }
        }
    }

    public decimal Solde
    {
                get => Portefeuille.PtfSolde;
        set
        {
            if (Portefeuille.PtfSolde != value)
            {
                Portefeuille.PtfSolde = value;
                OnPropertyChanged();
            }
        }
    }
    //private readonly SqliteRepository _database = new SqliteRepository("SuiviPortefeuille.sqlite");
    public void EnregistrerPortefeuille()
    {

             _database.Ajouter(Portefeuille);
    }
/*        public MainViewModel()
            { 
            Portefeuille = new monPortefeuille();
            }
*/
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }    
}

