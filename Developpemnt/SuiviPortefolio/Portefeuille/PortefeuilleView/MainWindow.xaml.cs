using Microsoft.Data.Sqlite;
using SuiviPortefolio.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SuiviPortefolio.Portefeuille.PortefeuilleModel;
using SuiviPortefolio.Portefeuille.PortefeuilleViewModel;

namespace SuiviPortefolio.Portefeuille.PortefeuilleView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SqliteRepository _database;
        //private readonly MonPf _viewModel;
        public MainWindow()
        {
            InitializeComponent();
            var dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SuiviPortefeuille.sqlite");
            _database = new SqliteRepository(dbPath);

            //var etfService = new EtfService(dbPath);

            //_viewModel = new MonPf(Portefeuille.cs);
            //_repository = new PortefeuilleBase(SqliteRepository); // Initialise la base et la table ici
            //InitializeDatabase();
            DataContext = new MainViewModel(); // ViewModel associé
            
        }
    }
}