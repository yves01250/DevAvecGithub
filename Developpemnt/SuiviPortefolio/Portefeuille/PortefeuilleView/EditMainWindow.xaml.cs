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
        Portefeuille.PtfNom = NomPtfTextBox.Text.Trim();
        Portefeuille.PtfType = TypeComboBox.SelectedItem?.ToString() ?? "Général";
        Portefeuille.PtfDevise = DevisePtfTextBox.Text.Trim().ToUpperInvariant();
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
