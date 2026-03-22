using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using ThommyKalkulator.Application.Services;
using ThommyKalkulator.Infrastructure.Export;
using ThommyKalkulator.WPF.ViewModels.Pages;

namespace ThommyKalkulator.WPF.Views.Pages;

public partial class ProjectsPage : UserControl
{
    private readonly CsvExportService _csvExportService = new();
    private readonly PdfExportService _pdfExportService = new();

    public ProjectsPage()
    {
        InitializeComponent();

        if (DesignerProperties.GetIsInDesignMode(this))
        {
            return;
        }

        DataContext = new ProjectsViewModel(App.AppState, new CalculationService());
    }

    private void ProjectListView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is ProjectsViewModel viewModel)
        {
            viewModel.LoadSelectedProject();
        }
    }

    private void ExportSingleCsvButton_OnClick(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is not ProjectsViewModel viewModel)
        {
            return;
        }

        var selectedItem = ProjectListView.SelectedItem as ProjectListItemViewModel;
        if (selectedItem is null)
        {
            viewModel.StatusMessage = "Bitte zuerst eine Kalkulation auswählen.";
            return;
        }

        var defaultFileName = BuildSingleCsvFileName(selectedItem.Project);
        var dialog = new SaveFileDialog
        {
            Title = "CSV exportieren",
            Filter = "CSV (*.csv)|*.csv",
            FileName = defaultFileName,
            AddExtension = true,
            DefaultExt = ".csv"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            _csvExportService.ExportProjects(dialog.FileName, new[] { selectedItem.Project });
            viewModel.StatusMessage = "CSV-Export gespeichert: " + dialog.FileName;
        }
        catch (Exception ex)
        {
            viewModel.StatusMessage = "CSV-Export fehlgeschlagen: " + ex.Message;
        }
    }

    private void ExportAllCsvButton_OnClick(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is not ProjectsViewModel viewModel)
        {
            return;
        }

        var projects = App.AppState.CurrentData.Projects.ToList();
        if (projects.Count == 0)
        {
            viewModel.StatusMessage = "Keine Kalkulationen für den CSV-Export vorhanden.";
            return;
        }

        var dialog = new SaveFileDialog
        {
            Title = "CSV exportieren",
            Filter = "CSV (*.csv)|*.csv",
            FileName = "Kalkulation_alle.csv",
            AddExtension = true,
            DefaultExt = ".csv"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            _csvExportService.ExportProjects(dialog.FileName, projects);
            viewModel.StatusMessage = "CSV-Export gespeichert: " + dialog.FileName;
        }
        catch (Exception ex)
        {
            viewModel.StatusMessage = "CSV-Export fehlgeschlagen: " + ex.Message;
        }
    }

    private void ExportPdfButton_OnClick(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is not ProjectsViewModel viewModel)
        {
            return;
        }

        var selectedProjects = ProjectListView.SelectedItems
            .OfType<ProjectListItemViewModel>()
            .Select(item => item.Project)
            .Distinct()
            .ToList();

        if (selectedProjects.Count == 0)
        {
            var selectedItem = ProjectListView.SelectedItem as ProjectListItemViewModel;
            if (selectedItem is not null)
            {
                selectedProjects.Add(selectedItem.Project);
            }
        }

        if (selectedProjects.Count == 0)
        {
            viewModel.StatusMessage = "Bitte mindestens eine Kalkulation für den PDF-Export auswählen.";
            return;
        }

        var dialog = new SaveFileDialog
        {
            Title = "PDF exportieren",
            Filter = "PDF (*.pdf)|*.pdf",
            FileName = BuildPdfFileName(selectedProjects),
            AddExtension = true,
            DefaultExt = ".pdf"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            _pdfExportService.ExportProjects(dialog.FileName, selectedProjects, App.AppState.CurrentData.GlobalSettings);
            viewModel.StatusMessage = "PDF-Export gespeichert: " + dialog.FileName;
        }
        catch (Exception ex)
        {
            viewModel.StatusMessage = "PDF-Export fehlgeschlagen: " + ex.Message;
        }
    }

    private static string BuildSingleCsvFileName(ThommyKalkulator.Domain.Models.CalculationProject project)
    {
        var suffix = string.IsNullOrWhiteSpace(project.Name)
            ? "einzeln"
            : SanitizeFileNamePart(project.Name.Replace(' ', '_'));

        return "Kalkulation_" + suffix + ".csv";
    }

    private static string BuildPdfFileName(IReadOnlyList<ThommyKalkulator.Domain.Models.CalculationProject> projects)
    {
        if (projects.Count == 1)
        {
            var project = projects[0];
            var projectNumber = SanitizeFileNamePart((project.ProjectNumber ?? string.Empty).Replace(' ', '_'));
            var projectName = SanitizeFileNamePart((project.Name ?? string.Empty).Replace(' ', '_'));
            return $"Kalkulation_{projectNumber}_{projectName}.pdf".Replace("__", "_");
        }

        var distinctNumbers = projects
            .Select(project => SanitizeFileNamePart((project.ProjectNumber ?? string.Empty).Replace(' ', '_')))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var numberPart = distinctNumbers.Count == 0
            ? "Diverse"
            : string.Join("_", distinctNumbers);

        return "Kalkulations-Sammelexport_" + numberPart + ".pdf";
    }

    private static string SanitizeFileNamePart(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        var cleaned = new string(value.Where(ch => !invalidChars.Contains(ch)).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(cleaned) ? string.Empty : cleaned;
    }
}
