using ThommyKalkulator.Domain.Models;

namespace ThommyKalkulator.Application.Interfaces;

public interface ICalculationService
{
    CalculationProject Calculate(CalculationProject project, GlobalSettings settings);
}
