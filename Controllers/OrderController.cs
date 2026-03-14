using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Data;
using FabrikaBackend.Models;
using System.Net.Http.Json;

namespace FabrikaBackend.Controllers;

public class OrderCreateRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class OrderStatusUpdateRequest
{
    public string Status { get; set; } = string.Empty;
}

[Route("api/orders")]
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

    // GET: api/orders/siparis-listesi
    [HttpGet("siparis-listesi")]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders([FromQuery] string? status)
    {
        var query = _context.Orders.AsQueryable();
        
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(o => o.Status == status);
        }
        
        return await query.ToListAsync();
    }

    // GET: api/orders/siparis-getir/5
    [HttpGet("siparis-getir/{id}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound("Sipariş bulunamadı.");
        return order;
    }

    // POST: api/orders/siparis-olustur
    [HttpPost("siparis-olustur")]
    public async Task<ActionResult<Order>> CreateOrder(OrderCreateRequest request)
    {
        var product = await _context.Products.FindAsync(request.ProductId);
        if (product == null) return NotFound("Hata: Ürün bulunamadı!");

        double netDailyCapacity = product.GunlukUretim * 0.85;
        double estimatedDays = request.Quantity / netDailyCapacity;
        if (product.HasHeatTreatment) estimatedDays += 1;

        var newOrder = new Order
        {
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            EstimatedDays = Math.Round(estimatedDays, 2),
            TotalCost = product.BaseCost * request.Quantity,
            SalePrice = (product.BaseCost * request.Quantity) * 1.5,
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(newOrder);
        await _context.SaveChangesAsync();

        // --- 🤖 AI RISK ANALİZİ TETİKLEYİCİSİ ---
        double capacityUtilization = (request.Quantity / product.MonthlyCapacity) * 100;
        bool isCriticalOrder = request.Quantity > (product.MonthlyCapacity * 0.20);

        if (capacityUtilization > 85 || isCriticalOrder)
        {
            try 
            {
                var aiRequest = new { mode = "risk_analysis" };
                var response = await _httpClient.PostAsJsonAsync("http://127.0.0.1:8000/api/ai/analyze", aiRequest);
                
                if(response.IsSuccessStatusCode)
                {
                    var aiResult = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("\n🚨 [AI RİSK ANALİZİ DEVREYE GİRDİ] 🚨");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("\n⚠️ [UYARI]: Python sunucusu (FastAPI) ulaşılamaz durumda.\n");
            }
        }

        return CreatedAtAction(nameof(GetOrder), new { id = newOrder.Id }, newOrder);
    }

    // PATCH: api/orders/durum-guncelle/5
    [HttpPatch("durum-guncelle/{id}")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatusUpdateRequest request)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound("Sipariş bulunamadı.");

        var allowedStatuses = new[] { "pending", "in_production", "completed", "cancelled" };
        if (!allowedStatuses.Contains(request.Status))
        {
            return BadRequest("Geçersiz durum. Beklenen: pending, in_production, completed, cancelled");
        }

        order.Status = request.Status;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Durum başarıyla güncellendi", new_status = order.Status });
    }

    // PUT: api/orders/siparis-duzenle/5
    [HttpPut("siparis-duzenle/{id}")]
    public async Task<IActionResult> UpdateOrder(int id, Order order)
    {
        if (id != order.Id) return BadRequest("ID uyuşmazlığı!");

        _context.Entry(order).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Orders.Any(e => e.Id == id)) return NotFound("Güncellenecek sipariş bulunamadı.");
            else throw;
        }

        return NoContent();
    }

    // DELETE: api/orders/siparis-sil/5
    [HttpDelete("siparis-sil/{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound("Silinecek sipariş bulunamadı.");

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}