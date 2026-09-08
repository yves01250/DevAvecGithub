using System.Globalization;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SuiviPortefolio.Data;
using SuiviPortefolio.Transactions.TransModel;

namespace SuiviPortefolio.Transactions.TransView;

public partial class TransEditWindow : Window
{
    private readonly CotationFetcher _fetcher = new();

    public Cotation Cotation { get; }

    public TransEditWindow(Cotation cotation, bool nouveau)
    {
        InitializeComponent();
        Cotation = cotation;
        NomTextBox.Text = cotation.Name;
        AfficherCotation(cotation);
    }

    private async void Rechercher_Click(object sender, RoutedEventArgs e)
    {
        await RechercherAsync();
    }

    private async void NomTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        e.Handled = true;
        await RechercherAsync();
    }

    private async Task RechercherAsync()
    {
        if (string.IsNullOrWhiteSpace(NomTextBox.Text))
        {
            MessageBox.Show("Saisissez un nom à rechercher.", "Cotation",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            NomTextBox.Focus();
            return;
        }

        try
        {
            ResultatsListBox.ItemsSource = await _fetcher.SearchAsync(NomTextBox.Text);
            if (ResultatsListBox.Items.Count == 0)
                MessageBox.Show("Aucune cotation trouvée.", "Cotation",
                    MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"La recherche internet a échoué : {ex.Message}", "Cotation",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Cotation",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void ResultatsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ResultatsListBox.SelectedItem is not Cotation resultat)
            return;

        try
        {
            var snapshot = await _fetcher.FetchSnapshotAsync(resultat.Symbol!);
            AfficherCotation(snapshot ?? resultat);
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"La récupération du cours a échoué : {ex.Message}", "Cotation",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Cotation",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AfficherCotation(Cotation cotation)
    {
        Cotation.Name = cotation.Name;
        Cotation.Symbol = cotation.Symbol;
        Cotation.Instrument = cotation.Instrument;
        Cotation.Marche = cotation.Marche;
        Cotation.Devise = cotation.Devise;
        Cotation.Close = cotation.Close;
        Cotation.Date = cotation.Date;
        Cotation.CreatedAt = cotation.CreatedAt;

        SymbolTextBox.Text = cotation.Symbol;
        InstrumentTextBox.Text = cotation.Instrument;
        MarcheTextBox.Text = cotation.Marche;
        DeviseTextBox.Text = cotation.Devise;
        CloseTextBox.Text = cotation.Close.ToString("N2", CultureInfo.CurrentCulture);
        DateTextBox.Text = cotation.Date == DateTime.MinValue
            ? string.Empty
            : cotation.Date.ToString("g", CultureInfo.CurrentCulture);
    }

    private void Utiliser_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Cotation.Symbol))
        {
            MessageBox.Show("Sélectionnez une cotation dans les résultats.", "Cotation",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
    }
}
