using System.Text.Json.Serialization;

namespace ThommyKalkulator.Domain.Models;

public sealed class AppData
{
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
}