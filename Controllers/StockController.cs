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

    // GET: api/stok/stok-getir/STK-001
    [HttpGet("stok-getir/{code}")]
    public async Task<ActionResult<Stock>> GetStock(string code)
    {
        var stock = await _context.Stocks.FindAsync(code);
        if (stock == null) return NotFound("Stok kaydı bulunamadı.");
        return stock;
    }

    // POST: api/stok/stok-ekle
    [HttpPost("stok-ekle")]
    public async Task<ActionResult<Stock>> CreateStock(Stock stock)
    {
        _context.Stocks.Add(stock);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetStock), new { code = stock.Code }, stock);
    }

    // PUT: api/stok/stok-guncelle/STK-001
    [HttpPut("stok-guncelle/{code}")]
    public async Task<IActionResult> UpdateStock(string code, Stock stock)
    {
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

    // DELETE: api/stok/stok-sil/STK-001
    [HttpDelete("stok-sil/{code}")]
    public async Task<IActionResult> DeleteStock(string code)
    {
        var stock = await _context.Stocks.FindAsync(code);
        if (stock == null) return NotFound("Silinecek stok bulunamadı.");

        _context.Stocks.Remove(stock);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}