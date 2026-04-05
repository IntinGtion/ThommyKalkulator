using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThommyKalkulator.Application.Interfaces;
using ThommyKalkulator.Domain.Models;

namespace ThommyKalkulator.WPF.ViewModels.Pages;

public sealed class CalculationViewModel : ObservableObject
{
    private readonly IAppState _appState;
    private readonly ICalculationService _calculationService;

    private string _pageTitle = "Kalkulation erstellen";
    private string _pageDescription = "Maschinen, Materialien und Arbeitszeiten werden direkt aus dem gemeinsamen Datenstand kalkuliert.";
    private string _statusMessage = "Bereit";
    private string _projectNumber = string.Empty;
    private string _positionName = string.Empty;
    private string _quantityText = "1";
    private string _selectedTimeUnit = "Stunden";
    private string _preparationTimeText = string.Empty;
    private string _postProcessingTimeText = string.Empty;
    private string _constructionTimeText = string.Empty;
    private string _surchargePercentText = string.Empty;
    private string _note = string.Empty;
    private bool _isUpdatingForm;
    private int? _editingProjectIndex;
    private CalculationProject _previewProject = new();
    private string _floatingPreviewText = "Eingaben vornehmen – Preis wird live berechnet.";

    public CalculationViewModel(IAppState appState, ICalculationService calculationService)
    {
        _appState = appState ?? throw new ArgumentNullException(nameof(appState));
        _calculationService = calculationService ?? throw new ArgumentNullException(nameof(calculationService));

        TimeUnits = new ObservableCollection<string>
        {
            "Stunden",
            "Minuten"
        };

        Machines = new ObservableCollection<CalculationMachineItemViewModel>();
        Materials = new ObservableCollection<CalculationMaterialItemViewModel>();
        FreeCosts = new ObservableCollection<CalculationFreeCostItemViewModel>();

        AddFreeCostCommand = new RelayCommand(AddFreeCost);
        RemoveFreeCostCommand = new RelayCommand<CalculationFreeCostItemViewModel?>(RemoveFreeCost);
        SaveCalculationCommand = new RelayCommand(SaveCalculation);
        ClearFormCommand = new RelayCommand(ClearForm);

        _appState.DataChanged += OnAppStateDataChanged;

        LoadReferenceData();
        AddFreeCost();
        Recalculate();
    }

    public string PageTitle
    {
        get => _pageTitle;
        set => SetProperty(ref _pageTitle, value);
    }

    public string PageDescription
    {
        get => _pageDescription;
        set => SetProperty(ref _pageDescription, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string SaveButtonText => _editingProjectIndex.HasValue
        ? "Änderungen speichern"
        : "Kalkulation speichern";

    public string ProjectNumber
    {
        get => _projectNumber;
        set
        {
            if (SetProperty(ref _projectNumber, value))
            {
                Recalculate();
            }
        }
    }

    public string PositionName
    {
        get => _positionName;
        set
        {
            if (SetProperty(ref _positionName, value))
            {
                Recalculate();
            }
        }
    }

    public string QuantityText
    {
        get => _quantityText;
        set
        {
            if (SetProperty(ref _quantityText, value))
            {
                Recalculate();
            }
        }
    }

    public ObservableCollection<string> TimeUnits { get; }

    public string SelectedTimeUnit
    {
        get => _selectedTimeUnit;
        set
        {
            if (SetProperty(ref _selectedTimeUnit, value))
            {
                Recalculate();
            }
        }
    }

    public string PreparationTimeText
    {
        get => _preparationTimeText;
        set
        {
            if (SetProperty(ref _preparationTimeText, value))
            {
                Recalculate();
            }
        }
    }

    public string PostProcessingTimeText
    {
        get => _postProcessingTimeText;
        set
        {
            if (SetProperty(ref _postProcessingTimeText, value))
            {
                Recalculate();
            }
        }
    }

    public string ConstructionTimeText
    {
        get => _constructionTimeText;
        set
        {
            if (SetProperty(ref _constructionTimeText, value))
            {
                Recalculate();
            }
        }
    }

    public string SurchargePercentText
    {
        get => _surchargePercentText;
        set
        {
            if (SetProperty(ref _surchargePercentText, value))
            {
                Recalculate();
            }
        }
    }

    public string Note
    {
        get => _note;
        set
        {
            if (SetProperty(ref _note, value))
            {
                Recalculate();
            }
        }
    }

    public ObservableCollection<CalculationMachineItemViewModel> Machines { get; }

    public ObservableCollection<CalculationMaterialItemViewModel> Materials { get; }

    public ObservableCollection<CalculationFreeCostItemViewModel> FreeCosts { get; }

    public string Currency => string.IsNullOrWhiteSpace(_appState.CurrentData.GlobalSettings.Currency)
        ? "€"
        : _appState.CurrentData.GlobalSettings.Currency;

    public string PowerCostText => FormatMoney(_previewProject.PowerCost);

    public string WearCostText => FormatMoney(_previewProject.WearCost);

    public string MaterialCostText => FormatMoney(_previewProject.MaterialCost);

    public string PreparationCostText => FormatMoney(_previewProject.PreparationCost);

    public string LaborCostText => FormatMoney(_previewProject.LaborCost);

    public string ConstructionCostText => FormatMoney(_previewProject.ConstructionCost);

    public string FreeCostsTotalText => FormatMoney(_previewProject.AdditionalCostTotal);

    public string CostPriceText => FormatMoney(_previewProject.CostPrice);

    public string SurchargeAmountText => FormatMoney(_previewProject.FinalPrice - _previewProject.CostPrice);

    public string UnitPriceText => FormatMoney(_previewProject.FinalPrice);

    public string TotalPriceText => FormatMoney(_previewProject.FinalPrice * _previewProject.Quantity);

    public string FloatingPreviewText
    {
        get => _floatingPreviewText;
        private set => SetProperty(ref _floatingPreviewText, value);
    }

    public IRelayCommand AddFreeCostCommand { get; }

    public IRelayCommand<CalculationFreeCostItemViewModel?> RemoveFreeCostCommand { get; }

    public IRelayCommand SaveCalculationCommand { get; }

    public IRelayCommand ClearFormCommand { get; }

    private void OnAppStateDataChanged(object? sender, EventArgs e)
    {
        LoadReferenceData();
        Recalculate();
    }

    private void LoadReferenceData()
    {
        var machineStateByName = Machines.ToDictionary(
            item => item.Name,
            item => new MachineState(item.IsSelected, item.RuntimeHoursText),
            StringComparer.OrdinalIgnoreCase);

        var materialStateByName = Materials.ToDictionary(
            item => item.Name,
            item => new MaterialState(item.IsSelected, item.QuantityText),
            StringComparer.OrdinalIgnoreCase);

        foreach (var machineItem in Machines)
        {
            machineItem.PropertyChanged -= OnMachineItemPropertyChanged;
        }

        foreach (var materialItem in Materials)
        {
            materialItem.PropertyChanged -= OnMaterialItemPropertyChanged;
        }

        Machines.Clear();
        foreach (var machine in _appState.CurrentData.Machines)
        {
            MachineState existingState;
            var hasState = machineStateByName.TryGetValue(machine.Name, out existingState);
            var runtimeText = hasState ? existingState.RuntimeHoursText : "1";
            var isSelected = hasState && existingState.IsSelected;
            var item = new CalculationMachineItemViewModel(machine, runtimeText, isSelected);
            item.PropertyChanged += OnMachineItemPropertyChanged;
            Machines.Add(item);
        }

        Materials.Clear();
        foreach (var material in _appState.CurrentData.Materials)
        {
            MaterialState existingState;
            var hasState = materialStateByName.TryGetValue(material.Name, out existingState);
            var quantityText = hasState ? existingState.QuantityText : "0";
            var isSelected = hasState && existingState.IsSelected;
            var item = new CalculationMaterialItemViewModel(material, quantityText, isSelected);
            item.PropertyChanged += OnMaterialItemPropertyChanged;
            Materials.Add(item);
        }

        if (string.IsNullOrWhiteSpace(SurchargePercentText))
        {
            SurchargePercentText = FormatDecimal(_appState.CurrentData.GlobalSettings.DefaultSurchargePercent);
        }
    }

    private void OnMachineItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Recalculate();
    }

    private void OnMaterialItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Recalculate();
    }

    private void AddFreeCost()
    {
        var item = new CalculationFreeCostItemViewModel();
        item.PropertyChanged += OnFreeCostItemPropertyChanged;
        FreeCosts.Add(item);
        Recalculate();
    }

    private void RemoveFreeCost(CalculationFreeCostItemViewModel? item)
    {
        if (item is null)
        {
            return;
        }

        item.PropertyChanged -= OnFreeCostItemPropertyChanged;
        FreeCosts.Remove(item);
        Recalculate();
    }

    private void OnFreeCostItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Recalculate();
    }

    public void LoadProjectForEditing(CalculationProject project, int editingIndex)
    {
        if (project is null)
        {
            throw new ArgumentNullException(nameof(project));
        }

        _isUpdatingForm = true;
        _editingProjectIndex = editingIndex;

        ProjectNumber = project.ProjectNumber;
        PositionName = project.Name;
        QuantityText = project.Quantity.ToString(CultureInfo.InvariantCulture);
        SelectedTimeUnit = string.Equals(project.TimeUnit, "Minuten", StringComparison.OrdinalIgnoreCase)
            ? "Minuten"
            : "Stunden";

        var timeMultiplier = string.Equals(SelectedTimeUnit, "Minuten", StringComparison.OrdinalIgnoreCase) ? 60m : 1m;
        PreparationTimeText = FormatDecimal(project.PreparationHours * timeMultiplier);
        PostProcessingTimeText = FormatDecimal(project.PostProcessingHours * timeMultiplier);
        ConstructionTimeText = FormatDecimal(project.ConstructionHours * timeMultiplier);
        SurchargePercentText = FormatDecimal(project.SurchargePercent);
        Note = project.Note;

        foreach (var machine in Machines)
        {
            var usage = project.MachineUsages.FirstOrDefault(item => string.Equals(item.Name, machine.Name, StringComparison.OrdinalIgnoreCase));
            machine.IsSelected = usage is not null;
            machine.RuntimeHoursText = usage is null
                ? "1"
                : FormatDecimal(usage.RuntimeHours);
        }

        foreach (var material in Materials)
        {
            var usage = project.MaterialUsages.FirstOrDefault(item => string.Equals(item.Name, material.Name, StringComparison.OrdinalIgnoreCase));
            material.IsSelected = usage is not null;
            material.QuantityText = usage is null
                ? "0"
                : FormatDecimal(usage.Quantity);
        }

        foreach (var freeCost in FreeCosts)
        {
            freeCost.PropertyChanged -= OnFreeCostItemPropertyChanged;
        }

        FreeCosts.Clear();
        foreach (var freeCostItem in project.FreeCostItems)
        {
            var viewModel = new CalculationFreeCostItemViewModel
            {
                Description = freeCostItem.Description,
                AmountText = FormatDecimal(freeCostItem.Amount)
            };
            viewModel.PropertyChanged += OnFreeCostItemPropertyChanged;
            FreeCosts.Add(viewModel);
        }

        if (FreeCosts.Count == 0)
        {
            AddFreeCost();
        }

        _isUpdatingForm = false;
        OnPropertyChanged(nameof(SaveButtonText));
        Recalculate();
        StatusMessage = "Kalkulation wurde zur Bearbeitung geladen.";
    }

    private void SaveCalculation()
    {
        if (string.IsNullOrWhiteSpace(ProjectNumber))
        {
            StatusMessage = "Bitte Projektnummer eingeben.";
            return;
        }

        if (string.IsNullOrWhiteSpace(PositionName))
        {
            StatusMessage = "Bitte Positionsbezeichnung eingeben.";
            return;
        }

        var project = BuildProjectFromForm(out var errors);
        if (errors.Count > 0)
        {
            StatusMessage = "Bitte folgende Felder korrigieren: " + string.Join(", ", errors) + ".";
            return;
        }

        if (_editingProjectIndex.HasValue
            && _editingProjectIndex.Value >= 0
            && _editingProjectIndex.Value < _appState.CurrentData.Projects.Count)
        {
            _appState.CurrentData.Projects[_editingProjectIndex.Value] = project;
            StatusMessage = "Kalkulation wurde aktualisiert.";
        }
        else
        {
            _appState.CurrentData.Projects.Add(project);
            StatusMessage = "Kalkulation wurde gespeichert.";
        }

        _editingProjectIndex = null;
        OnPropertyChanged(nameof(SaveButtonText));
        _appState.Save();
    }

    private void ClearForm()
    {
        _editingProjectIndex = null;
        OnPropertyChanged(nameof(SaveButtonText));

        ProjectNumber = string.Empty;
        PositionName = string.Empty;
        QuantityText = "1";
        SelectedTimeUnit = "Stunden";
        PreparationTimeText = string.Empty;
        PostProcessingTimeText = string.Empty;
        ConstructionTimeText = string.Empty;
        SurchargePercentText = FormatDecimal(_appState.CurrentData.GlobalSettings.DefaultSurchargePercent);
        Note = string.Empty;

        foreach (var machine in Machines)
        {
            machine.IsSelected = false;
            machine.RuntimeHoursText = "1";
        }

        foreach (var material in Materials)
        {
            material.IsSelected = false;
            material.QuantityText = "0";
        }

        foreach (var freeCost in FreeCosts)
        {
            freeCost.PropertyChanged -= OnFreeCostItemPropertyChanged;
        }

        FreeCosts.Clear();
        AddFreeCost();
        StatusMessage = "Formular wurde geleert.";
    }

    private void Recalculate()
    {
        if (_isUpdatingForm)
        {
            return;
        }

        var project = BuildProjectFromForm(out var errors);
        _previewProject = project;

        foreach (var machine in Machines)
        {
            machine.UpdateCalculatedValues(_previewProject, _appState.CurrentData.GlobalSettings, Currency);
        }

        foreach (var material in Materials)
        {
            material.UpdateCalculatedValues(_previewProject, Currency);
        }

        OnPropertyChanged(nameof(Currency));
        OnPropertyChanged(nameof(PowerCostText));
        OnPropertyChanged(nameof(WearCostText));
        OnPropertyChanged(nameof(MaterialCostText));
        OnPropertyChanged(nameof(PreparationCostText));
        OnPropertyChanged(nameof(LaborCostText));
        OnPropertyChanged(nameof(ConstructionCostText));
        OnPropertyChanged(nameof(FreeCostsTotalText));
        OnPropertyChanged(nameof(CostPriceText));
        OnPropertyChanged(nameof(SurchargeAmountText));
        OnPropertyChanged(nameof(UnitPriceText));
        OnPropertyChanged(nameof(TotalPriceText));

        FloatingPreviewText = BuildFloatingPreviewText(_previewProject, errors);

        if (errors.Count == 0)
        {
            StatusMessage = "Vorschau aktualisiert.";
        }
        else
        {
            StatusMessage = "Ungültige Felder: " + string.Join(", ", errors) + ".";
        }
    }

    private CalculationProject BuildProjectFromForm(out List<string> errors)
    {
        errors = new List<string>();
        var settings = _appState.CurrentData.GlobalSettings;
        var isMinutes = string.Equals(SelectedTimeUnit, "Minuten", StringComparison.OrdinalIgnoreCase);
        var timeDivisor = isMinutes ? 60m : 1m;

        var project = new CalculationProject
        {
            ProjectNumber = ProjectNumber.Trim(),
            Name = PositionName.Trim(),
            Date = DateTime.Now.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture),
            TimeUnit = isMinutes ? "Minuten" : "Stunden",
            Note = Note.Trim(),
            Quantity = ParseQuantity(QuantityText),
            FreeCostItems = new List<FreeCostItem>()
        };

        decimal parsedPreparation;
        if (!TryParseDecimal(PreparationTimeText, out parsedPreparation))
        {
            errors.Add("Vorbereitung");
            parsedPreparation = 0m;
        }
        project.PreparationHours = parsedPreparation / timeDivisor;

        decimal parsedPostProcessing;
        if (!TryParseDecimal(PostProcessingTimeText, out parsedPostProcessing))
        {
            errors.Add("Nachbearbeitung");
            parsedPostProcessing = 0m;
        }
        project.PostProcessingHours = parsedPostProcessing / timeDivisor;

        decimal parsedConstruction;
        if (!TryParseDecimal(ConstructionTimeText, out parsedConstruction))
        {
            errors.Add("Konstruktion");
            parsedConstruction = 0m;
        }
        project.ConstructionHours = parsedConstruction / timeDivisor;

        if (string.IsNullOrWhiteSpace(SurchargePercentText))
        {
            project.SurchargePercent = settings.DefaultSurchargePercent;
        }
        else
        {
            decimal parsedSurcharge;
            if (!TryParseDecimal(SurchargePercentText, out parsedSurcharge))
            {
                errors.Add("Aufschlag");
                parsedSurcharge = settings.DefaultSurchargePercent;
            }

            project.SurchargePercent = parsedSurcharge;
        }

        project.MachineUsages = new List<MachineUsage>();
        foreach (var machine in Machines.Where(item => item.IsSelected))
        {
            decimal runtimeHours;
            if (!TryParseDecimal(machine.RuntimeHoursText, out runtimeHours))
            {
                errors.Add("Laufzeit '" + machine.Name + "'");
                continue;
            }

            if (runtimeHours <= 0m)
            {
                continue;
            }

            project.MachineUsages.Add(new MachineUsage
            {
                Name = machine.Name,
                RuntimeHours = runtimeHours,
                Watt = machine.Watt,
                PurchasePrice = machine.PurchasePrice,
                LifetimeHours = machine.LifetimeHours
            });
        }

        project.MaterialUsages = new List<MaterialUsage>();
        foreach (var material in Materials.Where(item => item.IsSelected))
        {
            decimal quantity;
            if (!TryParseDecimal(material.QuantityText, out quantity))
            {
                errors.Add("Menge '" + material.Name + "'");
                continue;
            }

            if (quantity <= 0m)
            {
                continue;
            }

            project.MaterialUsages.Add(new MaterialUsage
            {
                Name = material.Name,
                Quantity = quantity,
                Unit = material.Unit,
                Price = material.Price
            });
        }

        foreach (var freeCost in FreeCosts)
        {
            decimal amount;
            if (!TryParseDecimal(freeCost.AmountText, out amount) || amount == 0m)
            {
                continue;
            }

            project.FreeCostItems.Add(new FreeCostItem
            {
                Description = string.IsNullOrWhiteSpace(freeCost.Description)
                    ? "Freie Position"
                    : freeCost.Description.Trim(),
                Amount = amount
            });
        }

        return _calculationService.Calculate(project, settings);
    }

    private int ParseQuantity(string text)
    {
        decimal parsedQuantity;
        if (!TryParseDecimal(text, out parsedQuantity) || parsedQuantity <= 0m)
        {
            return 1;
        }

        return Math.Max(1, (int)parsedQuantity);
    }

    private string FormatMoney(decimal value)
    {
        return value.ToString("N2", CultureInfo.CurrentCulture) + " " + Currency;
    }

    private static string FormatDecimal(decimal value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static bool TryParseDecimal(string? text, out decimal value)
    {
        var normalized = (text ?? string.Empty).Trim();
        if (normalized.Length == 0)
        {
            value = 0m;
            return true;
        }

        normalized = normalized.Replace(',', '.');
        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }


    private string BuildFloatingPreviewText(CalculationProject project, IReadOnlyCollection<string> errors)
    {
        var lines = new List<string>();

        if (errors.Count > 0)
        {
            lines.Add("⚠ Ungültige Felder: " + string.Join(", ", errors));
            lines.Add(string.Empty);
        }

        foreach (var machine in project.MachineUsages)
        {
            lines.Add(
                machine.Name.PadRight(22)
                + " " + FormatDecimal(machine.RuntimeHours)
                + " Std.  →  Strom " + FormatMoney(machine.PowerCost)
                + "  |  Verschleiß " + FormatMoney(machine.WearCost));
        }

        if (project.MachineUsages.Count > 0)
        {
            lines.Add(new string('─', 58));
        }

        foreach (var material in project.MaterialUsages)
        {
            lines.Add(
                material.Name.PadRight(22)
                + " " + FormatDecimal(material.Quantity)
                + " " + material.Unit
                + "  →  " + FormatMoney(material.Cost));
        }

        if (project.MaterialUsages.Count > 0)
        {
            lines.Add(new string('─', 58));
        }

        lines.Add("Strom gesamt:".PadRight(28) + FormatMoney(project.PowerCost));
        lines.Add("Verschleiß gesamt:".PadRight(28) + FormatMoney(project.WearCost));
        lines.Add("Materialkosten:".PadRight(28) + FormatMoney(project.MaterialCost));
        lines.Add("Vorbereitungskosten:".PadRight(28) + FormatMoney(project.PreparationCost));
        lines.Add("Nachbearbeitungskosten:".PadRight(28) + FormatMoney(project.LaborCost));
        lines.Add("CAD/RE-Kosten:".PadRight(28) + FormatMoney(project.ConstructionCost));

        foreach (var freeCost in project.FreeCostItems)
        {
            lines.Add((freeCost.Description + ":").PadRight(28) + FormatMoney(freeCost.Amount));
        }

        lines.Add(new string('─', 44));
        lines.Add("Selbstkosten:".PadRight(28) + FormatMoney(project.CostPrice));
        lines.Add(("Aufschlag (" + FormatDecimal(project.SurchargePercent) + "%):").PadRight(28)
            + FormatMoney(project.FinalPrice - project.CostPrice));
        lines.Add(new string('─', 44));
        lines.Add("Einzelpreis:".PadRight(28) + FormatMoney(project.FinalPrice));

        var quantity = Math.Max(1, project.Quantity);
        if (quantity > 1)
        {
            lines.Add("Stückzahl:".PadRight(28) + quantity + "x");
            lines.Add(new string('─', 44));
            lines.Add("Gesamtpreis:".PadRight(28) + FormatMoney(project.FinalPrice * quantity));
        }

        lines.Add(string.Empty);
        lines.Add("⟳ Vorschau – noch nicht gespeichert");

        return string.Join(Environment.NewLine, lines);
    }

    private readonly record struct MachineState(bool IsSelected, string RuntimeHoursText);

    private readonly record struct MaterialState(bool IsSelected, string QuantityText);
}

public sealed class CalculationMachineItemViewModel : ObservableObject
{
    private readonly Machine _machine;

    private bool _isSelected;
    private string _runtimeHoursText;
    private string _powerPerHourText = string.Empty;
    private string _wearPerHourText = string.Empty;
    private string _totalCostText = string.Empty;

    public CalculationMachineItemViewModel(Machine machine, string runtimeHoursText, bool isSelected)
    {
        _machine = machine ?? throw new ArgumentNullException(nameof(machine));
        _runtimeHoursText = runtimeHoursText;
        _isSelected = isSelected;
    }

    public string Name => _machine.Name;

    public decimal Watt => _machine.Watt;

    public decimal PurchasePrice => _machine.PurchasePrice;

    public decimal LifetimeHours => _machine.LifetimeHours;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public string RuntimeHoursText
    {
        get => _runtimeHoursText;
        set => SetProperty(ref _runtimeHoursText, value);
    }

    public string PowerPerHourText
    {
        get => _powerPerHourText;
        private set => SetProperty(ref _powerPerHourText, value);
    }

    public string WearPerHourText
    {
        get => _wearPerHourText;
        private set => SetProperty(ref _wearPerHourText, value);
    }

    public string TotalCostText
    {
        get => _totalCostText;
        private set => SetProperty(ref _totalCostText, value);
    }

    public void UpdateCalculatedValues(CalculationProject previewProject, GlobalSettings settings, string currency)
    {
        var matchingUsage = previewProject.MachineUsages.FirstOrDefault(item => string.Equals(item.Name, Name, StringComparison.OrdinalIgnoreCase));
        var lifetimeHours = LifetimeHours > 0m ? LifetimeHours : 1m;
        var powerPerHour = Watt / 1000m * settings.ElectricityPricePerKwh;
        var wearPerHour = PurchasePrice / lifetimeHours;

        PowerPerHourText = powerPerHour.ToString("N2", CultureInfo.CurrentCulture) + " " + currency + "/h";
        WearPerHourText = wearPerHour.ToString("N2", CultureInfo.CurrentCulture) + " " + currency + "/h";
        var totalCost = matchingUsage is null ? 0m : matchingUsage.PowerCost + matchingUsage.WearCost;
        TotalCostText = totalCost.ToString("N2", CultureInfo.CurrentCulture) + " " + currency;
    }
}

public sealed class CalculationMaterialItemViewModel : ObservableObject
{
    private readonly Material _material;

    private bool _isSelected;
    private string _quantityText;
    private string _unitPriceText = string.Empty;
    private string _totalCostText = string.Empty;

    public CalculationMaterialItemViewModel(Material material, string quantityText, bool isSelected)
    {
        _material = material ?? throw new ArgumentNullException(nameof(material));
        _quantityText = quantityText;
        _isSelected = isSelected;
    }

    public string Name => _material.Name;

    public string Unit => _material.Unit;

    public decimal Price => _material.Price;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public string QuantityText
    {
        get => _quantityText;
        set => SetProperty(ref _quantityText, value);
    }

    public string UnitPriceText
    {
        get => _unitPriceText;
        private set => SetProperty(ref _unitPriceText, value);
    }

    public string TotalCostText
    {
        get => _totalCostText;
        private set => SetProperty(ref _totalCostText, value);
    }

    public void UpdateCalculatedValues(CalculationProject previewProject, string currency)
    {
        UnitPriceText = Price.ToString("N2", CultureInfo.CurrentCulture) + " " + currency + "/" + Unit;
        var matchingUsage = previewProject.MaterialUsages.FirstOrDefault(item => string.Equals(item.Name, Name, StringComparison.OrdinalIgnoreCase));
        var totalCost = matchingUsage is null ? 0m : matchingUsage.Cost;
        TotalCostText = totalCost.ToString("N2", CultureInfo.CurrentCulture) + " " + currency;
    }
}

public sealed class CalculationFreeCostItemViewModel : ObservableObject
{
    private string _description = string.Empty;
    private string _amountText = string.Empty;

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string AmountText
    {
        get => _amountText;
        set => SetProperty(ref _amountText, value);
    }
}
