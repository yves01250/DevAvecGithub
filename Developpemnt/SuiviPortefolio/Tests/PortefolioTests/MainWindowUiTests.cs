using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using SuiviPortefolio.Portefeuille.PortefeuilleModel;
using SuiviPortefolio.Portefeuille.PortefeuilleView;
using Xunit;

namespace SuiviPortefolio.Tests.PortefolioTests;

public sealed class MainWindowUiTests
{
    [Fact]
    public void SelectionPortefeuille_MetAJourLeViewModelParLiaisonXaml()
    {
        RunOnStaThread(() =>
        {
            _ = Application.Current ?? new Application();
            var window = new MainWindow();
            var viewModel = new TestMainViewModel();
            window.DataContext = viewModel;
            window.Show();
            window.UpdateLayout();

            var selection = FindDescendant<ComboBox>(window, comboBox =>
                comboBox.GetBindingExpression(ItemsControl.ItemsSourceProperty)?
                    .ParentBinding.Path.Path == "ListePortefeuilles");

            Assert.NotNull(selection);
            Assert.Equal("PortefeuilleSelectionne",
                selection!.GetBindingExpression(Selector.SelectedItemProperty)?
                    .ParentBinding.Path.Path);

            selection.SelectedIndex = 1;
            selection.GetBindingExpression(Selector.SelectedItemProperty)!.UpdateSource();

            Assert.Same(viewModel.Portefeuilles[1], viewModel.PortefeuilleSelectionne);
            Assert.Same(viewModel.Portefeuilles[1], viewModel.Portefeuille);

            window.Close();
        });
    }

    private static T? FindDescendant<T>(
        DependencyObject parent,
        Func<T, bool> predicate)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            if (child is T candidate && predicate(candidate))
                return candidate;

            var descendant = FindDescendant(child, predicate);
            if (descendant != null)
                return descendant;
        }

        return null;
    }

    private static void RunOnStaThread(Action action)
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (exception != null)
            throw new Xunit.Sdk.XunitException("Le test WPF a échoué.", exception);
    }

    private sealed class TestMainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<monPortefeuille> Portefeuilles { get; } =
            new()
            {
                new monPortefeuille { PtfNom = "Premier", PtfSolde = 100m },
                new monPortefeuille { PtfNom = "Second", PtfSolde = 200m }
            };

        public ObservableCollection<monPortefeuille> ListePortefeuilles => Portefeuilles;

        private monPortefeuille? _portefeuilleSelectionne;

        public monPortefeuille? PortefeuilleSelectionne
        {
            get => _portefeuilleSelectionne;
            set
            {
                if (_portefeuilleSelectionne == value)
                    return;

                _portefeuilleSelectionne = value;
                Portefeuille = value;
                OnPropertyChanged();
            }
        }

        public monPortefeuille? Portefeuille { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
