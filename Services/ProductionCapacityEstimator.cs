using FabrikaBackend.Models;

namespace FabrikaBackend.Services;

public static class ProductionCapacityEstimator
{
    private const double WorkHoursPerDay = 8.0;

    public static int CalculateEffectiveDailyCapacity(Product? product)
    {
        if (product is null)
            return 0;

        if (product.GunlukUretim > 0)
            return product.GunlukUretim;

        if (product.BirimUretimSuresiSaat is > 0)
            return Math.Max(1, (int)Math.Floor(WorkHoursPerDay / product.BirimUretimSuresiSaat.Value));

        var machineCount = Math.Max(1, product.Machines?.Count ?? 0);
        var referenceWeight = product.NetAgirlikKg > 0
            ? product.NetAgirlikKg
            : product.BrutAgirlikKg > 0
                ? product.BrutAgirlikKg
                : 0.25;

        var estimatedPerMachine = (int)Math.Round(600 / Math.Max(referenceWeight, 0.05));
        estimatedPerMachine = Math.Clamp(estimatedPerMachine, 50, 5000);

        return machineCount * estimatedPerMachine;
    }

    public static double CalculateEstimatedDays(Product? product, int quantity)
    {
        var normalizedQuantity = Math.Max(0, quantity);
        if (product is null || normalizedQuantity <= 0)
            return 0;

        var effectiveDailyCapacity = CalculateEffectiveDailyCapacity(product);
        if (effectiveDailyCapacity <= 0)
            return 0;

        return Math.Round((double)normalizedQuantity / effectiveDailyCapacity, 2);
    }
}
