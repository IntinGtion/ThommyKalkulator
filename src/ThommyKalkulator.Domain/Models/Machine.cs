using System.Text.Json.Serialization;

namespace ThommyKalkulator.Domain.Models;

public sealed class Machine
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("watt")]
    public decimal Watt { get; set; }

    [JsonPropertyName("kaufpreis")]
    public decimal PurchasePrice { get; set; }

    [JsonPropertyName("lebensdauer_h")]
    public decimal LifetimeHours { get; set; }
}