using FabrikaBackend.Data;
using FabrikaBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FabrikaBackend.Controllers;

[ApiController]
[Route("api/capacity")]
public class CapacityController : ControllerBase
{
    private readonly AppDbContext _context;

    public CapacityController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary()
    {
        var state = await BuildFactoryState();
        return Ok(state.capacity);
    }

    [HttpGet("factory-state")]
    public async Task<IActionResult> FactoryState()
    {
        var state = await BuildFactoryState();
        return Ok(state);
    }

    private async Task<FactoryStateResponse> BuildFactoryState()
    {
        var activeStatuses = new[] { "pending", "approved", "in_production" };
        var activeOrders = await _context.Orders.Where(o => activeStatuses.Contains(o.Status)).ToListAsync();
        var products = await _context.Products.Include(p => p.Machines).ToListAsync();
        var activePersonnel = await _context.Personnels.Where(p => p.IsActive).ToListAsync();
        var productsByCode = products.ToDictionary(p => p.UrunKodu, StringComparer.OrdinalIgnoreCase);

        var productionLineCount = Math.Max(
            1,
            products
                .SelectMany(p => p.Machines)
                .Select(m => m.MachineName?.Trim())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count()
        );
        var totalCapacityDays = productionLineCount * 22.0;
        var totalOpenDays = Math.Round(activeOrders.Sum(o =>
        {
            productsByCode.TryGetValue(o.ProductId ?? string.Empty, out var product);
            return ProductionCapacityEstimator.CalculateEstimatedDays(product, o.Quantity);
        }), 2);
        var utilization = Math.Round((totalOpenDays / totalCapacityDays) * 100, 2);
        var avgMargin = activeOrders.Count > 0
            ? Math.Round(activeOrders.Average(o => o.MarginPercent), 2)
            : 0;
        var criticalProducts = products
            .Where(p => p.CurrentStock < 100)
            .Select(p => p.UrunAdi)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        return new FactoryStateResponse
        {
            capacity = new CapacitySummaryResponse
            {
                total_monthly_capacity = products.Sum(ProductionCapacityEstimator.CalculateEffectiveDailyCapacity) * 22.0,
                total_open_order_days = totalOpenDays,
                utilization_percent = utilization,
                available_days = Math.Max(0, totalCapacityDays - totalOpenDays),
                risk_level = utilization >= 110 ? "CRITICAL" : utilization >= 90 ? "HIGH" : utilization >= 85 ? "MEDIUM" : "LOW",
                active_order_count = activeOrders.Count
            },
            orders = new OrdersStateResponse
            {
                total_active = activeOrders.Count,
                low_margin_count = activeOrders.Count(o => o.MarginPercent < 10),
                average_margin_percent = avgMargin
            },
            personnel = new PersonnelStateResponse
            {
                total_active = activePersonnel.Count,
                average_performance_score = activePersonnel.Count > 0 ? Math.Round(activePersonnel.Average(p => p.PerformanceScore), 1) : 0,
                average_absenteeism_rate = activePersonnel.Count > 0 ? Math.Round(activePersonnel.Average(p => p.AbsenteeismRate) * 100, 2) : 0
            },
            stock = new StockStateResponse
            {
                total_materials = products.Count,
                critical_stock_count = criticalProducts.Count,
                critical_materials = criticalProducts
            }
        };
    }
}

public sealed class FactoryStateResponse
{
    public CapacitySummaryResponse capacity { get; set; } = new();
    public OrdersStateResponse orders { get; set; } = new();
    public PersonnelStateResponse personnel { get; set; } = new();
    public StockStateResponse stock { get; set; } = new();
}

public sealed class CapacitySummaryResponse
{
    public double total_monthly_capacity { get; set; }
    public double total_open_order_days { get; set; }
    public double utilization_percent { get; set; }
    public double available_days { get; set; }
    public string risk_level { get; set; } = "LOW";
    public int active_order_count { get; set; }
}

public sealed class OrdersStateResponse
{
    public int total_active { get; set; }
    public int low_margin_count { get; set; }
    public double average_margin_percent { get; set; }
}

public sealed class PersonnelStateResponse
{
    public int total_active { get; set; }
    public double average_performance_score { get; set; }
    public double average_absenteeism_rate { get; set; }
}

public sealed class StockStateResponse
{
    public int total_materials { get; set; }
    public int critical_stock_count { get; set; }
    public List<string> critical_materials { get; set; } = new();
}
