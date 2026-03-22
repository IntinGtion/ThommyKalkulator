using System.Text.Json.Serialization;

namespace ThommyKalkulator.Domain.Models;

public sealed class CalculationProject
{
    [JsonPropertyName("projekt_nr")]
    public string ProjectNumber { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("datum")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("vorbereitung_std")]
    public decimal PreparationHours { get; set; }

    [JsonPropertyName("nachbearbeitung_std")]
    public decimal PostProcessingHours { get; set; }

    [JsonPropertyName("konstruktion_std")]
    public decimal ConstructionHours { get; set; }

    [JsonPropertyName("time_unit")]
    public string TimeUnit { get; set; } = "Stunden";

    [JsonPropertyName("aufschlag")]
    public decimal SurchargePercent { get; set; }

    [JsonPropertyName("zusatz")]
    public decimal AdditionalCostTotal { get; set; }

    [JsonPropertyName("freie_positionen")]
    public List<FreeCostItem> FreeCostItems { get; set; } = new();

    [JsonPropertyName("notiz")]
    public string Note { get; set; } = string.Empty;

    [JsonPropertyName("stueckzahl")]
    public int Quantity { get; set; } = 1;

    [JsonPropertyName("maschinen_einsatz")]
    public List<MachineUsage> MachineUsages { get; set; } = new();

    [JsonPropertyName("material_einsatz")]
    public List<MaterialUsage> MaterialUsages { get; set; } = new();

    [JsonPropertyName("k_strom")]
    public decimal PowerCost { get; set; }

    [JsonPropertyName("k_verschleiss")]
    public decimal WearCost { get; set; }

    [JsonPropertyName("k_material")]
    public decimal MaterialCost { get; set; }

    [JsonPropertyName("k_vorbereitung")]
    public decimal PreparationCost { get; set; }

    [JsonPropertyName("k_arbeit")]
    public decimal LaborCost { get; set; }

    [JsonPropertyName("k_konstruktion")]
    public decimal ConstructionCost { get; set; }

    [JsonPropertyName("selbstkosten")]
    public decimal CostPrice { get; set; }

    [JsonPropertyName("endpreis")]
    public decimal FinalPrice { get; set; }
}