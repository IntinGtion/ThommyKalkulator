using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThommyKalkulator.Application.Interfaces;
using ThommyKalkulator.Domain.Models;

namespace ThommyKalkulator.WPF.ViewModels.Pages;

public sealed class ProjectsViewModel : ObservableObject
{
    private readonly IAppState _appState;
    private readonly ICalculationService _calculationService;

    private string _pageTitle = "Kalkulationen";
    private string _pageDescription = "Gespeicherte Kalkulationen können gesucht, geprüft, neu berechnet, gelöscht und zur Bearbeitung geladen werden.";
    private string _searchText = string.Empty;
    private string _statusMessage = "Bereit";
    private string _detailsText = "Kalkulation auswählen …";
    private ProjectListItemViewModel? _selectedProject;

    public ProjectsViewModel(IAppState appState, ICalculationService calculationService)
    {
        _appState = appState ?? throw new ArgumentNullException(nameof(appState));
        _calculationService = calculationService ?? throw new ArgumentNullException(nameof(calculationService));

        Projects = new ObservableCollection<ProjectListItemViewModel>();

        RefreshCommand = new RelayCommand(RefreshProjects);
        LoadProjectCommand = new RelayCommand(LoadSelectedProject);
        DeleteProjectCommand = new RelayCommand(DeleteSelectedProject);
        RecalculateProjectCommand = new RelayCommand(RecalculateSelectedProject);

        _appState.DataChanged += OnAppStateDataChanged;
        RefreshProjects();
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

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                RefreshProjects();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string DetailsText
    {
        get => _detailsText;
        set => SetProperty(ref _detailsText, value);
    }

    public ObservableCollection<ProjectListItemViewModel> Projects { get; }

    public ProjectListItemViewModel? SelectedProject
    {
        get => _selectedProject;
        set
        {
            if (SetProperty(ref _selectedProject, value))
            {
                UpdateDetails();
            }
        }
    }

    public IRelayCommand RefreshCommand { get; }

    public IRelayCommand LoadProjectCommand { get; }

    public IRelayCommand DeleteProjectCommand { get; }

    public IRelayCommand RecalculateProjectCommand { get; }

    public void LoadSelectedProject()
    {
        if (SelectedProject is null)
        {
            StatusMessage = "Bitte zuerst eine Kalkulation auswählen.";
            return;
        }

        App.RequestProjectEdit(SelectedProject.ProjectIndex, 0);
        StatusMessage = "Kalkulation wurde in den Tab 'Kalkulation' geladen.";
    }

    private void OnAppStateDataChanged(object? sender, EventArgs e)
    {
        RefreshProjects();
    }

    private void RefreshProjects()
    {
        var previouslySelectedIndex = SelectedProject?.ProjectIndex;
        var filter = (SearchText ?? string.Empty).Trim();

        Projects.Clear();

        for (var index = 0; index < _appState.CurrentData.Projects.Count; index++)
        {
            var project = _appState.CurrentData.Projects[index];
            if (!MatchesFilter(project, filter))
            {
                continue;
            }

            Projects.Add(new ProjectListItemViewModel(index, project, GetCurrency()));
        }

        if (Projects.Count == 0)
        {
            SelectedProject = null;
            DetailsText = "Keine gespeicherten Kalkulationen gefunden.";
            StatusMessage = string.IsNullOrWhiteSpace(filter)
                ? "Keine Kalkulationen gespeichert."
                : "Keine Kalkulationen zum Suchbegriff gefunden.";
            return;
        }

        SelectedProject = previouslySelectedIndex.HasValue
            ? Projects.FirstOrDefault(item => item.ProjectIndex == previouslySelectedIndex.Value) ?? Projects[0]
            : Projects[0];

        StatusMessage = $"{Projects.Count} Kalkulation(en) angezeigt.";
    }

    private void UpdateDetails()
    {
        if (SelectedProject is null)
        {
            DetailsText = "Kalkulation auswählen …";
            return;
        }

        var project = SelectedProject.Project;
        var lines = new List<string>
        {
            $"Projekt: {project.Name}   |   {project.Date}"
        };

        if (project.MachineUsages.Count > 0)
        {
            lines.Add("Geräte:");
            foreach (var machineUsage in project.MachineUsages)
            {
                lines.Add(
                    "    " +
                    machineUsage.Name.PadRight(20) +
                    FormatDecimal(machineUsage.RuntimeHours) + "h  →  Strom " +
                    FormatMoney(machineUsage.PowerCost) +
                    "  Verschleiß " +
                    FormatMoney(machineUsage.WearCost));
            }
        }

        if (project.MaterialUsages.Count > 0)
        {
            lines.Add("Materialien:");
            foreach (var materialUsage in project.MaterialUsages)
            {
                lines.Add(
                    "    " +
                    materialUsage.Name.PadRight(20) +
                    FormatDecimal(materialUsage.Quantity) + " " + materialUsage.Unit + "  →  " +
                    FormatMoney(materialUsage.Cost));
            }
        }

        lines.Add("Endpreis: " + FormatMoney(project.FinalPrice) + "   Aufschlag: " + FormatDecimal(project.SurchargePercent) + "%");
        if (!string.IsNullOrWhiteSpace(project.Note))
        {
            lines.Add("Notiz: " + project.Note.Trim());
        }

        DetailsText = string.Join(Environment.NewLine, lines);
    }

    private void DeleteSelectedProject()
    {
        if (SelectedProject is null)
        {
            StatusMessage = "Bitte zuerst eine Kalkulation auswählen.";
            return;
        }

        var deletedName = SelectedProject.Project.Name;
        _appState.CurrentData.Projects.RemoveAt(SelectedProject.ProjectIndex);
        _appState.Save();
        StatusMessage = $"'{deletedName}' wurde gelöscht.";
    }

    private void RecalculateSelectedProject()
    {
        if (SelectedProject is null)
        {
            StatusMessage = "Bitte zuerst eine Kalkulation auswählen.";
            return;
        }

        var projectIndex = SelectedProject.ProjectIndex;
        var sourceProject = SelectedProject.Project;
        var settings = _appState.CurrentData.GlobalSettings;

        var recalculationInput = new CalculationProject
        {
            ProjectNumber = sourceProject.ProjectNumber,
            Name = sourceProject.Name,
            Date = sourceProject.Date,
            PreparationHours = sourceProject.PreparationHours,
            PostProcessingHours = sourceProject.PostProcessingHours,
            ConstructionHours = sourceProject.ConstructionHours,
            TimeUnit = sourceProject.TimeUnit,
            SurchargePercent = sourceProject.SurchargePercent,
            Note = sourceProject.Note,
            Quantity = sourceProject.Quantity,
            FreeCostItems = sourceProject.FreeCostItems
                .Select(item => new FreeCostItem
                {
                    Description = item.Description,
                    Amount = item.Amount
                })
                .ToList(),
            MachineUsages = BuildUpdatedMachineUsages(sourceProject),
            MaterialUsages = BuildUpdatedMaterialUsages(sourceProject)
        };

        var recalculatedProject = _calculationService.Calculate(recalculationInput, settings);
        recalculatedProject.Date = DateTime.Now.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);

        _appState.CurrentData.Projects[projectIndex] = recalculatedProject;
        _appState.Save();

        StatusMessage = "Kalkulation wurde mit den aktuellen Stammdaten neu berechnet.";
    }

    private List<MachineUsage> BuildUpdatedMachineUsages(CalculationProject sourceProject)
    {
        var result = new List<MachineUsage>();

        foreach (var usage in sourceProject.MachineUsages)
        {
            var machine = _appState.CurrentData.Machines.FirstOrDefault(item => string.Equals(item.Name, usage.Name, StringComparison.OrdinalIgnoreCase));
            result.Add(new MachineUsage
            {
                Name = usage.Name,
                RuntimeHours = usage.RuntimeHours,
                Watt = machine?.Watt ?? usage.Watt,
                PurchasePrice = machine?.PurchasePrice ?? usage.PurchasePrice,
                LifetimeHours = machine?.LifetimeHours ?? usage.LifetimeHours
            });
        }

        return result;
    }

    private List<MaterialUsage> BuildUpdatedMaterialUsages(CalculationProject sourceProject)
    {
        var result = new List<MaterialUsage>();

        foreach (var usage in sourceProject.MaterialUsages)
        {
            var material = _appState.CurrentData.Materials.FirstOrDefault(item => string.Equals(item.Name, usage.Name, StringComparison.OrdinalIgnoreCase));
            result.Add(new MaterialUsage
            {
                Name = usage.Name,
                Quantity = usage.Quantity,
                Unit = material?.Unit ?? usage.Unit,
                Price = material?.Price ?? usage.Price
            });
        }

        return result;
    }

    private bool MatchesFilter(CalculationProject project, string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return true;
        }

        return project.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || project.ProjectNumber.Contains(filter, StringComparison.OrdinalIgnoreCase);
    }

    private string GetCurrency()
    {
        return string.IsNullOrWhiteSpace(_appState.CurrentData.GlobalSettings.Currency)
            ? "€"
            : _appState.CurrentData.GlobalSettings.Currency;
    }

    private string FormatMoney(decimal value)
    {
        return value.ToString("N2", CultureInfo.CurrentCulture) + " " + GetCurrency();
    }

    private static string FormatDecimal(decimal value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }
}

public sealed class ProjectListItemViewModel
{
    public ProjectListItemViewModel(int projectIndex, CalculationProject project, string currency)
    {
        ProjectIndex = projectIndex;
        Project = project ?? throw new ArgumentNullException(nameof(project));
        ProjectNumber = string.IsNullOrWhiteSpace(project.ProjectNumber) ? "–" : project.ProjectNumber;
        Name = project.Name;
        Date = project.Date;
        MachinesSummary = project.MachineUsages.Count == 0
            ? "–"
            : string.Join(", ", project.MachineUsages.Select(item => item.Name + " (" + FormatDecimal(item.RuntimeHours) + "h)"));
        MaterialsSummary = project.MaterialUsages.Count == 0
            ? "–"
            : string.Join(", ", project.MaterialUsages.Select(item => item.Name + " (" + FormatDecimal(item.Quantity) + " " + item.Unit + ")"));
        CostPriceText = FormatMoney(project.CostPrice, currency);

        if (project.Quantity <= 1)
        {
            EndPriceText = FormatMoney(project.FinalPrice, currency);
        }
        else
        {
            EndPriceText = FormatMoney(project.FinalPrice, currency) + " × " + project.Quantity;
        }
    }

    public int ProjectIndex { get; }

    public CalculationProject Project { get; }

    public string ProjectNumber { get; }

    public string Name { get; }

    public string Date { get; }

    public string MachinesSummary { get; }

    public string MaterialsSummary { get; }

    public string CostPriceText { get; }

    public string EndPriceText { get; }

    private static string FormatMoney(decimal value, string currency)
    {
        return value.ToString("N2", CultureInfo.CurrentCulture) + " " + currency;
    }

    private static string FormatDecimal(decimal value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
