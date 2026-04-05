using System.ComponentModel;
using System.Windows.Controls;
using ThommyKalkulator.WPF.ViewModels.Pages;

namespace ThommyKalkulator.WPF.Views.Pages;

/// <summary>
/// Interaktionslogik für AppearancePage.xaml
/// </summary>
public partial class AppearancePage : UserControl
{
    public AppearancePage()
    {
        InitializeComponent();

        if (DesignerProperties.GetIsInDesignMode(this))
        {
            DataContext = new AppearanceViewModel();
            return;
        }

        DataContext = new AppearanceViewModel();
        App.AppConfigurationChanged += OnAppConfigurationChanged;
        Unloaded += OnUnloaded;
    }

    private void OnAppConfigurationChanged(object? sender, AppConfigurationChangedEventArgs e)
    {
        if (DataContext is AppearanceViewModel viewModel)
        {
            viewModel.LoadFromConfiguration(e.Configuration);
        }
    }

    private void OnUnloaded(object sender, System.Windows.RoutedEventArgs e)
    {
        App.AppConfigurationChanged -= OnAppConfigurationChanged;
        Unloaded -= OnUnloaded;
    }
}
