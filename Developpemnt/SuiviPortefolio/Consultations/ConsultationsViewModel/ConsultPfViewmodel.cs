
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using System.Threading.Tasks;
using System;
using SuiviPortefolio.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using SuiviPortefolio.Consultations.ConsultationsView;

namespace SuiviPortefolio.Consultations.ConsultationsViewModel;
public class PortfolioViewModel : INotifyPropertyChanged
{
    private readonly ConsultPositionRepository _repository;
    private readonly ObservableCollection<PositionViewModel> _allPositions;
    private readonly RelayCommand _analyzeCommand;
    private readonly RelayCommand _refreshPricesCommand;

    private bool _isLoading;

    public PortfolioViewModel(ConsultPositionRepository repository)
    {
        _repository = repository;
        _allPositions = new ObservableCollection<PositionViewModel>();

        _analyzeCommand = new RelayCommand(async _ => await AnalyzeSelectedAsync(), _ => CanAnalyze);
        _refreshPricesCommand = new RelayCommand(async _ => await RefreshPricesAsync(), _ => !IsLoading);

        LoadCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsLoading);
    }

    public ICommand LoadCommand { get; }
    public ICommand AnalyzeCommand => _analyzeCommand;
    public ICommand RefreshPricesCommand => _refreshPricesCommand;

    public bool IsLoading
    {
        get => _isLoading;
        private set { _isLoading = value; OnPropertyChanged(); }
    }

    // Collection filtrée : uniquement les positions avec Quantity > 0
    public ObservableCollection<PositionViewModel> ActivePositions =>
        new ObservableCollection<PositionViewModel>(
            _allPositions.Where(p => p.Quantity > 0)
        );

    private bool CanAnalyze => _allPositions.Any(p => p.IsSelected && p.Quantity > 0);

    public async Task LoadAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        OnPropertyChanged(nameof(ActivePositions)); // pour notifier si nécessaire

        try
        {
            var positions = await _repository.GetAllAsync();
            _allPositions.Clear();
            foreach (var p in positions)
            {
                var position = new PositionViewModel(p);
                position.PropertyChanged += Position_PropertyChanged;
                _allPositions.Add(position);
            }

            OnPropertyChanged(nameof(ActivePositions));
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void Position_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PositionViewModel.IsSelected))
            _analyzeCommand.RaiseCanExecuteChanged();
    }

    // Simulation de mise à jour des cours (à remplacer par un appel API réel)
    public async Task RefreshPricesAsync()
    {
        if (IsLoading) return;
        IsLoading = true;

        try
        {
            var now = DateTime.UtcNow;
            var random = new Random();

            // Mise à jour en mémoire + persistance SQLite
            foreach (var vm in _allPositions.Where(p => p.Quantity > 0))
            {
                // Exemple : variation aléatoire entre -2% et +2%
                var variation = (decimal)(random.NextDouble() * 0.04 - 0.02);
                var newPrice = Math.Max(0.01m, vm.Price * (1 + variation));

                vm.UpdatePrice(newPrice, now);

                // Persistance (optionnel, peut être batché)
                await _repository.UpdatePriceAsync(vm.Id, newPrice, now);
            }

            // Notifier que la liste a changé (si besoin pour l’UI)
            OnPropertyChanged(nameof(ActivePositions));
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task AnalyzeSelectedAsync()
    {
        var selected = _allPositions
            .Where(p => p.IsSelected && p.Quantity > 0)
            .Select(p => p.Model)
            .ToList();

        if (selected.Count == 0) return;

        var window = new AnalysisWindow(selected)
        {
            Owner = Application.Current.MainWindow
        };
        window.ShowDialog();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}