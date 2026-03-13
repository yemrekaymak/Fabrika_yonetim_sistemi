using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Data;
using FabrikaBackend.Models;
using System.Net.Http.Json;

namespace FabrikaBackend.Controllers;

public class OrderCreateRequest
{
    public string MusteriAdi { get; set; } = string.Empty;
    public string UrunAdi { get; set; } = string.Empty;
    public int Miktar { get; set; }
}

public class OrderStatusUpdateRequest
{
    public string Status { get; set; } = string.Empty; // Durum -> Status yaptık
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

    // 1. TÜM SİPARİŞLERİ LİSTELE
    [HttpGet("tum-siparis-listesi")]
    public async Task<ActionResult<IEnumerable<Orders>>> GetOrders([FromQuery] string? status)
    {
        var query = _context.Orders.AsQueryable();
        
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(o => o.Status == status);
        }
        
        return await query.ToListAsync();
    }

    // 2. TEKİL SİPARİŞ DETAYI GETİR
    [HttpGet("siparis-detay-getir/{id}")]
    public async Task<ActionResult<Orders>> GetOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound(new { mesaj = "Sipariş bulunamadı." });
        return order;
    }

    // 3. YENİ SİPARİŞ OLUŞTUR
    [HttpPost("yeni-siparis-olustur")]
    public async Task<ActionResult<Orders>> CreateOrder(OrderCreateRequest request)
    {
        var newOrder = new Orders
        {
            MusteriAdi = request.MusteriAdi,
            UrunAdi = request.UrunAdi,
            Miktar = request.Miktar,
            Status = "pending",
            CreatedAt = DateTime.UtcNow // OlusturulmaTarihi -> CreatedAt yaptık
        };

        _context.Orders.Add(newOrder);
        await _context.SaveChangesAsync();

        // --- 🤖 AI TETİKLEYİCİSİ (DOKUNULMADI) ---
        try 
        {
            var aiRequest = new { mode = "risk_analysis" };
            var response = await _httpClient.PostAsJsonAsync("http://127.0.0.1:8000/api/ai/analyze", aiRequest);
            if(response.IsSuccessStatusCode)
            {
                Console.WriteLine("\n🚨 [AI RİSK ANALİZİ TETİKLENDİ] 🚨");
            }
        }
        catch (Exception)
        {
            Console.WriteLine("\n⚠️ [UYARI]: AI sunucusuna ulaşılamadı.\n");
        }

        return CreatedAtAction(nameof(GetOrder), new { id = newOrder.Id }, newOrder);
    }

    // 4. SİPARİŞ DURUMUNU GÜNCELLE
    [HttpPatch("siparis-durumu-guncelle/{id}")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatusUpdateRequest request)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound(new { mesaj = "Sipariş bulunamadı." });

        // İstersen buradaki değerleri de "beklemede" yerine "pending" vb. yapabilirsin, şimdilik bunları korudum.
        var allowedStatuses = new[] { "pending", "uretimde", "tamamlandi", "iptal" };
        if (!allowedStatuses.Contains(request.Status))
        {
            return BadRequest(new { mesaj = "Geçersiz durum. İzin verilenler: pending, uretimde, tamamlandi, iptal" });
        }

        order.Status = request.Status;
        await _context.SaveChangesAsync();

        return Ok(new { mesaj = "Sipariş durumu güncellendi", yeni_durum = order.Status });
    }

    // 5. TÜM SİPARİŞ VERİSİNİ GÜNCELLE
    [HttpPut("siparis-tum-verileri-duzelt/{id}")]
    public async Task<IActionResult> UpdateOrder(int id, Orders order)
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