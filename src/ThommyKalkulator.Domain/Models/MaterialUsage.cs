using System.Text.Json.Serialization;

namespace ThommyKalkulator.Domain.Models;

public sealed class MaterialUsage
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("menge")]
    public decimal Quantity { get; set; }

    [JsonPropertyName("einheit")]
    public string Unit { get; set; } = string.Empty;

    [JsonPropertyName("preis")]
    public decimal Price { get; set; }

    [JsonPropertyName("kosten")]
    public decimal Cost { get; set; }
}