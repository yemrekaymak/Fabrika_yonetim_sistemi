using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Data;
using FabrikaBackend.Models;
using FabrikaBackend.Services;

namespace FabrikaBackend.Controllers;

public class OrderCreateRequest
{
    public string MusteriAdi { get; set; } = string.Empty;
    public string UrunKodu { get; set; } = string.Empty;
    public string UrunAdi { get; set; } = string.Empty;
    public int Miktar { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public double? SalePrice { get; set; }
}

public class OrderStatusUpdateRequest
{
    public string Status { get; set; } = string.Empty;
}

[Route("api/[controller]")]
[ApiController]
[Microsoft.AspNetCore.Authorization.AllowAnonymous]
public class OrderController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly AiIntegrationService _aiIntegrationService;

    public OrderController(AppDbContext context, AiIntegrationService aiIntegrationService)
    {
        _context = context;
        _aiIntegrationService = aiIntegrationService;
    }

    [HttpGet("tum-siparis-listesi")]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders([FromQuery(Name = "durum")] string? status)
    {
        var query = _context.Orders.AsQueryable();
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(o => o.Status == status);
        }
        return await query.ToListAsync();
    }

    [HttpGet("siparis-detay-getir/{id}")]
    public async Task<ActionResult<Order>> GetOrder(string id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound(new { Mesaj = "Sipariş bulunamadı." });
        return order;
    }

    [HttpPut("siparis-tum-verileri-duzelt/{id}")]
    public async Task<ActionResult<Order>> PutOrder(string id, Order order)
    {
        if (id != order.Id) return BadRequest(new { Mesaj = "ID uyuşmazlığı!" });

        var existing = await _context.Orders.FindAsync(id);
        if (existing == null) return NotFound(new { Mesaj = "Sipariş bulunamadı." });

        if (order.MusteriAdi != null) existing.MusteriAdi = order.MusteriAdi;
        if (order.UrunKodu != null) existing.UrunKodu = order.UrunKodu;
        if (order.UrunAdi != null) existing.UrunAdi = order.UrunAdi;
        if (order.Quantity > 0) existing.Quantity = order.Quantity;
        if (order.EstimatedDays > 0) existing.EstimatedDays = order.EstimatedDays;
        if (order.TotalCost >= 0) existing.TotalCost = order.TotalCost;
        if (order.SalePrice >= 0) existing.SalePrice = order.SalePrice;
        if (!string.IsNullOrEmpty(order.Status)) existing.Status = order.Status;

        await _context.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpPatch("siparis-durumu-guncelle/{id}")]
    public async Task<ActionResult<Order>> PatchOrderStatus(string id, OrderStatusUpdateRequest request)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound(new { Mesaj = "Sipariş bulunamadı." });

        if (!string.IsNullOrEmpty(request.Status)) order.Status = request.Status;

        await _context.SaveChangesAsync();
        return Ok(order);
    }

    [HttpPost("yeni-siparis-olustur")]
    public async Task<ActionResult<Order>> CreateOrder(OrderCreateRequest request, CancellationToken cancellationToken)
    {
        var customerName = FirstNonEmpty(request.MusteriAdi, request.CustomerName);
        var productCode = FirstNonEmpty(request.UrunKodu, request.ProductCode, request.ProductId);
        var productName = FirstNonEmpty(request.UrunAdi);
        var quantity = request.Miktar > 0 ? request.Miktar : request.Quantity;

        if (string.IsNullOrWhiteSpace(customerName))
            return BadRequest(new { Mesaj = "Müşteri adı zorunludur." });

        if (quantity <= 0)
            return BadRequest(new { Mesaj = "Miktar sıfırdan büyük olmalıdır." });

        Product? product = null;
        if (!string.IsNullOrWhiteSpace(productCode))
            product = await _context.Products.FindAsync(new object?[] { productCode }, cancellationToken);

        if (product == null && !string.IsNullOrWhiteSpace(productName))
            product = await _context.Products.FirstOrDefaultAsync(p => p.UrunAdi == productName, cancellationToken);

        if (product == null) return NotFound(new { Mesaj = "Ürün bulunamadı." });

        double birimMaliyet = product.BaseCost;
        double birimFiyat = request.SalePrice is > 0 ? request.SalePrice.Value : product.SalePrice ?? product.BaseCost;
        double estimatedHours = (product.BirimUretimSuresiSaat ?? 0) * quantity;

        var newOrder = new Order
        {
            Id = await CreateNextOrderIdAsync(cancellationToken),
            ProductId = product.UrunKodu,
            UrunKodu = product.UrunKodu,
            UrunAdi = product.UrunAdi,
            MusteriAdi = customerName,
            Quantity = quantity,
            EstimatedDays = ProductionCapacityEstimator.CalculateEstimatedDays(product, quantity),
            TotalCost = birimMaliyet * quantity,
            SalePrice = birimFiyat,
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(newOrder);
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            var factoryState = await AiStateBuilder.BuildFactoryStateAsync(_context, cancellationToken);
            await _aiIntegrationService.PostAnalyzeAsync(new
            {
                mode = "risk_analysis",
                factory_state = factoryState
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AI servisine ulaşılamadı: {ex.Message}");
        }

        return Ok(newOrder);
    }

    [HttpDelete("siparis-kaydi-sil/{id}")]
    public async Task<IActionResult> DeleteOrder(string id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound("Sipariş bulunamadı.");

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return Ok(new { Mesaj = "Sipariş silindi." });
    }

    private static string FirstNonEmpty(params string[] values)
    {
        return values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))?.Trim() ?? string.Empty;
    }

    private async Task<string> CreateNextOrderIdAsync(CancellationToken cancellationToken)
    {
        var ids = await _context.Orders
            .AsNoTracking()
            .Select(order => order.Id)
            .ToListAsync(cancellationToken);

        var nextNumericId = ids
            .Select(id => int.TryParse(id, out var parsed) ? parsed : 0)
            .DefaultIfEmpty(0)
            .Max() + 1;

        return nextNumericId.ToString();
    }
}
