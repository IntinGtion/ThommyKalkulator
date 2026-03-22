using System.ComponentModel;
using System.Windows.Controls;
using ThommyKalkulator.Application.Services;
using ThommyKalkulator.WPF.ViewModels.Pages;

namespace ThommyKalkulator.WPF.Views.Pages;

public partial class CalculationPage : UserControl
{
    public CalculationPage()
    {
        InitializeComponent();

        if (DesignerProperties.GetIsInDesignMode(this))
        {
            return;
        }

        DataContext = new CalculationViewModel(App.AppState, new CalculationService());
        App.ProjectEditRequested += OnProjectEditRequested;
        Unloaded += OnUnloaded;
    }

    private void OnProjectEditRequested(object? sender, ProjectEditRequestEventArgs e)
    {
        if (DataContext is not CalculationViewModel viewModel)
        {
            return;
        }

        if (e.ProjectIndex < 0 || e.ProjectIndex >= App.AppState.CurrentData.Projects.Count)
        {
            return;
        }

        var project = App.AppState.CurrentData.Projects[e.ProjectIndex];
        viewModel.LoadProjectForEditing(project, e.ProjectIndex);
    }

    private void OnUnloaded(object sender, System.Windows.RoutedEventArgs e)
    {
        App.ProjectEditRequested -= OnProjectEditRequested;
        Unloaded -= OnUnloaded;
    }
}
