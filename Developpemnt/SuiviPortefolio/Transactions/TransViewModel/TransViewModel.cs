using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using SuiviPortefolio.Comptes.CompteModel;
using SuiviPortefolio.Comptes.CompteRepository;
using SuiviPortefolio.Transactions.TransModel;
using SuiviPortefolio.Transactions.TransRepository;
using SuiviPortefolio.Transactions.TransView;

namespace SuiviPortefolio.Transactions.TransViewModel;

public class TransactionViewModel : INotifyPropertyChanged
{
    private readonly ITransRepository _transactions;
    private readonly ICompteRepository _comptes;
    private Cotation? _cotationSelectionnee;
    private TransactionFinanciere? _transactionSelectionnee;
    private Compte? _compteSelectionne;
    private string _quantiteTexte = string.Empty;
    private string _fraisTexte = "0";
    private string _totalTexte = "0,00";
    private string _typeTransaction = "Achat";

    public TransactionViewModel(ITransRepository? transactions = null, ICompteRepository? comptes = null)
    {
        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SuiviPortefeuille.sqlite");
        _transactions = transactions ?? new SuiviPortefolio.Transactions.TransRepository.TransRepository(dbPath);
        _comptes = comptes ?? new CompteRepository(dbPath);
        foreach (var compte in _comptes.GetAll())
            Comptes.Add(compte);
        CompteSelectionne = Comptes.FirstOrDefault(c => c.CpteEstDefaut) ?? Comptes.FirstOrDefault();
    }

    public ObservableCollection<Cotation> Cotations { get; } = new();
    public ObservableCollection<TransactionFinanciere> TransactionsRecentes { get; } = new();
    public ObservableCollection<Compte> Comptes { get; } = new();
    public IReadOnlyList<string> TypesTransaction { get; } = new[] { "Achat", "Vente" };

    public Cotation? CotationSelectionnee
    {
        get => _cotationSelectionnee;
        set
        {
            if (ReferenceEquals(_cotationSelectionnee, value)) return;
            _cotationSelectionnee = value;
            OnPropertyChanged();
            ChargerTransactions();
        }
    }

    public TransactionFinanciere? TransactionSelectionnee
    {
        get => _transactionSelectionnee;
        set
        {
            if (ReferenceEquals(_transactionSelectionnee, value)) return;
            _transactionSelectionnee = value;
            OnPropertyChanged();
            if (value != null)
            {
                TypeTransaction = value.TransType;
                QuantiteTexte = value.TransQte.ToString(CultureInfo.CurrentCulture);
                FraisTexte = value.TransFrais.ToString(CultureInfo.CurrentCulture);
                RecalculerTotal();
            }
        }
    }

    public Compte? CompteSelectionne { get => _compteSelectionne; set { _compteSelectionne = value; OnPropertyChanged(); } }
    public string TypeTransaction { get => _typeTransaction; set { _typeTransaction = value; OnPropertyChanged(); } }
    public string QuantiteTexte { get => _quantiteTexte; set { _quantiteTexte = value; OnPropertyChanged(); RecalculerTotal(); } }
    public string FraisTexte { get => _fraisTexte; set { _fraisTexte = value; OnPropertyChanged(); } }
    public string TotalTexte { get => _totalTexte; private set { _totalTexte = value; OnPropertyChanged(); } }

    public void RechercherCotation(Window owner)
    {
        var dialog = new TransEditWindow(new Cotation { Name = "Nouvelle Cotation" }, nouveau: true) { Owner = owner };
        if (dialog.ShowDialog() == true)
        {
            if (!Cotations.Contains(dialog.Cotation)) Cotations.Add(dialog.Cotation);
            CotationSelectionnee = dialog.Cotation;
        }
    }

    public void Enregistrer()
    {
        if (CotationSelectionnee == null || CompteSelectionne == null ||
            !decimal.TryParse(QuantiteTexte, NumberStyles.Number, CultureInfo.CurrentCulture, out var quantity) ||
            quantity <= 0 || !decimal.TryParse(FraisTexte, NumberStyles.Number, CultureInfo.CurrentCulture, out var fees) || fees < 0)
        {
            MessageBox.Show("Sélectionnez une cotation, un compte et saisissez une quantité et des frais valides.", "Transaction", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var oldQuantity = TransactionSelectionnee?.TransQte;
        var item = TransactionSelectionnee ?? new TransactionFinanciere();
        item.TransType = TypeTransaction;
        item.TransQte = quantity;
        item.TransPrix = Convert.ToDecimal(CotationSelectionnee.Close);
        item.TransFrais = fees;
        item.TransDateTransac = DateTime.Now;
        item.TransCpteId = CompteSelectionne.CpteId;
        _transactions.Save(item, CotationSelectionnee, oldQuantity);
        ChargerTransactions();
        TransactionSelectionnee = TransactionsRecentes.FirstOrDefault(t => t.TransId == item.TransId);
        QuantiteTexte = string.Empty;
        FraisTexte = "0";
    }

    public void Annuler()
    {
        TransactionSelectionnee = null;
        QuantiteTexte = string.Empty;
        FraisTexte = "0";
        TypeTransaction = "Achat";
        RecalculerTotal();
    }

    private void ChargerTransactions()
    {
        TransactionsRecentes.Clear();
        if (CotationSelectionnee?.Symbol == null) return;
        var actifId = _transactions.EnsureActif(CotationSelectionnee);
        foreach (var transaction in _transactions.GetRecent(actifId))
            TransactionsRecentes.Add(transaction);
    }

    private void RecalculerTotal()
    {
        if (CotationSelectionnee == null || !decimal.TryParse(QuantiteTexte, NumberStyles.Number, CultureInfo.CurrentCulture, out var quantity))
        {
            TotalTexte = "0,00";
            return;
        }
        TotalTexte = (quantity * Convert.ToDecimal(CotationSelectionnee.Close)).ToString("N2", CultureInfo.CurrentCulture);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
