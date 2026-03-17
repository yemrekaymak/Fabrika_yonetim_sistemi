using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Data;
using FabrikaBackend.Models;
using System.Net.Http.Json;

namespace FabrikaBackend.Controllers;

public class OrderCreateRequest
{
    public string MusteriAdi { get; set; } = string.Empty;
    public string UrunKodu { get; set; } = string.Empty;
    public string UrunAdi { get; set; } = string.Empty;
    public int Miktar { get; set; }
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
    private readonly HttpClient _httpClient;

    public OrderController(AppDbContext context, HttpClient httpClient)
    {
        _context = context;
        _httpClient = httpClient;
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
        // 58. satırdaki hata düzeltildi: Parametre id ile nesne içindeki id karşılaştırılıyor
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
    public async Task<ActionResult<Order>> CreateOrder(OrderCreateRequest request)
    {
        Product? product = null;
        if (!string.IsNullOrEmpty(request.UrunKodu))
            product = await _context.Products.FindAsync(request.UrunKodu);
            
        if (product == null && !string.IsNullOrEmpty(request.UrunAdi))
            product = await _context.Products.FirstOrDefaultAsync(p => p.UrunAdi == request.UrunAdi);

        if (product == null) return NotFound(new { Mesaj = "Hata: Ürün bulunamadı!" });

        double birimMaliyet = product.BaseCost;
        double birimFiyat = product.SalePrice ?? product.BaseCost;
        double estimatedHours = (product.BirimUretimSuresiSaat ?? 0) * request.Miktar;

        var newOrder = new Order
        {
            // Veritabanı otomatik ID üretmiyorsa GUID oluşturuyoruz
            Id = Guid.NewGuid().ToString(), 
            ProductId = request.UrunKodu, // Ürünün kendi ID'sini atıyoruz
            UrunKodu = product.UrunKodu,
            UrunAdi = product.UrunAdi,
            MusteriAdi = request.MusteriAdi,
            Quantity = request.Miktar,
            EstimatedDays = Math.Round(estimatedHours / 8.0, 2),
            TotalCost = birimMaliyet * request.Miktar,
            SalePrice = birimFiyat,
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(newOrder);
        await _context.SaveChangesAsync();

        // AI Analizi tetikleyici
        try { await _httpClient.PostAsJsonAsync("http://127.0.0.1:8000/api/ai/analyze", new { mode = "risk_analysis" }); }
        catch { Console.WriteLine("AI servisine ulaşılamadı."); }

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
}