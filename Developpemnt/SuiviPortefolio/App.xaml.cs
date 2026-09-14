using System.Configuration;
using System.Data;
using System.Globalization;
using System.Windows;

namespace SuiviPortefolio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            CultureInfo.DefaultThreadCurrentCulture =
                CultureInfo.GetCultureInfo("fr-FR");

            CultureInfo.DefaultThreadCurrentUICulture =
                CultureInfo.GetCultureInfo("fr-FR");
        }
    }

}
