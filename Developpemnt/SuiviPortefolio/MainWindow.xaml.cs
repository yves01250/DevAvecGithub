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

namespace SuiviPortefolio.Portefeuille.PortefeuilleView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SuiviPortefeuille.sqlite");
            //var etfService = new EtfService(dbPath);
            //_viewModel = new MainViewModel(etfService);
            this.DataContext = new MainViewModel(); // ViewModel associé
        }
    }
}