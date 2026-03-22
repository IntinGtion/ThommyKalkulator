using System.ComponentModel;
using System.Windows.Controls;
using ThommyKalkulator.WPF.ViewModels.Pages;

namespace ThommyKalkulator.WPF.Views.Pages;

/// <summary>
/// Interaktionslogik für SettingsPage.xaml
/// </summary>
public partial class SettingsPage : UserControl
{
    public SettingsPage()
    {
        InitializeComponent();

        if (DesignerProperties.GetIsInDesignMode(this))
        {
            DataContext = new SettingsViewModel();
            return;
        }

        DataContext = new SettingsViewModel(App.AppState);
    }
}
