using System.Globalization;
using System.Windows;
using SuiviPortefolio.Portefeuille.PortefeuilleModel;

namespace SuiviPortefolio.Portefeuille.PortefeuilleView;

public partial class EditMainWindow : Window
{
    public monPortefeuille Portefeuille { get; }

    public EditMainWindow(monPortefeuille portefeuille, bool nouveau)
    {
        InitializeComponent();
        Portefeuille = portefeuille;
        DataContext = new EditMainWindowModel(nouveau);
        NomPtfTextBox.Text = portefeuille.PtfNom;
        TypeComboBox.SelectedItem = portefeuille.PtfType;
        DevisePtfTextBox.Text = portefeuille.PtfDevise;
        SoldePtfTextBox.Text = portefeuille.PtfSolde.ToString("0.00", CultureInfo.CurrentCulture);
        DefautCheckBox.IsChecked = portefeuille.PtfEstDefaut;
    }

    private void Enregistrer_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NomPtfTextBox.Text))
        {
            MessageBox.Show("Le nom du portefeuille est obligatoire.", "Portefeuille",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            NomPtfTextBox.Focus();
            return;
        }
        if (string.IsNullOrWhiteSpace(DevisePtfTextBox.Text))
        {
            MessageBox.Show("La devise est obligatoire.", "Portefeuille",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            DevisePtfTextBox.Focus();
            return;
        }
        if (!decimal.TryParse(SoldePtfTextBox.Text, NumberStyles.Number,
                CultureInfo.CurrentCulture, out var solde))
        {
            MessageBox.Show("Le solde doit être un nombre valide.", "Portefeuille",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            SoldePtfTextBox.Focus();
            return;
        }

        Portefeuille.PtfNom = NomPtfTextBox.Text.Trim();
        Portefeuille.PtfType = TypeComboBox.SelectedItem?.ToString() ?? "Général";
        Portefeuille.PtfDevise = DevisePtfTextBox.Text.Trim().ToUpperInvariant();
        Portefeuille.PtfSolde = solde;
        Portefeuille.PtfEstDefaut = DefautCheckBox.IsChecked == true;
        DialogResult = true;
    }

    private sealed class EditMainWindowModel
    {
        public string Titre { get; }
        public string[] TypesPortefeuille { get; } = ["Général", "Investissement", "Épargne", "Autre"];

        public EditMainWindowModel(bool nouveau) =>
            Titre = nouveau ? "Nouveau portefeuille" : "Modifier le portefeuille";
    }
}
