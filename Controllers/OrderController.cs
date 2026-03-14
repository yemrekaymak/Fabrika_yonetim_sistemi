using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Data;
using FabrikaBackend.Models;
using System.Net.Http.Json;

namespace FabrikaBackend.Controllers;

public class OrderCreateRequest
{
    // Frontend: musteriAdi, urunAdi ve miktar gönderiyor
    public string? MusteriAdi { get; set; }
    public string? UrunAdi { get; set; }
    public int Miktar { get; set; }
    
    // Eski eşleştirme için ProductId gerekebilir, 
    // ancak frontend urunAdi gönderdiği için iş mantığını ona göre kurmalısın.
    public int ProductId { get; set; } 
}

public class OrderStatusUpdateRequest
{
    public string Status { get; set; } = string.Empty;
}

[Route("api/Order")] // Frontend: api/Order/... bekliyor
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

    // Frontend: api.get('/api/Order/tum-siparis-listesi')
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

    // Frontend: api.get(`/api/Order/siparis-detay-getir/${id}`)
    [HttpGet("siparis-detay-getir/{id}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound("Sipariş bulunamadı.");
        return order;
    }

    // Frontend: api.post('/api/Order/yeni-siparis-olustur', { ... })
    [HttpPost("yeni-siparis-olustur")]
    public async Task<ActionResult<Order>> CreateOrder(OrderCreateRequest request)
    {
        // Not: Frontend urunAdi gönderiyor, ID ile işlem yapacaksan urun adına göre ProductId'yi bulmalısın.
        var product = await _context.Products.FindAsync(request.ProductId);
        if (product == null) return NotFound("Hata: Ürün bulunamadı!");

        double netDailyCapacity = product.GunlukUretim * 0.85;
        double estimatedDays = request.Miktar / netDailyCapacity;
        if (product.HasHeatTreatment) estimatedDays += 1;

        var newOrder = new Order
        {
            ProductId = request.ProductId,
            Quantity = request.Miktar,
            EstimatedDays = Math.Round(estimatedDays, 2),
            TotalCost = product.BaseCost * request.Miktar,
            SalePrice = (product.BaseCost * request.Miktar) * 1.5,
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(newOrder);
        await _context.SaveChangesAsync();

        // --- 🤖 AI RISK ANALİZİ ---
        double capacityUtilization = (request.Miktar / product.MonthlyCapacity) * 100;
        if (capacityUtilization > 85)
        {
            try { await _httpClient.PostAsJsonAsync("http://127.0.0.1:8000/api/ai/analyze", new { mode = "risk_analysis" }); }
            catch { Console.WriteLine("AI ulaşılamadı."); }
        }

        return CreatedAtAction(nameof(GetOrder), new { id = newOrder.Id }, newOrder);
    }

    // Frontend: api.patch(`/api/Order/siparis-durumu-guncelle/${id}`, { status })
    [HttpPatch("siparis-durumu-guncelle/{id}")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatusUpdateRequest request)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound("Sipariş bulunamadı.");

        order.Status = request.Status;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Durum güncellendi", new_status = order.Status });
    }

    // Frontend: api.put(`/api/Order/siparis-tum-verileri-duzelt/${id}`, body)
    [HttpPut("siparis-tum-verileri-duzelt/{id}")]
    public async Task<IActionResult> UpdateOrder(int id, Order order)
    {
        if (id != order.Id) return BadRequest("ID uyuşmazlığı!");

        _context.Entry(order).State = EntityState.Modified;

        try { await _context.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Orders.Any(e => e.Id == id)) return NotFound("Sipariş bulunamadı.");
            else throw;
        }

        return Ok(order);
    }

    // Frontend: api.delete(`/api/Order/siparis-kaydi-sil/${id}`)
    [HttpDelete("siparis-kaydi-sil/{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound("Sipariş bulunamadı.");

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}