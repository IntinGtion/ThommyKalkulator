using ThommyKalkulator.Domain.Models;

namespace ThommyKalkulator.Application.Interfaces;

public interface ICsvExportService
{
    void ExportProjects(string filePath, IReadOnlyList<CalculationProject> projects);
}
