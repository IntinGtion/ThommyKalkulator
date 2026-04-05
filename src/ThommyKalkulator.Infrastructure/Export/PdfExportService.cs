using System.Diagnostics;
using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ThommyKalkulator.Application.Interfaces;
using ThommyKalkulator.Domain.Models;

namespace ThommyKalkulator.Infrastructure.Export;

public sealed class PdfExportService : IPdfExportService
{
    public void ExportProjects(string filePath, IReadOnlyList<CalculationProject> projects, GlobalSettings settings)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(projects);
        ArgumentNullException.ThrowIfNull(settings);

        if (projects.Count == 0)
        {
            throw new InvalidOperationException("Es wurden keine Kalkulationen zum PDF-Export übergeben.");
        }

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Bei Bedarf auf Professional oder Enterprise anpassen, falls die Community-Lizenz für euren Einsatz nicht passt.
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            if (projects.Count > 1)
            {
                container.Page(page =>
                {
                    ConfigurePage(page);
                    page.Header().Element(header => ComposeHeader(header, settings, "Kalkulations-Zusammenfassung"));
                    page.Content().Element(content => ComposeSummaryPage(content, projects, settings));
                    page.Footer().Element(footer => ComposeFooter(footer));
                });
            }

            foreach (var project in projects)
            {
                container.Page(page =>
                {
                    ConfigurePage(page);
                    page.Header().Element(header => ComposeHeader(header, settings, BuildProjectTitle(project)));
                    page.Content().Element(content => ComposeProjectPage(content, project, settings));
                    page.Footer().Element(footer => ComposeFooter(footer));
                });
            }
        });

        document.GeneratePdf(filePath);

        if (settings.PdfAutoOpen)
        {
            TryOpen(filePath);
        }
    }

    private static void ConfigurePage(PageDescriptor page)
    {
        page.Size(PageSizes.A4);
        page.Margin(2, Unit.Centimetre);
        page.DefaultTextStyle(x => x.FontSize(10));
        page.PageColor(Colors.White);
    }

    private static void ComposeHeader(IContainer container, GlobalSettings settings, string title)
    {
        container.Column(column =>
        {
            column.Spacing(8);
            column.Item().Row(row =>
            {
                var logoBytes = TryReadLogo(settings.LogoPath);
                if (logoBytes is not null)
                {
                    row.ConstantItem(120)
                        .Height(50)
                        .Image(logoBytes)
                        .FitArea();
                }
                else
                {
                    row.ConstantItem(120).Height(1);
                }

                row.RelativeItem().AlignRight().Column(titleColumn =>
                {
                    titleColumn.Item().AlignRight().Text(title).FontSize(18).SemiBold();
                    titleColumn.Item().AlignRight().Text(DateTime.Now.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)).FontColor(Colors.Grey.Darken1);
                });
            });

            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
        });
    }

    private static void ComposeSummaryPage(IContainer container, IReadOnlyList<CalculationProject> projects, GlobalSettings settings)
    {
        container.Column(column =>
        {
            column.Spacing(12);
            column.Item().Text("Positionen").FontSize(13).SemiBold();
            column.Item().Element(x => ComposeSummaryTable(x, projects, settings));
        });
    }

    private static void ComposeSummaryTable(IContainer container, IReadOnlyList<CalculationProject> projects, GlobalSettings settings)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(90);
                columns.RelativeColumn(2.3f);
                columns.ConstantColumn(90);
                columns.ConstantColumn(55);
                columns.ConstantColumn(100);
            });

            table.Header(header =>
            {
                HeaderCell(header.Cell(), "Material");
                HeaderCell(header.Cell(), "Menge");
                HeaderCell(header.Cell(), "Preis / Einheit");
                HeaderCell(header.Cell(), "Gesamtpreis");
            });

            decimal total = 0m;
            foreach (var project in projects)
            {
                var quantity = Math.Max(1, project.Quantity);
                var totalPrice = project.FinalPrice * quantity;
                total += totalPrice;

                BodyCell(table, string.IsNullOrWhiteSpace(project.ProjectNumber) ? "–" : project.ProjectNumber);
                BodyCell(table, project.Name);
                BodyCell(table, FormatMoney(project.FinalPrice, settings), true);
                BodyCell(table, quantity.ToString(CultureInfo.InvariantCulture), true);
                BodyCell(table, FormatMoney(totalPrice, settings), true);
            }

            FooterLabelCell(table, "Gesamtsumme", 4);
            FooterValueCell(table, FormatMoney(total, settings));
        });
    }

    private static void ComposeProjectPage(IContainer container, CalculationProject project, GlobalSettings settings)
    {
        container.Column(column =>
        {
            column.Spacing(12);

            column.Item().Text($"Datum: {project.Date}");

            if (project.MachineUsages.Count > 0)
            {
                column.Item().Text("Maschineneinsatz").FontSize(13).SemiBold();
                column.Item().Element(x => ComposeMachinesTable(x, project, settings));
            }

            if (project.MaterialUsages.Count > 0)
            {
                column.Item().Text("Materialeinsatz").FontSize(13).SemiBold();
                column.Item().Element(x => ComposeMaterialsTable(x, project, settings));
            }

            column.Item().Text("Kostenaufstellung").FontSize(13).SemiBold();
            column.Item().Element(x => ComposeCostTable(x, project, settings));

            if (!string.IsNullOrWhiteSpace(project.Note))
            {
                column.Item().Text("Notiz").FontSize(13).SemiBold();
                column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text(project.Note);
            }
        });
    }

    private static void ComposeMachinesTable(IContainer container, CalculationProject project, GlobalSettings settings)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2f);
                columns.ConstantColumn(80);
                columns.ConstantColumn(100);
                columns.ConstantColumn(110);
            });

            table.Header(header =>
            {
                HeaderCell(header.Cell(), "Gerät");
                HeaderCell(header.Cell(), "Laufzeit");
                HeaderCell(header.Cell(), "Strom");
                HeaderCell(header.Cell(), "Verschleiß");
            });

            foreach (var machine in project.MachineUsages)
            {
                BodyCell(table, machine.Name);
                BodyCell(table, FormatHours(machine.RuntimeHours), true);
                BodyCell(table, FormatMoney(machine.PowerCost, settings), true);
                BodyCell(table, FormatMoney(machine.WearCost, settings), true);
            }
        });
    }

    private static void ComposeMaterialsTable(IContainer container, CalculationProject project, GlobalSettings settings)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2f);
                columns.ConstantColumn(90);
                columns.ConstantColumn(110);
                columns.ConstantColumn(110);
            });

            table.Header(header =>
            {
                HeaderCell(header.Cell(), "Projekt-Nr.");
                HeaderCell(header.Cell(), "Positionsbezeichnung");
                HeaderCell(header.Cell(), "Einzelpreis");
                HeaderCell(header.Cell(), "Stück");
                HeaderCell(header.Cell(), "Gesamtpreis");
            });

            foreach (var material in project.MaterialUsages)
            {
                BodyCell(table, material.Name);
                BodyCell(table, FormatQuantity(material.Quantity, material.Unit), true);
                BodyCell(table, FormatMoney(material.Price, settings), true);
                BodyCell(table, FormatMoney(material.Cost, settings), true);
            }
        });
    }

    private static void ComposeCostTable(IContainer container, CalculationProject project, GlobalSettings settings)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2.8f);
                columns.ConstantColumn(120);
            });

            table.Header(header =>
            {
                HeaderCell(header.Cell(), "Position");
                HeaderCell(header.Cell(), "Kosten");
            });

            BodyCell(table, "Strom gesamt");
            BodyCell(table, FormatMoney(project.PowerCost, settings), true);

            BodyCell(table, "Maschinenverschleiß gesamt");
            BodyCell(table, FormatMoney(project.WearCost, settings), true);

            BodyCell(table, "Materialkosten gesamt");
            BodyCell(table, FormatMoney(project.MaterialCost, settings), true);

            BodyCell(table, $"Vorbereitung ({FormatHours(project.PreparationHours)})");
            BodyCell(table, FormatMoney(project.PreparationCost, settings), true);

            BodyCell(table, $"Nachbearbeitung ({FormatHours(project.PostProcessingHours)})");
            BodyCell(table, FormatMoney(project.LaborCost, settings), true);

            BodyCell(table, $"CAD/RE ({FormatHours(project.ConstructionHours)})");
            BodyCell(table, FormatMoney(project.ConstructionCost, settings), true);

            foreach (var freeCost in project.FreeCostItems)
            {
                BodyCell(table, freeCost.Description);
                BodyCell(table, FormatMoney(freeCost.Amount, settings), true);
            }

            SummaryLabelCell(table, "Selbstkosten");
            SummaryValueCell(table, FormatMoney(project.CostPrice, settings));

            SummaryLabelCell(table, $"Aufschlag ({FormatDecimal(project.SurchargePercent)} %)");
            SummaryValueCell(table, FormatMoney(project.FinalPrice - project.CostPrice, settings));

            FinalLabelCell(table, "Einzelpreis");
            FinalValueCell(table, FormatMoney(project.FinalPrice, settings));

            if (project.Quantity > 1)
            {
                SummaryLabelCell(table, "Stückzahl");
                SummaryValueCell(table, project.Quantity.ToString(CultureInfo.InvariantCulture) + "x");

                FinalLabelCell(table, $"Gesamtpreis ({project.Quantity}x)");
                FinalValueCell(table, FormatMoney(project.FinalPrice * project.Quantity, settings));
            }
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            column.Item()
                .PaddingTop(6)
                .AlignCenter()
                .DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Darken1))
                .Text(text =>
                {
                    text.Span("Erstellt ");
                    text.Span(DateTime.Now.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture));
                    text.Span(" · Thommy Kalkulator · Seite ");
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
        });
    }

    private static void HeaderCell(IContainer cell, string text)
    {
        cell.Element(HeaderCellStyle).Text(text).SemiBold().FontColor(Colors.White);
    }

    private static void BodyCell(TableDescriptor table, string text, bool rightAligned = false)
    {
        table.Cell().Element(BodyCellStyle).AlignRightIf(rightAligned).Text(text);
    }

    private static void FooterLabelCell(TableDescriptor table, string text, int columnSpan)
    {
        table.Cell().ColumnSpan((uint)columnSpan).Element(SummaryLabelCellStyle).Text(text).SemiBold();
    }

    private static void FooterValueCell(TableDescriptor table, string text)
    {
        table.Cell().Element(FinalValueCellStyle).Text(text).SemiBold().FontColor(Colors.White);
    }

    private static void SummaryLabelCell(TableDescriptor table, string text)
    {
        table.Cell().Element(SummaryLabelCellStyle).Text(text).SemiBold();
    }

    private static void SummaryValueCell(TableDescriptor table, string text)
    {
        table.Cell().Element(SummaryValueCellStyle).AlignRight().Text(text).SemiBold();
    }

    private static void FinalLabelCell(TableDescriptor table, string text)
    {
        table.Cell().Element(FinalLabelCellStyle).Text(text).SemiBold().FontColor(Colors.White);
    }

    private static void FinalValueCell(TableDescriptor table, string text)
    {
        table.Cell().Element(FinalValueCellStyle).AlignRight().Text(text).SemiBold().FontColor(Colors.White);
    }

    private static IContainer HeaderCellStyle(IContainer container)
    {
        return container
            .Background(Colors.Blue.Darken3)
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(6)
            .PaddingHorizontal(8);
    }

    private static IContainer BodyCellStyle(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(5)
            .PaddingHorizontal(8);
    }

    private static IContainer SummaryLabelCellStyle(IContainer container)
    {
        return container
            .Background(Colors.Grey.Lighten3)
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(6)
            .PaddingHorizontal(8);
    }

    private static IContainer SummaryValueCellStyle(IContainer container)
    {
        return container
            .Background(Colors.Grey.Lighten3)
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(6)
            .PaddingHorizontal(8);
    }

    private static IContainer FinalLabelCellStyle(IContainer container)
    {
        return container
            .Background(Colors.Green.Darken2)
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(6)
            .PaddingHorizontal(8);
    }

    private static IContainer FinalValueCellStyle(IContainer container)
    {
        return container
            .Background(Colors.Green.Darken2)
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(6)
            .PaddingHorizontal(8);
    }

    private static byte[]? TryReadLogo(string? logoPath)
    {
        if (string.IsNullOrWhiteSpace(logoPath) || !File.Exists(logoPath))
        {
            return null;
        }

        try
        {
            return File.ReadAllBytes(logoPath);
        }
        catch
        {
            return null;
        }
    }

    private static void TryOpen(string filePath)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
        }
        catch
        {
            // bewusst ignoriert
        }
    }

    private static string BuildProjectTitle(CalculationProject project)
    {
        var projectNumber = string.IsNullOrWhiteSpace(project.ProjectNumber) ? "–" : project.ProjectNumber;
        return $"Kalkulation für {projectNumber}, \"{project.Name}\"";
    }

    private static string FormatMoney(decimal value, GlobalSettings settings)
    {
        var currency = string.IsNullOrWhiteSpace(settings.Currency) ? "€" : settings.Currency;
        return value.ToString("N2", CultureInfo.GetCultureInfo("de-DE")) + " " + currency;
    }

    private static string FormatHours(decimal hours)
    {
        return FormatDecimal(hours) + " Std.";
    }

    private static string FormatQuantity(decimal quantity, string unit)
    {
        return FormatDecimal(quantity) + " " + unit;
    }

    private static string FormatDecimal(decimal value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }
}

internal static class QuestPdfContainerExtensions
{
    public static IContainer AlignRightIf(this IContainer container, bool rightAligned)
    {
        return rightAligned ? container.AlignRight() : container;
    }
}
