using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using SuiviPortefolio.Consultations.ConsultationsModel;
using SuiviPortefolio.Consultations.ConsultationsView;
using SuiviPortefolio.Data;

namespace SuiviPortefolio.Consultations.ConsultationsViewModel;

[ObservableObject]
public partial class PortfolioViewModel
{
    private readonly ConsultPositionRepository _repository;
    private readonly CotationFetcher _cotationFetcher = new();
    private readonly ObservableCollection<PositionViewModel> _allPositions = new();
    private bool _isUpdatingSelection;

    // ✅ Propriétés avec [ObservableProperty]
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isChartLoading;

    [ObservableProperty]
    private string _summaryText = "Aucune donnée disponible pour les Tickers sélectionnés.";

    [ObservableProperty]
    private string _selectedPeriod = "1y";

    // ✅ ChartModel reste inchangé (pas besoin de notification)
    public PlotModel ChartModel { get; } = new()
    {
        Title = "Évolution des positions sélectionnées",
        LegendPosition = LegendPosition.RightTop,
        LegendPlacement = LegendPlacement.Outside,
        LegendMaxWidth = double.PositiveInfinity,
        LegendFontSize = 10,
        LegendSymbolLength = 10
    };

    public IReadOnlyDictionary<string, string> Periods { get; } = new Dictionary<string, string>
    {
        ["1 mois"] = "1mo",
        ["3 mois"] = "3mo",
        ["6 mois"] = "6mo",
        ["1 an"] = "1y",
        ["2 ans"] = "2y",
        ["5 ans"] = "5y"
    };

    // ✅ ActivePositions devient une collection stockée (pas recréée à chaque accès)
    public ObservableCollection<PositionViewModel> ActivePositions { get; } = new();

    // ✅ Propriété calculée (pas de changement)
    public bool? IsAllSelected
    {
        get
        {
            var activePositions = ActivePositions.ToList();
            if (activePositions.Count == 0 || activePositions.All(position => !position.IsSelected))
                return false;

            return activePositions.All(position => position.IsSelected) ? true : null;
        }
    }

    // ✅ Méthodes pour CanExecute
    private bool CanAnalyze => _allPositions.Any(p => p.IsSelected && p.Quantity > 0);
    private bool CanRefreshPrices => !IsLoading;

    // ✅ Constructeur simplifié
    public PortfolioViewModel(ConsultPositionRepository repository)
    {
        _repository = repository;
        UpdateActivePositions();
    }

    // ✅ Gestion du changement de SelectedPeriod
    partial void OnSelectedPeriodChanged(string value)
    {
        _ = RefreshChartAsync();
    }

    // ✅ Méthode pour mettre à jour ActivePositions
    private void UpdateActivePositions()
    {
        ActivePositions.Clear();
        foreach (var p in _allPositions.Where(p => p.Quantity > 0))
        {
            ActivePositions.Add(p);
        }
    }

    // ✅ Commandes avec [RelayCommand]
    [RelayCommand(CanExecute = nameof(CanRefreshPrices))]
    private async Task RefreshPricesAsync()
    {
        if (IsLoading) return;
        IsLoading = true;

        try
        {
            foreach (var vm in _allPositions.Where(p => p.Quantity > 0))
            {
                var snapshot = await _cotationFetcher.FetchSnapshotAsync(vm.Ticker);
                if (snapshot is null || snapshot.Close <= 0)
                    continue;

                var price = (decimal)snapshot.Close;
                vm.UpdatePrice(price, snapshot.Date);
                await _repository.UpdatePriceAsync(vm.Id, price, snapshot.Date);
            }

            UpdateActivePositions();
            await RefreshChartAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanAnalyze))]
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

    [RelayCommand]
    private async Task ToggleAllSelectionAsync()
    {
        var selectAll = IsAllSelected != true;
        _isUpdatingSelection = true;
        try
        {
            foreach (var position in _allPositions.Where(position => position.Quantity > 0))
                position.IsSelected = selectAll;
        }
        finally
        {
            _isUpdatingSelection = false;
        }

        OnPropertyChanged(nameof(IsAllSelected));
        AnalyzeSelectedCommand.NotifyCanExecuteChanged();
        await RefreshChartAsync();
    }

    [RelayCommand(CanExecute = nameof(CanRefreshPrices))] // ✅ Même condition que RefreshPrices
    public async Task LoadAsync()
    {
        if (IsLoading) return;
        IsLoading = true;

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

            UpdateActivePositions();
        }
        finally
        {
            IsLoading = false;
        }

        await RefreshPricesAsync();
    }

    private async Task RefreshChartAsync()
    {
        if (IsChartLoading) return;

        var selected = _allPositions
            .Where(p => p.IsSelected && p.Quantity > 0)
            .ToList();

        IsChartLoading = true;
        try
        {
            var histories = await Task.WhenAll(selected.Select(async position =>
                (position, history: await _cotationFetcher.FetchHistoryAsync(
                    position.Ticker, SelectedPeriod))));

            // ✅ Calcul des résumés
            var summaries = histories
                .Where(item => item.history.Count > 0)
                .GroupBy(item => item.position.Ticker)
                .Select(group =>
                {
                    var points = group.First().history;
                    var first = points.OrderBy(point => point.Date).First();
                    var latest = points.OrderByDescending(point => point.Date).First();
                    var performancePct = first.Close == 0
                        ? 0
                        : (latest.Close / first.Close - 1) * 100;

                    return new
                    {
                        Ticker = group.Key,
                        Performance = performancePct,
                        LastPrice = latest.Close,
                        Points = points.Count
                    };
                })
                .OrderByDescending(summary => summary.Performance)
                .Select(summary =>
                    $"{summary.Ticker}  " +
                    $"Performance : {summary.Performance:+0.00;-0.00;0.00}%  " +
                    $"Dernier cours : {summary.LastPrice:N2}  " +
                    $"Points : {summary.Points}")
                .ToList();

            SummaryText = summaries.Count == 0
                ? "Aucune donnée disponible pour les Tickers sélectionnés."
                : string.Join("\n\n", summaries);

            // ✅ Configuration du graphique
            ChartModel.Series.Clear();
            ChartModel.Axes.Clear();

            var majorStep = SelectedPeriod switch
            {
                "1mo"  => 7,
                "3mo"  => 15,
                "6mo"  => 30,
                "1y"   => 30,
                "2y"   => 60,
                "5y"   => 180,
                _      => 30
            };

            ChartModel.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Date",
                StringFormat = "MM/yyyy",
                MajorStep = majorStep,
                IntervalLength = majorStep
            });
            ChartModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Valeur de la position"
            });

            foreach (var (position, history) in histories)
            {
                if (!history.Any()) continue;

                var series = new LineSeries
                {
                    Title = position.Ticker + "--",
                    StrokeThickness = 2,
                    TrackerFormatString = "{0}\nDate : {2:dd/MM/yyyy}\nValeur : {4:0.##}"
                };

                var firstClose = history.First().Close;
                foreach (var point in history)
                {
                    var normalizedValue = (point.Close / firstClose) * 100;
                    series.Points.Add(new DataPoint(
                        DateTimeAxis.ToDouble(point.Date),
                        normalizedValue));
                }

                if (series.Points.Count > 0)
                    ChartModel.Series.Add(series);
            }

            ChartModel.InvalidatePlot(true);
        }
        finally
        {
            IsChartLoading = false;
        }
    }

    private async void Position_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PositionViewModel.IsSelected))
        {
            if (_isUpdatingSelection)
                return;

            OnPropertyChanged(nameof(IsAllSelected));
            AnalyzeSelectedCommand.NotifyCanExecuteChanged();
            await RefreshChartAsync();
        }
    }
}