using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThommyKalkulator.WPF.Services;

namespace ThommyKalkulator.WPF.ViewModels.Pages;

public partial class AppearanceViewModel : ObservableObject
{
    [ObservableProperty]
    private string pageTitle = "Darstellung";

    [ObservableProperty]
    private string pageDescription = "Hier werden Theme- und Anzeigeoptionen der Anwendung verwaltet.";

    [ObservableProperty]
    private string statusMessage = "Bereit.";

    [ObservableProperty]
    private bool darkModeSelected;

    [ObservableProperty]
    private bool lightModeSelected;

    [ObservableProperty]
    private bool systemModeSelected;

    [ObservableProperty]
    private string windowWidthText = "1400";

    [ObservableProperty]
    private string windowHeightText = "900";

    public AppearanceViewModel()
    {
        var configuration = App.CurrentConfiguration ?? AppConfigurationStore.CreateDefault();
        LoadFromConfiguration(configuration);
    }

    public void LoadFromConfiguration(AppConfiguration configuration)
    {
        configuration = AppConfigurationStore.Normalize(configuration);

        var appearance = AppThemeModes.Normalize(configuration.Appearance);
        DarkModeSelected = string.Equals(appearance, AppThemeModes.Dark, StringComparison.OrdinalIgnoreCase);
        LightModeSelected = string.Equals(appearance, AppThemeModes.Light, StringComparison.OrdinalIgnoreCase);
        SystemModeSelected = string.Equals(appearance, AppThemeModes.System, StringComparison.OrdinalIgnoreCase);

        WindowWidthText = configuration.WindowWidth.ToString();
        WindowHeightText = configuration.WindowHeight.ToString();
    }

    [RelayCommand]
    private void ApplyAppearance()
    {
        var appearance = GetSelectedAppearance();
        var updatedConfiguration = new AppConfiguration
        {
            Appearance = appearance,
            WindowWidth = ParseDimension(WindowWidthText, 1400, 1100),
            WindowHeight = ParseDimension(WindowHeightText, 900, 700)
        };

        App.UpdateConfiguration(updatedConfiguration);
        StatusMessage = "Darstellung wurde gespeichert und angewendet.";
    }

    [RelayCommand]
    private void ApplyWindowSize()
    {
        if (!int.TryParse(WindowWidthText, out var width) || !int.TryParse(WindowHeightText, out var height))
        {
            StatusMessage = "Bitte gültige Zahlen für Breite und Höhe eingeben.";
            return;
        }

        width = Math.Max(1100, width);
        height = Math.Max(700, height);

        WindowWidthText = width.ToString();
        WindowHeightText = height.ToString();

        var updatedConfiguration = new AppConfiguration
        {
            Appearance = GetSelectedAppearance(),
            WindowWidth = width,
            WindowHeight = height
        };

        App.UpdateConfiguration(updatedConfiguration);
        StatusMessage = "Fenstergröße wurde gespeichert und angewendet.";
    }

    private string GetSelectedAppearance()
    {
        if (LightModeSelected)
        {
            return AppThemeModes.Light;
        }

        if (SystemModeSelected)
        {
            return AppThemeModes.System;
        }

        return AppThemeModes.Dark;
    }

    private static int ParseDimension(string? text, int fallback, int minimum)
    {
        if (!int.TryParse(text, out var parsed))
        {
            return fallback;
        }

        return Math.Max(minimum, parsed);
    }
}
