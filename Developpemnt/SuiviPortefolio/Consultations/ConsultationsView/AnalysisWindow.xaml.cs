// Views/AnalysisWindow.xaml.cs
using System.Windows;
using SuiviPortefolio.Consultations.ConsultationsModel;


namespace SuiviPortefolio.Consultations.ConsultationsView;
public partial class AnalysisWindow : Window
{
    public AnalysisWindow(System.Collections.Generic.IList<ConsultPosition> positions)

    {
        InitializeComponent();
        DataContext = new AnalysisViewModel(positions);
    }
}
