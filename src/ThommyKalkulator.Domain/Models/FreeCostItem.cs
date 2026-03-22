using System.Text.Json.Serialization;

namespace ThommyKalkulator.Domain.Models;

public sealed class FreeCostItem
{
    [JsonPropertyName("bezeichnung")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("betrag")]
    public decimal Amount { get; set; }
}