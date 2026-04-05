using System.Text.Json.Serialization;
using ThommyKalkulator.Domain.Models;
using ThommyKalkulator.WPF.Services;

namespace ThommyKalkulator.WPF.Models;

public sealed class AppBackupFile
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    [JsonPropertyName("exported_at")]
    public string ExportedAt { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    [JsonPropertyName("global_settings")]
    public GlobalSettings GlobalSettings { get; set; } = new();

    [JsonPropertyName("machines")]
    public List<Machine> Machines { get; set; } = new();

    [JsonPropertyName("mat_types")]
    public List<string> MaterialTypes { get; set; } = new();

    [JsonPropertyName("materials")]
    public List<Material> Materials { get; set; } = new();

    [JsonPropertyName("projects")]
    public List<CalculationProject> Projects { get; set; } = new();

    [JsonPropertyName("ui_configuration")]
    public AppConfiguration? UiConfiguration { get; set; }

    public static AppBackupFile From(AppData appData, AppConfiguration uiConfiguration)
    {
        ArgumentNullException.ThrowIfNull(appData);
        ArgumentNullException.ThrowIfNull(uiConfiguration);

        return new AppBackupFile
        {
            GlobalSettings = appData.GlobalSettings,
            Machines = appData.Machines,
            MaterialTypes = appData.MaterialTypes,
            Materials = appData.Materials,
            Projects = appData.Projects,
            UiConfiguration = AppConfigurationStore.Normalize(uiConfiguration)
        };
    }

    public AppData ToAppData()
    {
        return new AppData
        {
            GlobalSettings = GlobalSettings ?? new GlobalSettings(),
            Machines = Machines ?? new List<Machine>(),
            MaterialTypes = MaterialTypes ?? new List<string>(),
            Materials = Materials ?? new List<Material>(),
            Projects = Projects ?? new List<CalculationProject>()
        };
    }
}
