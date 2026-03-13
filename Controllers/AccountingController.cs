using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Data;
using FabrikaBackend.Models;

namespace FabrikaBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountingController : ControllerBase
{
    private readonly AppDbContext _context;

    public AccountingController(AppDbContext context)
    {
        _context = context;
    }

    // --- GİDER İŞLEMLERİ ---

    // Gider Kaydet veya Güncelle (Upsert Mantığı)
    [HttpPost("gider-kaydet")]
    public async Task<ActionResult<Expense>> SaveExpense(Expense expense)
    {
        var existing = await _context.Expenses.FirstOrDefaultAsync(x => x.GiderAdi == expense.GiderAdi);
        
        if (existing != null)
        {
            // Eğer "Elektrik" zaten varsa tutarını ve tarihini güncelle
            existing.Tutar = expense.Tutar;
            existing.Tarih = expense.Tarih;
            existing.GiderTipi = expense.GiderTipi;
            _context.Expenses.Update(existing);
            await _context.SaveChangesAsync();
            return Ok(new { mesaj = $"{expense.GiderAdi} güncellendi.", veri = existing });
        }

        // Yoksa yeni ekle
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();
        return Ok(new { mesaj = "Yeni gider kaydedildi.", veri = expense });
    }

    [HttpDelete("gider-sil/{giderAdi}")]
    public async Task<IActionResult> DeleteExpense(string giderAdi)
    {
        var expense = await _context.Expenses.FirstOrDefaultAsync(x => x.GiderAdi == giderAdi);
        if (expense == null) return NotFound(new { mesaj = "Gider bulunamadı." });

        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync();
        return Ok(new { mesaj = "Gider silindi." });
    }

    // --- GELİR İŞLEMLERİ ---

    [HttpPost("gelir-kaydet")]
    public async Task<ActionResult<Income>> SaveIncome(Income income)
    {
        var existing = await _context.Incomes.FirstOrDefaultAsync(x => x.GelirAdi == income.GelirAdi);
        
        if (existing != null)
        {
            existing.Tutar = income.Tutar;
            existing.Tarih = income.Tarih;
            _context.Incomes.Update(existing);
            await _context.SaveChangesAsync();
            return Ok(new { mesaj = $"{income.GelirAdi} güncellendi.", veri = existing });
        }

        _context.Incomes.Add(income);
        await _context.SaveChangesAsync();
        return Ok(new { mesaj = "Yeni gelir kaydedildi.", veri = income });
    }

    [HttpDelete("gelir-sil/{gelirAdi}")]
    public async Task<IActionResult> DeleteIncome(string gelirAdi)
    {
        var income = await _context.Incomes.FirstOrDefaultAsync(x => x.GelirAdi == gelirAdi);
        if (income == null) return NotFound(new { mesaj = "Gelir bulunamadı." });

        _context.Incomes.Remove(income);
        await _context.SaveChangesAsync();
        return Ok(new { mesaj = "Gelir silindi." });
    }

    // --- ÖZET RAPOR ---
    [HttpGet("ozet-rapor")]
    public async Task<IActionResult> GetSummary()
    {
        var giderler = await _context.Expenses.ToListAsync();
        var gelirler = await _context.Incomes.ToListAsync();

        var toplamSabit = giderler.Where(x => x.GiderTipi == "Sabit").Sum(x => x.Tutar);
        var toplamDegisken = giderler.Where(x => x.GiderTipi == "Değişken").Sum(x => x.Tutar);
        var toplamGelir = gelirler.Sum(x => x.Tutar);

        return Ok(new {
            sabit_giderler = giderler.Where(x => x.GiderTipi == "Sabit"),
            degisken_giderler = giderler.Where(x => x.GiderTipi == "Değişken"),
            gelirler = gelirler,
            ozet = new {
                toplam_gider = toplamSabit + toplamDegisken,
                toplam_gelir = toplamGelir,
                net_kar = toplamGelir - (toplamSabit + toplamDegisken)
            }
        });
    }
}