using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Data;

namespace FabrikaBackend.Services;

public static class AiStateBuilder
{
    private const double AylikKapasiteSaat = 200.0;
    private static readonly string[] ActiveStatuses =
    {
        "pending",
        "Beklemede",
        "Onaylandı",
        "approved",
        "Üretimde",
        "in_production"
    };

    public static async Task<object> BuildFactoryStateAsync(AppDbContext context, CancellationToken ct = default)
    {
        var activeOrders = await context.Orders
            .Where(o => ActiveStatuses.Contains(o.Status))
            .ToListAsync(ct);

        double totalOpenHours = activeOrders.Sum(o => o.EstimatedDays * 8.0);
        double utilization = AylikKapasiteSaat > 0
            ? Math.Round((totalOpenHours / AylikKapasiteSaat) * 100.0, 2)
            : 0;
        string riskLevel = utilization >= 110 ? "CRITICAL" : utilization >= 90 ? "HIGH" : utilization >= 85 ? "MEDIUM" : "LOW";

        int lowMarginCount = 0;
        double totalMarginPct = 0;
        foreach (var o in activeOrders)
        {
            if (o.Quantity <= 0 || o.SalePrice <= 0) continue;
            double birimMaliyet = o.TotalCost / o.Quantity;
            double marginPct = ((o.SalePrice - birimMaliyet) / o.SalePrice) * 100.0;
            if (marginPct < 10) lowMarginCount++;
            totalMarginPct += marginPct;
        }
        double avgMargin = activeOrders.Count > 0 ? Math.Round(totalMarginPct / activeOrders.Count, 2) : 0;

        var personnels = await context.Personnels.ToListAsync(ct);
        int totalPersonnel = personnels.Count;
        double avgPerf = totalPersonnel > 0
            ? Math.Round(personnels.Average(p => p.PerformanceScore), 1)
            : 0;
        var withLeave = personnels.Where(p => p.TotalAnnualLeave > 0).ToList();
        double avgAbsent = withLeave.Count > 0
            ? Math.Round(withLeave.Average(p => (p.AbsenteeismDays * 100.0) / p.TotalAnnualLeave), 2)
            : 0;

        var products = await context.Products.ToListAsync(ct);
        const int kritikStokEsik = 100;
        var criticalProducts = products.Where(p => p.CurrentStock < kritikStokEsik).Select(p => p.UrunAdi ?? p.UrunKodu).ToList();
        var stocks = await context.Stocks.ToListAsync(ct);
        var criticalStocks = stocks.Where(s => s.Quantity < s.CriticalLevel).Select(s => s.Name).ToList();
        var criticalMaterials = criticalProducts.Concat(criticalStocks).Distinct().ToList();

        return new
        {
            capacity = new
            {
                utilization_percent = utilization,
                risk_level = riskLevel,
                active_order_count = activeOrders.Count,
                available_days = Math.Max(0, (AylikKapasiteSaat / 8.0) - (totalOpenHours / 8.0))
            },
            orders = new
            {
                total_active = activeOrders.Count,
                low_margin_count = lowMarginCount,
                average_margin_percent = avgMargin
            },
            personnel = new
            {
                total_active = totalPersonnel,
                average_performance_score = avgPerf,
                average_absenteeism_rate = avgAbsent
            },
            stock = new
            {
                total_materials = products.Count + stocks.Count,
                critical_stock_count = criticalMaterials.Count,
                critical_materials = criticalMaterials
            }
        };
    }
}
