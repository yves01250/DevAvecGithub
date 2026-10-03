using System.Globalization;
using System.Windows;
using SuiviPortefolio.Data;

// TODO: Implémenter la recherche dans la liste
// FIXME: Corriger le bug d'affichage lorsque aucune donnée SQLite n'est trouvée
// AFFAIRE: Ajouter une migration de schéma pour la base SQLite
// AMELIORATION: Ajouter des tests unitaires au dépôt de données
// BUG: Ajouter un mécanisme de journalisation pour le dépôt de données
// NOTE: Ajouter un mécanisme de journalisation pour le dépôt de données
// TODO: Ajouter un mécanisme de journalisation pour le dépôt de données

namespace SuiviPortefolio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    
    // TEST: Ajouter un mécanisme de journalisation pour le dépôt de données
    public partial class App : Application
    {
        public App()
        {
            CultureInfo.DefaultThreadCurrentCulture =
                CultureInfo.GetCultureInfo("fr-FR");

            CultureInfo.DefaultThreadCurrentUICulture =
                CultureInfo.GetCultureInfo("fr-FR");

            DatabaseLocation.Initialize();
        }
    }

}
