using CommunityToolkit.Mvvm.ComponentModel;

namespace ThommyKalkulator.WPF.ViewModels.Pages;

public partial class AppearanceViewModel : ObservableObject
{
    [ObservableProperty]
    private string pageTitle = "Darstellung";

    [ObservableProperty]
    private string pageDescription = "Hier werden Theme- und Anzeigeoptionen der Anwendung verwaltet.";
}