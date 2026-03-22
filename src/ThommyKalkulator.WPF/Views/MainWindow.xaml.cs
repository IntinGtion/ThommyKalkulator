using System.Windows;

namespace ThommyKalkulator.WPF.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        App.TabSelectionRequested += OnTabSelectionRequested;
        Closed += OnClosed;
    }

    private void OnTabSelectionRequested(object? sender, TabSelectionRequestEventArgs e)
    {
        MainTabControl.SelectedIndex = e.TabIndex;
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        App.TabSelectionRequested -= OnTabSelectionRequested;
        Closed -= OnClosed;
    }
}
