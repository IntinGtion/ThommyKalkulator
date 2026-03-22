using System.Globalization;
using System.Text;
using ThommyKalkulator.Application.Interfaces;
using ThommyKalkulator.Domain.Models;

namespace ThommyKalkulator.Infrastructure.Export;

public sealed class CsvExportService : ICsvExportService
{
    private static readonly string[] FieldNames =
    {
        "name",
        "datum",
        "projekt_nr",
        "vorbereitung_std",
        "nachbearbeitung_std",
        "konstruktion_std",
        "k_strom",
        "k_verschleiss",
        "k_material",
        "k_vorbereitung",
        "k_arbeit",
        "k_konstruktion",
        "zusatz",
        "selbstkosten",
        "aufschlag",
        "endpreis",
        "notiz"
    };

    public void ExportProjects(string filePath, IReadOnlyList<CalculationProject> projects)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(projects);

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var writer = new StreamWriter(filePath, false, new UTF8Encoding(true));
        writer.WriteLine(string.Join(";", FieldNames));

        foreach (var project in projects)
        {
            var values = new[]
            {
                Escape(project.Name),
                Escape(project.Date),
                Escape(project.ProjectNumber),
                Escape(FormatDecimal(project.PreparationHours)),
                Escape(FormatDecimal(project.PostProcessingHours)),
                Escape(FormatDecimal(project.ConstructionHours)),
                Escape(FormatDecimal(project.PowerCost)),
                Escape(FormatDecimal(project.WearCost)),
                Escape(FormatDecimal(project.MaterialCost)),
                Escape(FormatDecimal(project.PreparationCost)),
                Escape(FormatDecimal(project.LaborCost)),
                Escape(FormatDecimal(project.ConstructionCost)),
                Escape(FormatDecimal(project.AdditionalCostTotal)),
                Escape(FormatDecimal(project.CostPrice)),
                Escape(FormatDecimal(project.SurchargePercent)),
                Escape(FormatDecimal(project.FinalPrice)),
                Escape(project.Note)
            };

            writer.WriteLine(string.Join(";", values));
        }
    }

    private static string FormatDecimal(decimal value)
    {
        return value.ToString("0.####", CultureInfo.InvariantCulture);
    }

    private static string Escape(string? value)
    {
        var text = value ?? string.Empty;
        if (!text.Contains(';') && !text.Contains('"') && !text.Contains('\n') && !text.Contains('\r'))
        {
            return text;
        }

        return "\"" + text.Replace("\"", "\"\"") + "\"";
    }
}
