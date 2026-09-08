using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SuiviPortefolio.Transactions.TransModel;
/*using SuiviPortefolio.Transactions.TransRepository;*/
using SuiviPortefolio.Transactions.TransView;

namespace SuiviPortefolio.Transactions.TransViewModel
{
    public class TransactionViewModel : INotifyPropertyChanged
    {
    private void AjouterCotation()
    {
    var nouvelleCotation = new Cotation
    {
        Name = "Nouvelle Cotation",
    };

    var dialog = new TransEditWindow(nouvelleCotation, nouveau: true);
    if (dialog.ShowDialog() != true)
        return;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    }
}