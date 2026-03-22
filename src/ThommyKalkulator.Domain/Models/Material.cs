using System.Text.Json.Serialization;

namespace ThommyKalkulator.Domain.Models;

public sealed class Material
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("typ")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("einheit")]
    public string Unit { get; set; } = string.Empty;

    [JsonPropertyName("preis")]
    public decimal Price { get; set; }

    [JsonPropertyName("hersteller")]
    public string Manufacturer { get; set; } = string.Empty;

    [JsonPropertyName("lieferant")]
    public string Supplier { get; set; } = string.Empty;

    [JsonPropertyName("notiz")]
    public string Note { get; set; } = string.Empty;
}