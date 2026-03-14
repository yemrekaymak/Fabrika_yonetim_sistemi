using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Data;
using FabrikaBackend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FabrikaBackend.Controllers;

[Route("api/stok")] // Frontend: /api/stok bekliyor
[ApiController]
public class StockController : ControllerBase
{
    private readonly AppDbContext _context;

    public StockController(AppDbContext context)
    {
        _context = context;
    }

    // Frontend: api.get('/api/stok')
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Stock>>> GetStocks()
    {
        return await _context.Stocks.ToListAsync();
    }

    // Frontend: api.get(`/api/stok/${code}`) 
    // Not: Frontend list.find yapsa da silme/güncelleme için bu route şart.
    [HttpGet("{code}")]
    public async Task<ActionResult<Stock>> GetStock(string code)
    {
        var stock = await _context.Stocks.FindAsync(code);
        if (stock == null) return NotFound("Stok kaydı bulunamadı.");
        return stock;
    }

    // Frontend: api.post('/api/stok', body)
    [HttpPost]
    public async Task<ActionResult<Stock>> CreateStock(Stock stock)
    {
        _context.Stocks.Add(stock);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetStock), new { code = stock.Code }, stock);
    }

    // Frontend: api.put(`/api/stok/${id}`, body)
    [HttpPut("{code}")]
    public async Task<IActionResult> UpdateStock(string code, Stock stock)
    {
        // Modelindeki anahtar alan Code olduğu için eşleşmeyi buradan yapıyoruz
        if (code != stock.Code) return BadRequest("Kod uyuşmazlığı!");

        _context.Entry(stock).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Stocks.Any(e => e.Code == code)) return NotFound("Güncellenecek stok bulunamadı.");
            else throw;
        }

        return NoContent();
    }

    // Frontend: api.delete(`/api/stok/${id}`)
    [HttpDelete("{code}")]
    public async Task<IActionResult> DeleteStock(string code)
    {
        var stock = await _context.Stocks.FindAsync(code);
        if (stock == null) return NotFound("Silinecek stok bulunamadı.");

        _context.Stocks.Remove(stock);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}