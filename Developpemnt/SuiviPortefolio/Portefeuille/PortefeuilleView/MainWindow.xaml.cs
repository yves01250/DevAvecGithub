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
using System.Windows;



namespace SuiviPortefolio.Portefeuille.PortefeuilleView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool IsDarkTheme { get; set; } = false;
        private readonly SqliteRepository _database;
        //private readonly MonPf _viewModel;
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel(); // ViewModel associé
            
        }
        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button
                ?? throw new InvalidOperationException("Le changement de thème doit être déclenché par un bouton.");

            if (IsDarkTheme)
            {
                ThemeManager.ApplyLightTheme();
                button.Content = "🌓 Basculer en mode sombre";
            }
            else
            {
                ThemeManager.ApplyDarkTheme();
                button.Content = "☀️ Basculer en mode clair";
            }
            IsDarkTheme = !IsDarkTheme;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source == sender &&
                ComptesTab.IsSelected &&
                ComptesView.DataContext is SuiviPortefolio.Comptes.CpteViewModel.CompteViewModel viewModel)
            {
                viewModel.ChargerComptes();
            }
        }

            private void Enregistrer_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.EnregistrerPortefeuille();
            }
        }
    }
}