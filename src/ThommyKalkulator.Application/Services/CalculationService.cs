using ThommyKalkulator.Application.Interfaces;
using ThommyKalkulator.Domain.Models;

namespace ThommyKalkulator.Application.Services;

public sealed class CalculationService : ICalculationService
{
    public CalculationProject Calculate(CalculationProject project, GlobalSettings settings)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(settings);

        var machinePowerCost = 0m;
        var machineWearCost = 0m;

        foreach (var machineUsage in project.MachineUsages)
        {
            var runtimeHours = machineUsage.RuntimeHours;
            var watt = machineUsage.Watt;
            var purchasePrice = machineUsage.PurchasePrice;
            var lifetimeHours = machineUsage.LifetimeHours > 0m ? machineUsage.LifetimeHours : 1m;

            machineUsage.PowerCost = runtimeHours * watt / 1000m * settings.ElectricityPricePerKwh;
            machineUsage.WearCost = runtimeHours * purchasePrice / lifetimeHours;

            machinePowerCost += machineUsage.PowerCost;
            machineWearCost += machineUsage.WearCost;
        }

        var materialCost = 0m;
        foreach (var materialUsage in project.MaterialUsages)
        {
            materialUsage.Cost = materialUsage.Quantity * materialUsage.Price;
            materialCost += materialUsage.Cost;
        }

        var additionalCost = project.FreeCostItems.Sum(item => item.Amount);
        var preparationCost = project.PreparationHours * settings.LaborRate;
        var laborCost = project.PostProcessingHours * settings.LaborRate;
        var constructionCost = project.ConstructionHours * settings.ConstructionLaborRate;

        project.PowerCost = machinePowerCost;
        project.WearCost = machineWearCost;
        project.MaterialCost = materialCost;
        project.PreparationCost = preparationCost;
        project.LaborCost = laborCost;
        project.ConstructionCost = constructionCost;
        project.AdditionalCostTotal = additionalCost;
        project.CostPrice = machinePowerCost
                          + machineWearCost
                          + materialCost
                          + preparationCost
                          + laborCost
                          + constructionCost
                          + additionalCost;
        project.FinalPrice = project.CostPrice * (1m + (project.SurchargePercent / 100m));

        return project;
    }
}
