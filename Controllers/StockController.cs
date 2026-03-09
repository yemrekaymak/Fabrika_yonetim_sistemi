using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Data;
using FabrikaBackend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FabrikaBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StockController : ControllerBase
{
    private readonly AppDbContext _context;

    public StockController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("anlik-stok-listesi")]
    public async Task<ActionResult<IEnumerable<Stock>>> GetStocks()
    {
        return await _context.Stocks.ToListAsync();
    }

    [HttpGet("stok-detay-getir/{id}")]
    public async Task<ActionResult<Stock>> GetStockById(int id)
    {
        var stock = await _context.Stocks.FindAsync(id);
        if (stock == null) return NotFound(new { mesaj = $"{id} numaralı stok bulunamadı." });
        return stock;
    }

    [HttpPost("yeni-stok-ekle")]
    public async Task<ActionResult<Stock>> CreateStock(Stock stock)
    {
        _context.Stocks.Add(stock);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetStockById), new { id = stock.Id }, stock);
    }

    [HttpPut("stok-bilgisi-guncelle/{id}")]
    public async Task<IActionResult> UpdateStock(int id, Stock stock)
    {
        if (id != stock.Id) return BadRequest(new { mesaj = "ID uyuşmazlığı!" });

        _context.Entry(stock).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return Ok(new { mesaj = "Stok verileri güncellendi.", guncellenenId = id });
    }

    [HttpDelete("stok-kaydi-sil/{id}")]
    public async Task<IActionResult> DeleteStock(int id)
    {
        var stock = await _context.Stocks.FindAsync(id);
        if (stock == null) return NotFound();

        _context.Stocks.Remove(stock);
        await _context.SaveChangesAsync();
        return Ok(new { mesaj = "Stok kaydı silindi." });
    }
}