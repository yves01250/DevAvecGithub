using System.Windows.Controls;
using MouvementViewModelType = SuiviPortefolio.Transactions.MouvementViewModel.MouvementViewModel;

namespace SuiviPortefolio.Transactions.MouvementView;

public partial class MouvementView : UserControl
{
    public MouvementView()
    {
        InitializeComponent();
        DataContext = new MouvementViewModelType();
    }
}
