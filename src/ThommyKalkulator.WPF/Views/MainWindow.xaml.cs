using System.Windows;
using System.Windows.Controls;
using ThommyKalkulator.WPF.Services;
using ThommyKalkulator.WPF.Views.Pages;

namespace ThommyKalkulator.WPF.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Closed += OnClosed;
        MainTabControl.SelectionChanged += MainTabControl_OnSelectionChanged;
        App.TabSelectionRequested += OnTabSelectionRequested;
        App.AppConfigurationChanged += OnAppConfigurationChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        AppearanceManager.ApplyWindowSize(this, App.CurrentConfiguration);
        UpdateFloatingPreviewForCurrentTab();
    }

    private void MainTabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!ReferenceEquals(e.Source, MainTabControl))
        {
            return;
        }

        UpdateFloatingPreviewForCurrentTab();
    }

    private void OnTabSelectionRequested(object? sender, TabSelectionRequestEventArgs e)
    {
        MainTabControl.SelectedIndex = e.TabIndex;
        UpdateFloatingPreviewForCurrentTab();
    }

    private void OnAppConfigurationChanged(object? sender, AppConfigurationChangedEventArgs e)
    {
        AppearanceManager.ApplyWindowSize(this, e.Configuration);
        UpdateFloatingPreviewForCurrentTab();
    }

    private void UpdateFloatingPreviewForCurrentTab()
    {
        var calculationPage = FindCalculationPage();
        if (calculationPage is null)
        {
            return;
        }

        var selectedContent = (MainTabControl.SelectedItem as TabItem)?.Content;
        var isCalculationTabSelected = ReferenceEquals(selectedContent, calculationPage);

        calculationPage.HandleTabSelectionChanged(isCalculationTabSelected);
    }

    private CalculationPage? FindCalculationPage()
    {
        foreach (var item in MainTabControl.Items.OfType<TabItem>())
        {
            if (item.Content is CalculationPage calculationPage)
            {
                return calculationPage;
            }
        }

        return null;
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        App.TabSelectionRequested -= OnTabSelectionRequested;
        App.AppConfigurationChanged -= OnAppConfigurationChanged;
        MainTabControl.SelectionChanged -= MainTabControl_OnSelectionChanged;
        Loaded -= OnLoaded;
        Closed -= OnClosed;

        FindCalculationPage()?.DisposeFloatingPreview();
    }
}
