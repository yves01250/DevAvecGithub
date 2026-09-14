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
using System.Diagnostics;



namespace SuiviPortefolio.Portefeuille.PortefeuilleView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool IsDarkTheme { get; set; } = false;
        //private readonly MonPf _viewModel;
        public MainWindow()
        {
            InitializeComponent();

                // Récupère la police par défaut de la fenêtre (dont héritent tous les TextBlock)
    FontFamily currentFont = this.FontFamily;
    double fontSize = this.FontSize;
    Debug.WriteLine($"Police par défaut : {currentFont.Source}");
    Debug.WriteLine($"Taille par défaut : {fontSize}");    
    // Ou l'afficher dans une boîte de message au lancement :
    //MessageBox.Show($"Police : {currentFont.Source} | Taille : {fontSize}");

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

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
