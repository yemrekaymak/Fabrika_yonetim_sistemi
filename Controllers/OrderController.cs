using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Data;
using FabrikaBackend.Models;
using System.Net.Http.Json;

namespace FabrikaBackend.Controllers;

// Mevcut request modellerin kalsın
public class OrderCreateRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
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

    // 1. TÜM SİPARİŞLERİ LİSTELE (OPSİYONEL DURUM FİLTRESİYLE)
    [HttpGet("tum-siparis-listesi")]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders([FromQuery] string? durum)
    {
        var query = _context.Orders.AsQueryable();
        
        if (!string.IsNullOrEmpty(durum))
        {
            query = query.Where(o => o.Status == durum);
        }
        
        return await query.ToListAsync();
    }

    // 2. TEKİL SİPARİŞ DETAYI GETİR
    [HttpGet("siparis-detay-getir/{id}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound(new { mesaj = "Sipariş bulunamadı." });
        return order;
    }

    // 3. YENİ SİPARİŞ OLUŞTUR (AI VE HESAPLAMA MANTIĞI DAHİL)
    [HttpPost("yeni-siparis-olustur")]
    public async Task<ActionResult<Order>> CreateOrder(OrderCreateRequest request)
    {
        var product = await _context.Products.FindAsync(request.ProductId);
        if (product == null) return NotFound(new { mesaj = "Hata: Ürün bulunamadı!" });

        // Süre hesaplama mantığı (Değiştirilmedi)
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

        // --- 🤖 AI TETİKLEYİCİSİ (Aynı mantık korunuyor) ---
        double capacityUtilization = (request.Quantity / product.MonthlyCapacity) * 100;
        bool isCriticalOrder = request.Quantity > (product.MonthlyCapacity * 0.20);

        if (capacityUtilization > 85 || isCriticalOrder)
        {
            try 
            {
                var aiRequest = new { mode = "risk_analysis" };
                // Render'da Python sunucusu farklı bir URL olabilir, buraya dikkat!
                var response = await _httpClient.PostAsJsonAsync("http://127.0.0.1:8000/api/ai/analyze", aiRequest);
                
                if(response.IsSuccessStatusCode)
                {
                    var aiResult = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("\n🚨 [AI RİSK ANALİZİ TETİKLENDİ] 🚨");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("\n⚠️ [UYARI]: AI sunucusuna ulaşılamadı.\n");
            }
        }

        return CreatedAtAction(nameof(GetOrder), new { id = newOrder.Id }, newOrder);
    }

    // 4. SİPARİŞ DURUMUNU GÜNCELLE (Sadece Status)
    [HttpPatch("siparis-durumu-guncelle/{id}")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatusUpdateRequest request)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound(new { mesaj = "Sipariş bulunamadı." });

        var allowedStatuses = new[] { "pending", "in_production", "completed", "cancelled" };
        if (!allowedStatuses.Contains(request.Status))
        {
            return BadRequest(new { mesaj = "Geçersiz durum. İzin verilenler: pending, in_production, completed, cancelled" });
        }

        order.Status = request.Status;
        await _context.SaveChangesAsync();

        return Ok(new { mesaj = "Sipariş durumu güncellendi", yeni_durum = order.Status });
    }

    // 5. TÜM SİPARİŞ VERİSİNİ GÜNCELLE
    [HttpPut("siparis-tum-verileri-duzelt/{id}")]
    public async Task<IActionResult> UpdateOrder(int id, Order order)
    {
        if (id != order.Id) return BadRequest(new { mesaj = "ID uyuşmazlığı!" });

        _context.Entry(order).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return Ok(new { mesaj = "Sipariş verileri başarıyla güncellendi." });
    }

    // 6. SİPARİŞİ SİSTEMDEN SİL
    [HttpDelete("siparis-kaydi-sil/{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound(new { mesaj = "Silinecek sipariş bulunamadı." });

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return Ok(new { mesaj = "Sipariş sistemden kaldırıldı." });
    }
}