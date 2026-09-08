using System.Windows.Controls;
using System.Windows;
using SuiviPortefolio.Transactions.TransModel;
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

        private void AjouterCotation_Click(object sender, RoutedEventArgs e)
        {
            var nouvelleCotation = new Cotation
            {
                Name = "Nouvelle Cotation"
            };

            var dialog = new TransEditWindow(nouvelleCotation, nouveau: true)
            {
                Owner = Window.GetWindow(this)
            };

            dialog.ShowDialog();
        }
    }
}