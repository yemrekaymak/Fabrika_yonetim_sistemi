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

    // 1. TÜM STOK LİSTESİNİ GETİR
    [HttpGet("anlik-stok-listesi")]
    public async Task<ActionResult<IEnumerable<Stock>>> GetStocks()
    {
        return await _context.Stocks.ToListAsync();
    }

    // 2. ID İLE STOK DETAYI GETİR (Ürün adı, Miktar, Raf No vb.)
    [HttpGet("stok-detay-getir/{id}")]
    public async Task<ActionResult<Stock>> GetStockById(int id)
    {
        var stock = await _context.Stocks.FindAsync(id);

        if (stock == null)
        {
            return NotFound(new { mesaj = $"{id} numaralı stok kaydı bulunamadı." });
        }

        return stock;
    }

    // 3. YENİ STOK GİRİŞİ YAP
    [HttpPost("yeni-stok-ekle")]
    public async Task<ActionResult<Stock>> CreateStock(Stock stock)
    {
        _context.Stocks.Add(stock);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetStockById), new { id = stock.Id }, stock);
    }

    // 4. STOK BİLGİLERİNİ GÜNCELLE (Miktar artırımı/azaltımı için)
    [HttpPut("stok-bilgisi-guncelle/{id}")]
    public async Task<IActionResult> UpdateStock(int id, Stock stock)
    {
        if (id != stock.Id)
        {
            return BadRequest(new { mesaj = "ID uyuşmazlığı saptandı!" });
        }

        _context.Entry(stock).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Stocks.Any(e => e.Id == id))
            {
                return NotFound(new { mesaj = "Güncellenecek stok kaydı bulunamadı." });
            }
            else throw;
        }

        return Ok(new { mesaj = "Stok verileri başarıyla güncellendi.", guncellenenId = id });
    }

    // 5. STOK KAYDINI SİSTEMDEN SİL
    [HttpDelete("stok-kaydi-sil/{id}")]
    public async Task<IActionResult> DeleteStock(int id)
    {
        var stock = await _context.Stocks.FindAsync(id);
        if (stock == null)
        {
            return NotFound(new { mesaj = "Silinecek stok kaydı bulunamadı." });
        }

        _context.Stocks.Remove(stock);
        await _context.SaveChangesAsync();

        return Ok(new { mesaj = "Stok kaydı sistemden başarıyla kaldırıldı." });
    }
}