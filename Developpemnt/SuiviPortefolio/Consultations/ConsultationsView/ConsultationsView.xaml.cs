using System.Windows.Controls;
using SuiviPortefolio.Data;
using SuiviPortefolio.Consultations.ConsultationsViewModel;


namespace SuiviPortefolio.Consultations.ConsultationsView
{
    public partial class ConsultationsView : UserControl
    {
        public ConsultationsView()
        {
            InitializeComponent();
            var viewModel = new PortfolioViewModel(
                new ConsultPositionRepository(DatabaseLocation.DatabasePath));

            DataContext = viewModel;
            Loaded += async (_, _) => await viewModel.LoadAsync();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
