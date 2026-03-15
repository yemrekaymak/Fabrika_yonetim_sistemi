using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Data;
using FabrikaBackend.Models;
using System.Net.Http.Json;

namespace FabrikaBackend.Controllers;

public class OrderCreateRequest
{
    public string? MusteriAdi { get; set; }
    public string? UrunKodu { get; set; } // ProductId yerine UrunKodu kullanıyoruz
    public int Miktar { get; set; }
}

public class OrderStatusUpdateRequest
{
    public string Status { get; set; } = string.Empty;
}

[Route("api/[controller]")]
[ApiController]
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

    [HttpPost("yeni-siparis-olustur")]
    public async Task<ActionResult<Order>> CreateOrder(OrderCreateRequest request)
    {
        // ARTIK ÜRÜNÜ URUNKODU İLE BULUYORUZ (String anahtar)
        var product = await _context.Products.FindAsync(request.UrunKodu);
        if (product == null) return NotFound(new { Mesaj = "Hata: Ürün bulunamadı!" });

        // HATALAR BURADA DÜZELTİLDİ: Sildiğimiz alanlar yerine sabit mantık kurduk
        // Kapasite hesaplamalarını modelden sildiğimiz için şimdilik 1.0 (sabit) varsayıyoruz
        double estimatedDays = request.Miktar / 100.0; // Örnek: Günde 100 birim sabit üretim varsayımı

        var newOrder = new Order
        {
            // Order modelinde ProductId (int) varsa stringe çevrilebilir veya 
            // Order modelini de string UrunKodu tutacak şekilde güncellemen gerekebilir.
            // Şimdilik hata almamak için verileri yerleştiriyoruz:
            Quantity = request.Miktar,
            EstimatedDays = Math.Round(estimatedDays, 2),
            TotalCost = (double)product.BaseCost * request.Miktar,
            SalePrice = ((double)product.BaseCost * request.Miktar) * 1.5,
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(newOrder);
        await _context.SaveChangesAsync();

        // AI Analizi (Kapasite hesaplaması sildiğimiz için bu kısmı basitleştirdik)
        try { await _httpClient.PostAsJsonAsync("http://127.0.0.1:8000/api/ai/analyze", new { mode = "risk_analysis" }); }
        catch { Console.WriteLine("AI ulaşılamadı."); }

        return Ok(newOrder);
    }

    [HttpDelete("siparis-kaydi-sil/{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound("Sipariş bulunamadı.");

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return Ok(new { Mesaj = "Sipariş silindi." });
    }
}