using System.ComponentModel;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using ThommyKalkulator.WPF.Models;
using ThommyKalkulator.WPF.Services;
using ThommyKalkulator.WPF.ViewModels.Pages;

namespace ThommyKalkulator.WPF.Views.Pages;

/// <summary>
/// Interaktionslogik für SettingsPage.xaml
/// </summary>
public partial class SettingsPage : UserControl
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

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

    private void ExportConfigurationButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Konfiguration exportieren",
            Filter = "JSON Backup (*.json)|*.json|Alle Dateien (*.*)|*.*",
            FileName = "thommy_kalkulator_backup.json"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var backup = AppBackupFile.From(App.AppState.CurrentData, App.CurrentConfiguration);
            var json = JsonSerializer.Serialize(backup, _jsonOptions);
            File.WriteAllText(dialog.FileName, json);

            MessageBox.Show(
                "Konfiguration erfolgreich exportiert:\n" + dialog.FileName,
                "Export erfolgreich",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Die Konfiguration konnte nicht exportiert werden:\n" + ex.Message,
                "Exportfehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ImportConfigurationButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Konfiguration importieren",
            Filter = "JSON Backup (*.json)|*.json|Alle Dateien (*.*)|*.*"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var json = File.ReadAllText(dialog.FileName);
            var backup = JsonSerializer.Deserialize<AppBackupFile>(json, _jsonOptions);

            if (backup is null)
            {
                throw new InvalidOperationException("Die Datei konnte nicht als Backup gelesen werden.");
            }

            var importedData = backup.ToAppData();
            App.AppState.Replace(importedData);
            App.AppState.Save();

            if (backup.UiConfiguration is not null)
            {
                App.UpdateConfiguration(backup.UiConfiguration);
            }

            if (DataContext is SettingsViewModel viewModel)
            {
                viewModel.StatusMessage = "Konfiguration wurde importiert.";
            }

            MessageBox.Show(
                "Konfiguration erfolgreich importiert.",
                "Import erfolgreich",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Die Konfiguration konnte nicht importiert werden:\n" + ex.Message,
                "Importfehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
