using System.Windows.Controls;
using SuiviPortefolio.Comptes.CpteViewModel;



namespace SuiviPortefolio.Comptes.CompteView
{
    public partial class CompteView : UserControl
    {
        public CompteView()
        {
            InitializeComponent();
            DataContext = new CompteViewModel();

        }
    }
}
