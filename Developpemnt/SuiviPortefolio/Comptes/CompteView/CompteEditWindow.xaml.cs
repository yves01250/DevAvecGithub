using System.Globalization;
using System.Windows;
using SuiviPortefolio.Comptes.CompteModel;

namespace SuiviPortefolio.Comptes.CompteView;

public partial class CompteEditWindow : Window
{
    public Compte Compte { get; }

    public CompteEditWindow(Compte compte, bool nouveau)
    {
        InitializeComponent();

        Compte = compte;
        DataContext = new CompteEditWindowModel(nouveau);

        NomTextBox.Text = compte.CpteNom;
        TypeComboBox.SelectedItem = compte.CpteType;
        DeviseTextBox.Text = compte.CpteDevise;
        SoldeTextBox.Text = compte.CpteSolde.ToString(CultureInfo.CurrentCulture);
        DefautCheckBox.IsChecked = compte.CpteEstDefaut;
    }

    private void Enregistrer_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NomTextBox.Text))
        {
            MessageBox.Show("Le nom du compte est obligatoire.", "Compte",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            NomTextBox.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(DeviseTextBox.Text))
        {
            MessageBox.Show("La devise est obligatoire.", "Compte",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            DeviseTextBox.Focus();
            return;
        }

        if (!decimal.TryParse(SoldeTextBox.Text, NumberStyles.Number,
                CultureInfo.CurrentCulture, out var solde))
        {
            MessageBox.Show("Le solde doit être un nombre valide.", "Compte",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            SoldeTextBox.Focus();
            return;
        }

        Compte.CpteNom = NomTextBox.Text.Trim();
        Compte.CpteType = (TypeCompte)TypeComboBox.SelectedItem!;
        Compte.CpteDevise = DeviseTextBox.Text.Trim().ToUpperInvariant();
        Compte.CpteSolde = solde;
        Compte.CpteEstDefaut = DefautCheckBox.IsChecked == true;

        DialogResult = true;
    }

    private sealed class CompteEditWindowModel
    {
        public string Titre { get; }
        public Array TypesCompte { get; } = Enum.GetValues<TypeCompte>();

        public CompteEditWindowModel(bool nouveau)
        {
            Titre = nouveau ? "Nouveau compte" : "Modifier le compte";
        }
    }
}
