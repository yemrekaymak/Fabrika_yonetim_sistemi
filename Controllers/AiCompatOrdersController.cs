using FabrikaBackend.Data;
using FabrikaBackend.Models;
using FabrikaBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FabrikaBackend.Controllers;

[ApiController]
[Route("api/orders")]
public class AiCompatOrdersController : ControllerBase
{
    private readonly AppDbContext _context;

    public AiCompatOrdersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? status)
    {
        var query = _context.Orders.AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(o => o.Status == status);

        var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        return Ok(orders.Select(MapOrder));
    }

    [HttpGet("{orderId}")]
    public async Task<IActionResult> Get(string orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        return order is null ? NotFound() : Ok(MapOrder(order));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AiCompatOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.product_id))
            return BadRequest(new { detail = "Ürün seçimi zorunludur." });

        var product = await _context.Products.FindAsync(request.product_id.Trim());
        if (product is null)
            return NotFound(new { detail = "Ürün bulunamadı." });

        var quantity = Math.Max(0, request.quantity);
        if (quantity <= 0)
            return BadRequest(new { detail = "Miktar 0'dan büyük olmalıdır." });

        var totalCost = product.BaseCost * quantity;
        var totalSalePrice = request.sale_price > 0 ? request.sale_price : (product.SalePrice ?? product.BaseCost) * quantity;
        var marginPercent = totalSalePrice > 0 ? ((totalSalePrice - totalCost) / totalSalePrice) * 100 : 0;

        var order = new Order
        {
            Id = await CreateNextOrderIdAsync(),
            ProductId = product.UrunKodu,
            UrunKodu = product.UrunKodu,
            UrunAdi = product.UrunAdi,
            MusteriAdi = request.customer_name?.Trim() ?? string.Empty,
            Quantity = quantity,
            EstimatedDays = ProductionCapacityEstimator.CalculateEstimatedDays(product, quantity),
            TotalCost = totalCost,
            SalePrice = totalSalePrice,
            MarginPercent = marginPercent,
            Status = "pending",
            DeliveryDate = request.delivery_date,
            Notes = request.notes?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return Ok(MapOrder(order));
    }

    [HttpPut("{orderId}")]
    public async Task<IActionResult> Update(string orderId, [FromBody] AiCompatOrderRequest request)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order is null)
            return NotFound();

        var productCode = string.IsNullOrWhiteSpace(request.product_id) ? order.ProductId : request.product_id.Trim();
        var product = await _context.Products.FindAsync(productCode);
        if (product is null)
            return NotFound(new { detail = "Ürün bulunamadı." });

        var quantity = Math.Max(0, request.quantity);
        var totalCost = product.BaseCost * quantity;
        var totalSalePrice = request.sale_price > 0 ? request.sale_price : (product.SalePrice ?? product.BaseCost) * quantity;

        order.ProductId = product.UrunKodu;
        order.UrunKodu = product.UrunKodu;
        order.UrunAdi = product.UrunAdi;
        order.MusteriAdi = request.customer_name?.Trim() ?? string.Empty;
        order.Quantity = quantity;
        order.EstimatedDays = ProductionCapacityEstimator.CalculateEstimatedDays(product, quantity);
        order.TotalCost = totalCost;
        order.SalePrice = totalSalePrice;
        order.MarginPercent = totalSalePrice > 0 ? ((totalSalePrice - totalCost) / totalSalePrice) * 100 : 0;
        order.DeliveryDate = request.delivery_date;
        order.Notes = request.notes?.Trim() ?? string.Empty;

        await _context.SaveChangesAsync();
        return Ok(MapOrder(order));
    }

    [HttpPatch("{orderId}/status")]
    public async Task<IActionResult> UpdateStatus(string orderId, [FromQuery] string status)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order is null)
            return NotFound();

        order.Status = string.IsNullOrWhiteSpace(status) ? order.Status : status.Trim();
        await _context.SaveChangesAsync();
        return Ok(MapOrder(order));
    }

    [HttpDelete("{orderId}")]
    public async Task<IActionResult> Delete(string orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order is null)
            return NotFound();

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string> CreateNextOrderIdAsync()
    {
        var ids = await _context.Orders
            .AsNoTracking()
            .Select(order => order.Id)
            .ToListAsync();

        var nextNumericId = ids
            .Select(id => int.TryParse(id, out var parsed) ? parsed : 0)
            .DefaultIfEmpty(0)
            .Max() + 1;

        return nextNumericId.ToString();
    }

    private static object MapOrder(Order order) => new
    {
        id = order.Id,
        product_id = order.ProductId,
        quantity = order.Quantity,
        estimated_days = order.EstimatedDays,
        total_cost = order.TotalCost,
        sale_price = order.SalePrice,
        margin_percent = order.MarginPercent,
        status = order.Status,
        customer_name = order.MusteriAdi,
        delivery_date = order.DeliveryDate,
        notes = order.Notes,
        created_at = order.CreatedAt,
        profit = order.Profit,
        urun_kodu = order.UrunKodu,
        urun_adi = order.UrunAdi
    };
}

public sealed class AiCompatOrderRequest
{
    public string product_id { get; set; } = string.Empty;
    public int quantity { get; set; }
    public double sale_price { get; set; }
    public string customer_name { get; set; } = string.Empty;
    public DateTime? delivery_date { get; set; }
    public string notes { get; set; } = string.Empty;
}
