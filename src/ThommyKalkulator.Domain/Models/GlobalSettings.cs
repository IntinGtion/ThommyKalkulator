using System.Text.Json.Serialization;

namespace ThommyKalkulator.Domain.Models;

public sealed class GlobalSettings
{
    [JsonPropertyName("waehrung")]
    public string Currency { get; set; } = "€";

    [JsonPropertyName("strom_preis_kwh")]
    public decimal ElectricityPricePerKwh { get; set; }

    [JsonPropertyName("stundenlohn")]
    public decimal LaborRate { get; set; }

    [JsonPropertyName("konstruktion_stundenlohn")]
    public decimal ConstructionLaborRate { get; set; }

    [JsonPropertyName("standard_aufschlag")]
    public decimal DefaultSurchargePercent { get; set; }

    [JsonPropertyName("logo_path")]
    public string LogoPath { get; set; } = string.Empty;

    [JsonPropertyName("floating_live_default")]
    public bool FloatingLiveDefault { get; set; }

    [JsonPropertyName("pdf_auto_open")]
    public bool PdfAutoOpen { get; set; }
}