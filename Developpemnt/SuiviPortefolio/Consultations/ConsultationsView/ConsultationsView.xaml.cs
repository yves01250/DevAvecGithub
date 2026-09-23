using System.Windows.Controls;
using System.IO;
using SuiviPortefolio.Data;
using SuiviPortefolio.Consultations.ConsultationsViewModel;


namespace SuiviPortefolio.Consultations.ConsultationsView
{
    public partial class ConsultationsView : UserControl
    {
        public ConsultationsView()
        {
            InitializeComponent();
            var databasePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "SuiviPortefeuille.sqlite");
            var viewModel = new PortfolioViewModel(
                new ConsultPositionRepository(databasePath));

            DataContext = viewModel;
            Loaded += async (_, _) => await viewModel.LoadAsync();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
