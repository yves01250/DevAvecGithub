
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
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;

namespace SuiviPortefolio.Consultations.ConsultationsViewModel;
public class PortfolioViewModel : INotifyPropertyChanged
{
    private readonly ConsultPositionRepository _repository;
    private readonly CotationFetcher _cotationFetcher = new();
    private readonly ObservableCollection<PositionViewModel> _allPositions;
    private readonly RelayCommand _analyzeCommand;
    private readonly RelayCommand _refreshPricesCommand;
    private readonly RelayCommand _toggleAllSelectionCommand;

    private bool _isLoading;
    private bool _isUpdatingSelection;

    private string _selectedPeriod = "1y";
    private bool _isChartLoading;
    private string _summaryText = "Aucune donnée disponible pour les Tickers sélectionnés.";

    public PlotModel ChartModel { get; }
    public string SummaryText
    {
        get => _summaryText;
        private set
        {
            if (_summaryText == value) return;
            _summaryText = value;
            OnPropertyChanged();
        }
    }

    public IReadOnlyDictionary<string, string> Periods { get; } =
        new Dictionary<string, string>
        {
            ["1 mois"] = "1mo",
            ["3 mois"] = "3mo",
            ["6 mois"] = "6mo",
            ["1 an"] = "1y",
            ["2 ans"] = "2y",
            ["5 ans"] = "5y"
        };

    public string SelectedPeriod
    {
        get => _selectedPeriod;
        set
        {
            if (_selectedPeriod == value) return;
            _selectedPeriod = value;
            OnPropertyChanged();
            _ = RefreshChartAsync();
        }
    }

    public bool IsChartLoading
    {
        get => _isChartLoading;
        private set { _isChartLoading = value; OnPropertyChanged(); }
    }

    public PortfolioViewModel(ConsultPositionRepository repository)
    {
        _repository = repository;
        _allPositions = new ObservableCollection<PositionViewModel>();

        ChartModel = new PlotModel
        {
            Title = "Évolution des positions sélectionnées",
            LegendPosition = LegendPosition.RightTop,
            LegendPlacement = LegendPlacement.Outside,
            // LegendBackground = OxyColor.FromAColor(220, OxyColors.White),
            //LegendBorder = OxyColors.DimGray,
            // LegendBorderThickness = 1,
            LegendMaxWidth = double.PositiveInfinity,
            LegendFontSize = 10,
            LegendSymbolLength = 10
        };

    _analyzeCommand = new RelayCommand(async _ => await AnalyzeSelectedAsync(), _ => CanAnalyze);
    _refreshPricesCommand = new RelayCommand(async _ => await RefreshPricesAsync(), _ => !IsLoading);
    _toggleAllSelectionCommand = new RelayCommand(async _ => await ToggleAllSelectionAsync());

    LoadCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsLoading);
    }

    public ICommand LoadCommand { get; }
    public ICommand AnalyzeCommand => _analyzeCommand;
    public ICommand RefreshPricesCommand => _refreshPricesCommand;
    public ICommand ToggleAllSelectionCommand => _toggleAllSelectionCommand;

    public bool? IsAllSelected
    {
        get
        {
            var activePositions = _allPositions.Where(position => position.Quantity > 0).ToList();
            if (activePositions.Count == 0 || activePositions.All(position => !position.IsSelected))
                return false;

            return activePositions.All(position => position.IsSelected) ? true : null;
        }
    }

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

        await RefreshPricesAsync();
    }

    private async void Position_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PositionViewModel.IsSelected))
        {
            if (_isUpdatingSelection)
                return;

            OnPropertyChanged(nameof(IsAllSelected));
            _analyzeCommand.RaiseCanExecuteChanged();
            await RefreshChartAsync();
        }
    }

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
        _analyzeCommand.RaiseCanExecuteChanged();
        await RefreshChartAsync();
    }

    public async Task RefreshPricesAsync()
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

            OnPropertyChanged(nameof(ActivePositions));
            await RefreshChartAsync();
        }
        finally
        {
            IsLoading = false;
        }
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
                .OrderByDescending(summary => summary.Performance) // Tri sur la performance            
                .Select(summary =>
                    $"{summary.Ticker}  " +
                    $"Performance : {summary.Performance:+0.00;-0.00;0.00}%  " +
                    $"Dernier cours : {summary.LastPrice:N2}  " +
                    $"Points : {summary.Points}")
                .ToList();

            SummaryText = summaries.Count == 0
                ? "Aucune donnée disponible pour les Tickers sélectionnés."
                : string.Join("\n\n", summaries);

            ChartModel.Series.Clear();
            ChartModel.Axes.Clear();
            
            // Ajustement dynamique du pas des dates en fonction de la période sélectionnée
            var majorStep = SelectedPeriod switch
            {
                "1mo"  => 7,    // 1 semaine
                "3mo"  => 15,   // 15 jours
                "6mo"  => 30,   // 1 mois
                "1y"   => 30,   // 1 mois
                "2y"   => 60,   // 2 mois
                "5y"   => 180,  // 6 mois
                _      => 30     // Défaut
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
    if (!history.Any()) continue;  // Ignore les séries vides

    var series = new LineSeries
    {
        Title = position.Ticker + "--", // le "--" permet d'avoir le texte complet de la légende visible               
        StrokeThickness = 2,
        TrackerFormatString = "{0}\nDate : {2:dd/MM/yyyy}\nValeur : {4:0.##}"
    };

    var firstClose = history.First().Close;  // Valeur de référence (100%)
    foreach (var point in history)
    {
        // Normalisation : (Close / Close_initial) * 100
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