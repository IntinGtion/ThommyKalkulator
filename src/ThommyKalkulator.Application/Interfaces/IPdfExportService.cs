using ThommyKalkulator.Domain.Models;

namespace ThommyKalkulator.Application.Interfaces;

public interface IPdfExportService
{
    void ExportProjects(string filePath, IReadOnlyList<CalculationProject> projects, GlobalSettings settings);
}
