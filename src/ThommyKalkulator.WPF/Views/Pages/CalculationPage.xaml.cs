using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ThommyKalkulator.Application.Services;
using ThommyKalkulator.WPF.ViewModels.Pages;
using ThommyKalkulator.WPF.Views;

namespace ThommyKalkulator.WPF.Views.Pages;

public partial class CalculationPage : UserControl
{
    private readonly CalculationViewModel? _viewModel;
    private FloatingPreviewWindow? _floatingPreviewWindow;
    private bool _isCalculationTabSelected;

    public CalculationPage()
    {
        InitializeComponent();

        if (DesignerProperties.GetIsInDesignMode(this))
        {
            return;
        }

        _viewModel = new CalculationViewModel(App.AppState, new CalculationService());
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        DataContext = _viewModel;
        App.ProjectEditRequested += OnProjectEditRequested;
        App.AppState.DataChanged += OnAppStateDataChanged;
        Unloaded += OnUnloaded;
    }

    public void HandleTabSelectionChanged(bool isSelected)
    {
        _isCalculationTabSelected = isSelected;
        SyncFloatingPreview();
    }

    public void DisposeFloatingPreview()
    {
        if (_floatingPreviewWindow is null)
        {
            return;
        }

        _floatingPreviewWindow.Close();
        _floatingPreviewWindow = null;
    }

    private void OnProjectEditRequested(object? sender, ProjectEditRequestEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        if (e.ProjectIndex < 0 || e.ProjectIndex >= App.AppState.CurrentData.Projects.Count)
        {
            return;
        }

        var project = App.AppState.CurrentData.Projects[e.ProjectIndex];
        _viewModel.LoadProjectForEditing(project, e.ProjectIndex);
        SyncFloatingPreview();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CalculationViewModel.FloatingPreviewText))
        {
            SyncFloatingPreview();
        }
    }

    private void OnAppStateDataChanged(object? sender, EventArgs e)
    {
        SyncFloatingPreview();
    }

    private void SyncFloatingPreview()
    {
        if (_viewModel is null)
        {
            return;
        }

        if (!App.AppState.CurrentData.GlobalSettings.FloatingLiveDefault)
        {
            if (_floatingPreviewWindow is not null)
            {
                _floatingPreviewWindow.Close();
                _floatingPreviewWindow = null;
            }

            return;
        }

        var ownerWindow = Window.GetWindow(this);
        var previewWindow = EnsureFloatingPreviewWindow(ownerWindow);

        if (_isCalculationTabSelected)
        {
            previewWindow.UpdateText(_viewModel.FloatingPreviewText);
        }
        else
        {
            previewWindow.UpdateText(
                "Bitte Kalkulation öffnen …"
                + Environment.NewLine
                + Environment.NewLine
                + "Wechsle zum Tab „Kalkulation“,"
                + Environment.NewLine
                + "um die Live-Vorschau zu aktivieren.");
        }

        if (!previewWindow.IsVisible)
        {
            if (ownerWindow is not null)
            {
                previewWindow.Left = ownerWindow.Left + ownerWindow.Width + 16;
                previewWindow.Top = ownerWindow.Top + 40;
            }

            previewWindow.Show();
        }
    }

    private FloatingPreviewWindow EnsureFloatingPreviewWindow(Window? ownerWindow)
    {
        if (_floatingPreviewWindow is not null)
        {
            return _floatingPreviewWindow;
        }

        _floatingPreviewWindow = new FloatingPreviewWindow();
        _floatingPreviewWindow.Closed += FloatingPreviewWindow_OnClosed;

        if (ownerWindow is not null)
        {
            _floatingPreviewWindow.Owner = ownerWindow;
        }

        return _floatingPreviewWindow;
    }

    private void FloatingPreviewWindow_OnClosed(object? sender, EventArgs e)
    {
        if (_floatingPreviewWindow is not null)
        {
            _floatingPreviewWindow.Closed -= FloatingPreviewWindow_OnClosed;
        }

        _floatingPreviewWindow = null;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        App.ProjectEditRequested -= OnProjectEditRequested;
        App.AppState.DataChanged -= OnAppStateDataChanged;
        Unloaded -= OnUnloaded;

        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        DisposeFloatingPreview();
    }
}
