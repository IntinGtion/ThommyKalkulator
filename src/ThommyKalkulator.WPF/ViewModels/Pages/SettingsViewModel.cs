using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThommyKalkulator.Application.Interfaces;
using ThommyKalkulator.Domain.Models;

namespace ThommyKalkulator.WPF.ViewModels.Pages;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IAppState? _appState;
    private Machine? _selectedMachineModel;

    [ObservableProperty]
    private string pageTitle = "Einstellungen";

    [ObservableProperty]
    private string pageDescription = "Hier werden globale Preise, Maschinen und Anwendungsoptionen verwaltet.";

    [ObservableProperty]
    private string statusMessage = "Bereit.";

    [ObservableProperty]
    private MachineListItemViewModel? selectedMachine;

    [ObservableProperty]
    private string machineName = string.Empty;

    [ObservableProperty]
    private string machineWattText = "0";

    [ObservableProperty]
    private string machinePurchasePriceText = "0";

    [ObservableProperty]
    private string machineLifetimeHoursText = "0";

    [ObservableProperty]
    private string currency = "€";

    [ObservableProperty]
    private string electricityPricePerKwhText = "0";

    [ObservableProperty]
    private string laborRateText = "0";

    [ObservableProperty]
    private string constructionLaborRateText = "0";

    [ObservableProperty]
    private string defaultSurchargePercentText = "0";

    [ObservableProperty]
    private string logoPath = string.Empty;

    [ObservableProperty]
    private bool floatingLiveDefault;

    [ObservableProperty]
    private bool pdfAutoOpen;

    public ObservableCollection<MachineListItemViewModel> Machines { get; } = new();

    public SettingsViewModel()
        : this(null)
    {
    }

    public SettingsViewModel(IAppState? appState)
    {
        _appState = appState;

        if (_appState is null)
        {
            StatusMessage = "Designmodus oder kein AppState verfügbar.";
            return;
        }

        _appState.DataChanged += OnAppStateDataChanged;
        LoadFromAppState();
    }

    partial void OnSelectedMachineChanged(MachineListItemViewModel? value)
    {
        LoadMachineToEditor(value?.Machine);
    }

    [RelayCommand]
    private void NewMachine()
    {
        _selectedMachineModel = null;
        SelectedMachine = null;
        ClearMachineEditor();
        StatusMessage = "Neue Maschine vorbereitet.";
    }

    [RelayCommand]
    private void SaveMachine()
    {
        if (_appState is null)
        {
            return;
        }

        var trimmedName = MachineName.Trim();

        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            StatusMessage = "Der Maschinenname darf nicht leer sein.";
            return;
        }

        if (!TryReadDecimal(MachineWattText, out var machineWatt)
            || !TryReadDecimal(MachinePurchasePriceText, out var machinePurchasePrice)
            || !TryReadDecimal(MachineLifetimeHoursText, out var machineLifetimeHours))
        {
            StatusMessage = "Bitte für Watt, Kaufpreis und Lebensdauer gültige Zahlen eingeben.";
            return;
        }

        if (_selectedMachineModel is null)
        {
            var newMachine = new Machine
            {
                Name = trimmedName,
                Watt = machineWatt,
                PurchasePrice = machinePurchasePrice,
                LifetimeHours = machineLifetimeHours
            };

            _appState.CurrentData.Machines.Add(newMachine);
        }
        else
        {
            _selectedMachineModel.Name = trimmedName;
            _selectedMachineModel.Watt = machineWatt;
            _selectedMachineModel.PurchasePrice = machinePurchasePrice;
            _selectedMachineModel.LifetimeHours = machineLifetimeHours;
        }

        _appState.Save();
        ReloadMachines(trimmedName);
        StatusMessage = $"Maschine '{trimmedName}' gespeichert.";
    }

    [RelayCommand]
    private void DeleteMachine()
    {
        if (_appState is null || _selectedMachineModel is null)
        {
            return;
        }

        var deletedName = _selectedMachineModel.Name;
        _appState.CurrentData.Machines.Remove(_selectedMachineModel);
        _appState.Save();

        _selectedMachineModel = null;
        SelectedMachine = null;
        ClearMachineEditor();
        ReloadMachines(null);
        StatusMessage = $"Maschine '{deletedName}' gelöscht.";
    }

    [RelayCommand]
    private void SaveGlobalSettings()
    {
        if (_appState is null)
        {
            return;
        }

        if (!TryReadDecimal(ElectricityPricePerKwhText, out var electricityPricePerKwh)
            || !TryReadDecimal(LaborRateText, out var laborRate)
            || !TryReadDecimal(ConstructionLaborRateText, out var constructionLaborRate)
            || !TryReadDecimal(DefaultSurchargePercentText, out var defaultSurchargePercent))
        {
            StatusMessage = "Bitte für Strompreis, Stundenlöhne und Aufschlag gültige Zahlen eingeben.";
            return;
        }

        var globalSettings = _appState.CurrentData.GlobalSettings;
        globalSettings.Currency = Currency.Trim();
        globalSettings.ElectricityPricePerKwh = electricityPricePerKwh;
        globalSettings.LaborRate = laborRate;
        globalSettings.ConstructionLaborRate = constructionLaborRate;
        globalSettings.DefaultSurchargePercent = defaultSurchargePercent;
        globalSettings.LogoPath = LogoPath.Trim();
        globalSettings.FloatingLiveDefault = FloatingLiveDefault;
        globalSettings.PdfAutoOpen = PdfAutoOpen;

        _appState.Save();
        StatusMessage = "Globale Einstellungen gespeichert.";
    }

    private void LoadFromAppState()
    {
        if (_appState is null)
        {
            return;
        }

        var globalSettings = _appState.CurrentData.GlobalSettings;
        Currency = globalSettings.Currency;
        ElectricityPricePerKwhText = FormatDecimal(globalSettings.ElectricityPricePerKwh);
        LaborRateText = FormatDecimal(globalSettings.LaborRate);
        ConstructionLaborRateText = FormatDecimal(globalSettings.ConstructionLaborRate);
        DefaultSurchargePercentText = FormatDecimal(globalSettings.DefaultSurchargePercent);
        LogoPath = globalSettings.LogoPath;
        FloatingLiveDefault = globalSettings.FloatingLiveDefault;
        PdfAutoOpen = globalSettings.PdfAutoOpen;

        ReloadMachines(SelectedMachine?.Name);

        if (SelectedMachine is null)
        {
            ClearMachineEditor();
        }
    }

    private void ReloadMachines(string? selectedMachineName)
    {
        if (_appState is null)
        {
            return;
        }

        Machines.Clear();

        foreach (var machine in _appState.CurrentData.Machines.OrderBy(machine => machine.Name))
        {
            Machines.Add(new MachineListItemViewModel(machine));
        }

        if (!string.IsNullOrWhiteSpace(selectedMachineName))
        {
            SelectedMachine = Machines.FirstOrDefault(machine =>
                string.Equals(machine.Name, selectedMachineName, StringComparison.OrdinalIgnoreCase));
        }
    }

    private void LoadMachineToEditor(Machine? machine)
    {
        _selectedMachineModel = machine;

        if (machine is null)
        {
            ClearMachineEditor();
            return;
        }

        MachineName = machine.Name;
        MachineWattText = FormatDecimal(machine.Watt);
        MachinePurchasePriceText = FormatDecimal(machine.PurchasePrice);
        MachineLifetimeHoursText = FormatDecimal(machine.LifetimeHours);
        StatusMessage = $"Maschine '{machine.Name}' geladen.";
    }

    private void ClearMachineEditor()
    {
        MachineName = string.Empty;
        MachineWattText = "0";
        MachinePurchasePriceText = "0";
        MachineLifetimeHoursText = "0";
    }

    private void OnAppStateDataChanged(object? sender, EventArgs e)
    {
        var selectedMachineName = SelectedMachine?.Name;
        LoadFromAppState();

        if (!string.IsNullOrWhiteSpace(selectedMachineName))
        {
            SelectedMachine = Machines.FirstOrDefault(machine =>
                string.Equals(machine.Name, selectedMachineName, StringComparison.OrdinalIgnoreCase));
        }
    }

    private static string FormatDecimal(decimal value)
    {
        return value.ToString(CultureInfo.CurrentCulture);
    }

    private static bool TryReadDecimal(string text, out decimal value)
    {
        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out value)
            || decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }
}

public sealed class MachineListItemViewModel
{
    public MachineListItemViewModel(Machine machine)
    {
        Machine = machine ?? throw new ArgumentNullException(nameof(machine));
    }

    public Machine Machine { get; }

    public string Name => Machine.Name;
}
