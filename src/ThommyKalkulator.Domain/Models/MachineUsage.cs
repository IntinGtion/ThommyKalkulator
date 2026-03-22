using System.Text.Json.Serialization;

namespace ThommyKalkulator.Domain.Models;

public sealed class MachineUsage
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("laufzeit_std")]
    public decimal RuntimeHours { get; set; }

    [JsonPropertyName("watt")]
    public decimal Watt { get; set; }

    [JsonPropertyName("kaufpreis")]
    public decimal PurchasePrice { get; set; }

    [JsonPropertyName("lebensdauer_h")]
    public decimal LifetimeHours { get; set; }

    [JsonPropertyName("k_strom")]
    public decimal PowerCost { get; set; }

    [JsonPropertyName("k_verschleiss")]
    public decimal WearCost { get; set; }
}