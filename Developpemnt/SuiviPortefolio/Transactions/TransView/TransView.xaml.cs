using System.Windows.Controls;
using System.Windows;
using SuiviPortefolio.Transactions.TransViewModel;



namespace SuiviPortefolio.Transactions.TransView
{
    public partial class TransView : UserControl
    {
        public TransView()
        {
            InitializeComponent();
            DataContext = new TransactionViewModel();
        }

        private void RechercherCotation_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is TransactionViewModel viewModel)
            {
                var owner = Window.GetWindow(this);
                if (owner != null)
                    viewModel.RechercherCotation(owner);
            }
        }

        private void Enregistrer_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is TransactionViewModel viewModel)
                viewModel.Enregistrer();
        }

        private void Annuler_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is TransactionViewModel viewModel)
                viewModel.Annuler();
        }
    }
}