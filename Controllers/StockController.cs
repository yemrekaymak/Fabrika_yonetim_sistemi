using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Data;
using FabrikaBackend.Models;
using Microsoft.AspNetCore.Authorization;

namespace FabrikaBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StockController : ControllerBase
{
    private readonly AppDbContext _context;

    public StockController(AppDbContext context)
    {
        _context = context;
    }

    private int GetCompanyId() =>
        int.Parse(User.FindFirst("company_id")!.Value);

    // 1. ANLIK STOK LİSTESİ
    [HttpGet("anlik-stok-listesi")]
    public async Task<ActionResult<IEnumerable<Stock>>> GetStocks()
    {
        var companyId = GetCompanyId();
        return await _context.Stocks
            .Where(s => s.CompanyId == companyId)
            .ToListAsync();
    }

    // 2. TEKİL STOK DETAYI
    [HttpGet("stok-detay-getir/{id}")]
    public async Task<ActionResult<Stock>> GetStockById(int id)
    {
        var companyId = GetCompanyId();
        var stock = await _context.Stocks
            .FirstOrDefaultAsync(s => s.Id == id && s.CompanyId == companyId);
        if (stock == null) 
            return NotFound(new { mesaj = $"{id} numaralı stok bulunamadı." });
        
        return stock;
    }

    // 3. YENİ STOK EKLE
    [HttpPost("yeni-stok-ekle")]
    public async Task<ActionResult<Stock>> CreateStock(Stock stock)
    {
        var companyId = GetCompanyId();
        stock.CompanyId = companyId;

        _context.Stocks.Add(stock);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetStockById), new { id = stock.Id }, stock);
    }

    // 4. STOK GÜNCELLE
    [HttpPut("stok-bilgisi-guncelle/{id}")]
    public async Task<IActionResult> UpdateStock(int id, Stock stock)
    {
        var companyId = GetCompanyId();
        if (id != stock.Id) 
            return BadRequest(new { mesaj = "ID uyuşmazlığı!" });

        var existing = await _context.Stocks
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.CompanyId == companyId);

        if (existing == null)
        {
            return NotFound(new { mesaj = "Güncellenecek stok bulunamadı." });
        }

        stock.CompanyId = companyId;
        _context.Entry(stock).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Stocks.Any(e => e.Id == id)) 
                return NotFound(new { mesaj = "Güncellenecek stok bulunamadı." });
            throw;
        }

        return Ok(new { mesaj = "Stok verileri başarıyla güncellendi.", guncellenenId = id });
    }

    // 5. STOK SİL
    [HttpDelete("stok-kaydi-sil/{id}")]
    public async Task<IActionResult> DeleteStock(int id)
    {
        var companyId = GetCompanyId();
        var stock = await _context.Stocks
            .FirstOrDefaultAsync(s => s.Id == id && s.CompanyId == companyId);
        if (stock == null) 
            return NotFound(new { mesaj = "Silinecek stok kaydı bulunamadı." });

        _context.Stocks.Remove(stock);
        await _context.SaveChangesAsync();
        
        return Ok(new { mesaj = "Stok kaydı sistemden silindi." });
    }
}