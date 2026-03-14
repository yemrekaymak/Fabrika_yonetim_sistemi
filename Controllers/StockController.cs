using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Data;
using FabrikaBackend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FabrikaBackend.Controllers;

[Route("api/stok")]
[ApiController]
public class StockController : ControllerBase
{
    private readonly AppDbContext _context;

    public StockController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/stok/stok-listesi
    [HttpGet("stok-listesi")]
    public async Task<ActionResult<IEnumerable<Stock>>> GetStocks()
    {
        return await _context.Stocks.ToListAsync();
    }

    // GET: api/stok/stok-getir/5
    [HttpGet("stok-getir/{id}")]
    public async Task<ActionResult<Stock>> GetStock(int id)
    {
        var stock = await _context.Stocks.FindAsync(id);
        if (stock == null) return NotFound("Stok kaydı bulunamadı.");
        return stock;
    }

    // POST: api/stok/stok-ekle
    [HttpPost("stok-ekle")]
    public async Task<ActionResult<Stock>> CreateStock(Stock stock)
    {
        _context.Stocks.Add(stock);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetStock), new { id = stock.Id }, stock);
    }

    // PUT: api/stok/stok-guncelle/5
    [HttpPut("stok-guncelle/{id}")]
    public async Task<IActionResult> UpdateStock(int id, Stock stock)
    {
        if (id != stock.Id) return BadRequest("ID uyuşmazlığı!");

        _context.Entry(stock).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Stocks.Any(e => e.Id == id)) return NotFound("Güncellenecek stok kaydı bulunamadı.");
            else throw;
        }

        return NoContent();
    }

    // DELETE: api/stok/stok-sil/5
    [HttpDelete("stok-sil/{id}")]
    public async Task<IActionResult> DeleteStock(int id)
    {
        var stock = await _context.Stocks.FindAsync(id);
        if (stock == null) return NotFound("Silinecek stok kaydı bulunamadı.");

        _context.Stocks.Remove(stock);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}