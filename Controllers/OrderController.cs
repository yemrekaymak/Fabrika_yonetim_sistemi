using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Data;
using FabrikaBackend.Models;
using System.Net.Http.Json; // Python'a JSON göndermek için şart!

namespace FabrikaBackend.Controllers;

public class OrderCreateRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

// YENİ: Sadece durumu güncellemek için özel model
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

    // FİLTRELİ GET METODU (Örn: ?status=pending)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders([FromQuery] string? status)
    {
        var query = _context.Orders.AsQueryable();
        
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(o => o.Status == status);
        }
        
        return await query.ToListAsync();
    }

    // TEKİL GET METODU
    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound("Order not found.");
        return order;
    }

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(OrderCreateRequest request)
    {
        var product = await _context.Products.FindAsync(request.ProductId);
        if (product == null) return NotFound("Error: Product not found!");

        // 1. SÜRE HESAPLAMA MANTIĞI
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
            Status = "pending", // Yeni siparişler varsayılan olarak bekliyor
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(newOrder);
        await _context.SaveChangesAsync();

        // --- 🤖 AI TETİKLEYİCİSİ (EVENT-DRIVEN) ---
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
                    Console.WriteLine(aiResult);
                    Console.WriteLine("------------------------------------\n");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("\n⚠️ [UYARI]: AI Tetiklendi ama Python sunucusu (FastAPI) şu an kapalı veya ulaşılamıyor.\n");
            }
        }

        return CreatedAtAction(nameof(GetOrder), new { id = newOrder.Id }, newOrder);
    }

    // YENİ: SADECE SİPARİŞ DURUMU GÜNCELLEME (PATCH)
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatusUpdateRequest request)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound("Order not found.");

        // İzin verilen durumlar
        var allowedStatuses = new[] { "pending", "in_production", "completed", "cancelled" };
        if (!allowedStatuses.Contains(request.Status))
        {
            return BadRequest("Invalid status. Allowed values: pending, in_production, completed, cancelled");
        }

        order.Status = request.Status;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Status updated successfully", new_status = order.Status });
    }

    // TÜM SİPARİŞİ GÜNCELLEME (PUT)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(int id, Order order)
    {
        if (id != order.Id) return BadRequest("ID mismatch!");

        _context.Entry(order).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Orders.Any(e => e.Id == id)) return NotFound("Order to update not found.");
            else throw;
        }

        return NoContent();
    }

    // SİPARİŞ SİLME (DELETE)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound("Order to delete not found.");

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}