using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThommyKalkulator.Application.Interfaces;
using ThommyKalkulator.Domain.Models;

namespace ThommyKalkulator.WPF.ViewModels.Pages;

public sealed class MaterialsViewModel : ObservableObject
{
    private static readonly string[] DefaultUnitSuggestions =
    [
        "g",
        "kg",
        "ml",
        "l",
        "Stück",
        "m",
        "cm",
        "mm",
        "Rolle",
        "Flasche"
    ];

    private readonly IAppState _appState;

    private string _pageTitle = "Materialien";
    private string _pageDescription = "Hier werden Materialtypen und Materialien verwaltet.";
    private string _statusMessage = "Bereit";
    private string _typeEditorText = string.Empty;
    private string? _selectedMaterialType;
    private MaterialListItemViewModel? _selectedMaterial;
    private string _materialName = string.Empty;
    private string _materialType = string.Empty;
    private string _materialUnit = "g";
    private string _materialPriceText = string.Empty;
    private string _materialManufacturer = string.Empty;
    private string _materialSupplier = string.Empty;
    private string _materialNote = string.Empty;
    private string? _pendingSelectedTypeName;
    private string? _pendingSelectedMaterialName;

    public MaterialsViewModel(IAppState appState)
    {
        _appState = appState ?? throw new ArgumentNullException(nameof(appState));
        _appState.DataChanged += OnAppStateDataChanged;

        MaterialTypes = new ObservableCollection<string>();
        Materials = new ObservableCollection<MaterialListItemViewModel>();
        UnitSuggestions = new ObservableCollection<string>(DefaultUnitSuggestions);

        AddTypeCommand = new RelayCommand(AddType);
        RenameTypeCommand = new RelayCommand(RenameType);
        DeleteTypeCommand = new RelayCommand(DeleteType);
        AddMaterialCommand = new RelayCommand(AddMaterial);
        UpdateMaterialCommand = new RelayCommand(UpdateMaterial);
        DeleteMaterialCommand = new RelayCommand(DeleteMaterial);
        ClearMaterialFormCommand = new RelayCommand(ClearMaterialForm);

        LoadFromAppState();
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

    public ObservableCollection<string> MaterialTypes { get; }

    public ObservableCollection<MaterialListItemViewModel> Materials { get; }

    public ObservableCollection<string> UnitSuggestions { get; }

    public string TypeEditorText
    {
        get => _typeEditorText;
        set => SetProperty(ref _typeEditorText, value);
    }

    public string? SelectedMaterialType
    {
        get => _selectedMaterialType;
        set
        {
            if (SetProperty(ref _selectedMaterialType, value) && !string.IsNullOrWhiteSpace(value))
            {
                TypeEditorText = value;
            }
        }
    }

    public MaterialListItemViewModel? SelectedMaterial
    {
        get => _selectedMaterial;
        set
        {
            if (SetProperty(ref _selectedMaterial, value))
            {
                LoadSelectedMaterialIntoForm();
            }
        }
    }

    public string MaterialName
    {
        get => _materialName;
        set => SetProperty(ref _materialName, value);
    }

    public string MaterialType
    {
        get => _materialType;
        set => SetProperty(ref _materialType, value);
    }

    public string MaterialUnit
    {
        get => _materialUnit;
        set => SetProperty(ref _materialUnit, value);
    }

    public string MaterialPriceText
    {
        get => _materialPriceText;
        set => SetProperty(ref _materialPriceText, value);
    }

    public string MaterialManufacturer
    {
        get => _materialManufacturer;
        set => SetProperty(ref _materialManufacturer, value);
    }

    public string MaterialSupplier
    {
        get => _materialSupplier;
        set => SetProperty(ref _materialSupplier, value);
    }

    public string MaterialNote
    {
        get => _materialNote;
        set => SetProperty(ref _materialNote, value);
    }

    public IRelayCommand AddTypeCommand { get; }

    public IRelayCommand RenameTypeCommand { get; }

    public IRelayCommand DeleteTypeCommand { get; }

    public IRelayCommand AddMaterialCommand { get; }

    public IRelayCommand UpdateMaterialCommand { get; }

    public IRelayCommand DeleteMaterialCommand { get; }

    public IRelayCommand ClearMaterialFormCommand { get; }

    private void OnAppStateDataChanged(object? sender, EventArgs e)
    {
        LoadFromAppState();
    }

    private void LoadFromAppState()
    {
        var currentData = _appState.CurrentData;

        MaterialTypes.Clear();
        foreach (var type in currentData.MaterialTypes)
        {
            MaterialTypes.Add(type);
        }

        Materials.Clear();
        foreach (var material in currentData.Materials)
        {
            Materials.Add(new MaterialListItemViewModel(material));
        }

        ApplyPendingSelections();

        if (string.IsNullOrWhiteSpace(MaterialType) && MaterialTypes.Count > 0)
        {
            MaterialType = MaterialTypes[0];
        }
    }

    private void ApplyPendingSelections()
    {
        var selectedTypeName = _pendingSelectedTypeName ?? SelectedMaterialType;
        if (!string.IsNullOrWhiteSpace(selectedTypeName))
        {
            SelectedMaterialType = MaterialTypes.FirstOrDefault(t => t == selectedTypeName);
        }
        else if (MaterialTypes.Count > 0)
        {
            SelectedMaterialType = MaterialTypes[0];
        }
        else
        {
            SelectedMaterialType = null;
        }

        var selectedMaterialName = _pendingSelectedMaterialName ?? SelectedMaterial?.Name;
        if (!string.IsNullOrWhiteSpace(selectedMaterialName))
        {
            SelectedMaterial = Materials.FirstOrDefault(m => m.Name == selectedMaterialName);
        }
        else
        {
            SelectedMaterial = null;
        }

        _pendingSelectedTypeName = null;
        _pendingSelectedMaterialName = null;
    }

    private void AddType()
    {
        var newTypeName = TypeEditorText.Trim();

        if (string.IsNullOrWhiteSpace(newTypeName))
        {
            StatusMessage = "Bitte einen Materialtyp eingeben.";
            return;
        }

        if (_appState.CurrentData.MaterialTypes.Any(t => string.Equals(t, newTypeName, StringComparison.OrdinalIgnoreCase)))
        {
            StatusMessage = $"Der Materialtyp '{newTypeName}' existiert bereits.";
            return;
        }

        _appState.CurrentData.MaterialTypes.Add(newTypeName);
        _pendingSelectedTypeName = newTypeName;
        _appState.Save();

        TypeEditorText = string.Empty;
        if (string.IsNullOrWhiteSpace(MaterialType))
        {
            MaterialType = newTypeName;
        }

        StatusMessage = $"Materialtyp '{newTypeName}' wurde hinzugefügt.";
    }

    private void RenameType()
    {
        if (string.IsNullOrWhiteSpace(SelectedMaterialType))
        {
            StatusMessage = "Bitte zuerst einen Materialtyp auswählen.";
            return;
        }

        var newTypeName = TypeEditorText.Trim();
        if (string.IsNullOrWhiteSpace(newTypeName))
        {
            StatusMessage = "Bitte einen neuen Namen für den Materialtyp eingeben.";
            return;
        }

        if (!string.Equals(SelectedMaterialType, newTypeName, StringComparison.OrdinalIgnoreCase) &&
            _appState.CurrentData.MaterialTypes.Any(t => string.Equals(t, newTypeName, StringComparison.OrdinalIgnoreCase)))
        {
            StatusMessage = $"Der Materialtyp '{newTypeName}' existiert bereits.";
            return;
        }

        var oldTypeName = SelectedMaterialType;
        var typeIndex = _appState.CurrentData.MaterialTypes.FindIndex(t => t == oldTypeName);
        if (typeIndex < 0)
        {
            StatusMessage = "Der ausgewählte Materialtyp konnte nicht gefunden werden.";
            return;
        }

        _appState.CurrentData.MaterialTypes[typeIndex] = newTypeName;

        foreach (var material in _appState.CurrentData.Materials)
        {
            if (material.Type == oldTypeName)
            {
                material.Type = newTypeName;
            }
        }

        if (MaterialType == oldTypeName)
        {
            MaterialType = newTypeName;
        }

        _pendingSelectedTypeName = newTypeName;
        _pendingSelectedMaterialName = SelectedMaterial?.Name;
        _appState.Save();

        StatusMessage = $"Materialtyp '{oldTypeName}' wurde in '{newTypeName}' umbenannt.";
    }

    private void DeleteType()
    {
        if (string.IsNullOrWhiteSpace(SelectedMaterialType))
        {
            StatusMessage = "Bitte zuerst einen Materialtyp auswählen.";
            return;
        }

        var typeName = SelectedMaterialType;
        var isUsedByMaterials = _appState.CurrentData.Materials.Any(m => m.Type == typeName);
        if (isUsedByMaterials)
        {
            var result = MessageBox.Show(
                $"Typ '{typeName}' wird bereits von Materialien verwendet. Trotzdem löschen?",
                "Materialtyp löschen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                StatusMessage = $"Löschen von '{typeName}' abgebrochen.";
                return;
            }
        }

        _appState.CurrentData.MaterialTypes.Remove(typeName);
        _pendingSelectedTypeName = _appState.CurrentData.MaterialTypes.FirstOrDefault();
        _appState.Save();

        if (MaterialType == typeName)
        {
            MaterialType = _pendingSelectedTypeName ?? string.Empty;
        }

        TypeEditorText = string.Empty;
        StatusMessage = $"Materialtyp '{typeName}' wurde gelöscht.";
    }

    private void AddMaterial()
    {
        if (!TryBuildMaterial(out var material, out var errorMessage))
        {
            StatusMessage = errorMessage;
            return;
        }

        if (_appState.CurrentData.Materials.Any(m => string.Equals(m.Name, material.Name, StringComparison.OrdinalIgnoreCase)))
        {
            StatusMessage = $"Das Material '{material.Name}' existiert bereits.";
            return;
        }

        _appState.CurrentData.Materials.Add(material);
        _pendingSelectedMaterialName = null;
        _appState.Save();

        SelectedMaterial = null;
        ClearMaterialFormFields();
        StatusMessage = $"Material '{material.Name}' wurde hinzugefügt.";
    }

    private void UpdateMaterial()
    {
        if (SelectedMaterial is null)
        {
            StatusMessage = "Bitte zuerst ein Material in der Liste auswählen.";
            return;
        }

        if (!TryBuildMaterial(out var updatedMaterial, out var errorMessage))
        {
            StatusMessage = errorMessage;
            return;
        }

        var materials = _appState.CurrentData.Materials;
        var currentIndex = materials.FindIndex(m => m.Name == SelectedMaterial.Name);
        if (currentIndex < 0)
        {
            StatusMessage = "Das ausgewählte Material konnte nicht gefunden werden.";
            return;
        }

        var nameExistsElsewhere = materials
            .Where((_, index) => index != currentIndex)
            .Any(m => string.Equals(m.Name, updatedMaterial.Name, StringComparison.OrdinalIgnoreCase));

        if (nameExistsElsewhere)
        {
            StatusMessage = $"Ein anderes Material mit dem Namen '{updatedMaterial.Name}' existiert bereits.";
            return;
        }

        materials[currentIndex] = updatedMaterial;
        _pendingSelectedMaterialName = updatedMaterial.Name;
        _appState.Save();

        StatusMessage = $"Material '{updatedMaterial.Name}' wurde gespeichert.";
    }

    private void DeleteMaterial()
    {
        if (SelectedMaterial is null)
        {
            StatusMessage = "Bitte zuerst ein Material in der Liste auswählen.";
            return;
        }

        var materialName = SelectedMaterial.Name;
        var result = MessageBox.Show(
            $"Material '{materialName}' wirklich löschen?",
            "Material löschen",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
        {
            StatusMessage = $"Löschen von '{materialName}' abgebrochen.";
            return;
        }

        var removedCount = _appState.CurrentData.Materials.RemoveAll(m => m.Name == materialName);
        if (removedCount == 0)
        {
            StatusMessage = "Das ausgewählte Material konnte nicht gelöscht werden.";
            return;
        }

        SelectedMaterial = null;
        ClearMaterialFormFields();
        _appState.Save();

        StatusMessage = $"Material '{materialName}' wurde gelöscht.";
    }

    private void ClearMaterialForm()
    {
        SelectedMaterial = null;
        ClearMaterialFormFields();
        StatusMessage = "Formular wurde geleert.";
    }

    private void LoadSelectedMaterialIntoForm()
    {
        if (SelectedMaterial is null)
        {
            return;
        }

        MaterialName = SelectedMaterial.Name;
        MaterialType = SelectedMaterial.Type;
        MaterialUnit = SelectedMaterial.Unit;
        MaterialPriceText = SelectedMaterial.PriceText;
        MaterialManufacturer = SelectedMaterial.Manufacturer;
        MaterialSupplier = SelectedMaterial.Supplier;
        MaterialNote = SelectedMaterial.Note;
    }

    private void ClearMaterialFormFields()
    {
        MaterialName = string.Empty;
        MaterialPriceText = string.Empty;
        MaterialManufacturer = string.Empty;
        MaterialSupplier = string.Empty;
        MaterialNote = string.Empty;
        MaterialUnit = UnitSuggestions.FirstOrDefault() ?? "g";
        MaterialType = MaterialTypes.FirstOrDefault() ?? string.Empty;
    }

    private bool TryBuildMaterial(out Material material, out string errorMessage)
    {
        material = new Material();
        errorMessage = string.Empty;

        var materialName = MaterialName.Trim();
        if (string.IsNullOrWhiteSpace(materialName))
        {
            errorMessage = "Bitte einen Materialnamen eingeben.";
            return false;
        }

        if (!TryParseDecimal(MaterialPriceText, out var price))
        {
            errorMessage = "Preis muss eine gültige Zahl sein.";
            return false;
        }

        material = new Material
        {
            Name = materialName,
            Type = MaterialType.Trim(),
            Unit = string.IsNullOrWhiteSpace(MaterialUnit) ? "g" : MaterialUnit.Trim(),
            Price = price,
            Manufacturer = MaterialManufacturer.Trim(),
            Supplier = MaterialSupplier.Trim(),
            Note = MaterialNote.Trim()
        };

        return true;
    }

    private static bool TryParseDecimal(string text, out decimal value)
    {
        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out value) ||
               decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }
}

public sealed class MaterialListItemViewModel
{
    public MaterialListItemViewModel(Material material)
    {
        ArgumentNullException.ThrowIfNull(material);

        Name = material.Name;
        Type = material.Type;
        Unit = material.Unit;
        Price = material.Price;
        Manufacturer = material.Manufacturer;
        Supplier = material.Supplier;
        Note = material.Note;
    }

    public string Name { get; }

    public string Type { get; }

    public string Unit { get; }

    public decimal Price { get; }

    public string PriceText => Price.ToString("0.##", CultureInfo.CurrentCulture);

    public string Manufacturer { get; }

    public string Supplier { get; }

    public string Note { get; }
}
