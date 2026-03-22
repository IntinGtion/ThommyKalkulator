using CommunityToolkit.Mvvm.ComponentModel;

namespace ThommyKalkulator.WPF.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string appTitle = "Thommy Kalkulator";

    [ObservableProperty]
    private string statusText = "Bereit";

    [ObservableProperty]
    private int selectedTabIndex;
}