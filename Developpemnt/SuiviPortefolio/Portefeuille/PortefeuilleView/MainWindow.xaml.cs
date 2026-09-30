using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using SuiviPortefolio.Data;
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
        //choix du thème par défaut (clair ou sombre)
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

        private void MenuSauvegarder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Sauvegarder la base de données",
                Filter = "Base de données SQLite (*.sqlite)|*.sqlite|Tous les fichiers (*.*)|*.*",
                FileName = "SuiviPortefeuille-sauvegarde.sqlite",
                AddExtension = true,
                OverwritePrompt = true
            };

            if (dialog.ShowDialog(this) != true)
                return;

            if (DatabaseManager.BackupDatabase(GetDatabasePath(), dialog.FileName))
            {
                MessageBox.Show(
                    "La base de données a été sauvegardée.",
                    "Sauvegarde",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void MenuChoisirDossierBase_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Choisir le dossier de la base de données",
                InitialDirectory = DatabaseLocation.DirectoryPath,
                Multiselect = false
            };

            if (dialog.ShowDialog(this) != true)
                return;

            var selectedDirectory = Path.GetFullPath(dialog.FolderName);
            if (string.Equals(
                    selectedDirectory,
                    DatabaseLocation.DirectoryPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var selectedDatabasePath = Path.Combine(selectedDirectory, "SuiviPortefeuille.sqlite");
            if (File.Exists(selectedDatabasePath) &&
                MessageBox.Show(
                    "Ce dossier contient déjà une base de données. Elle sera remplacée par la base actuellement utilisée. Continuer ?",
                    "Remplacer la base existante",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                if (!DatabaseLocation.ChangeDirectory(selectedDirectory))
                    return;

                ReloadWithConfiguredDatabase();
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    $"Impossible de déplacer la base de données : {exception.Message}",
                    "Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void MenuRestaurer_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Restaurer une base de données",
                Filter = "Base de données SQLite (*.sqlite)|*.sqlite|Tous les fichiers (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false
            };

            if (dialog.ShowDialog(this) != true)
                return;

            if (MessageBox.Show(
                    "La restauration remplacera les données actuelles. Voulez-vous continuer ?",
                    "Restaurer la base de données",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            SqliteConnection.ClearAllPools();
            if (DatabaseManager.RestoreDatabase(dialog.FileName, GetDatabasePath()))
            {
                ReloadWithConfiguredDatabase();
                MessageBox.Show(
                    "La base de données a été restaurée.",
                    "Restauration",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void MenuQuitter_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private static string GetDatabasePath() => DatabaseLocation.DatabasePath;

        private void ReloadWithConfiguredDatabase()
        {
            var newWindow = new MainWindow();
            Application.Current.MainWindow = newWindow;
            newWindow.Show();
            Close();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        }
    }
}
