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

    [HttpPost("gider-kaydet")]
    public async Task<ActionResult<Expense>> SaveExpense(Expense expense)
    {
        var existing = await _context.Expenses.FirstOrDefaultAsync(x => x.GiderAdi == expense.GiderAdi);
        if (existing != null)
        {
            existing.Tutar = expense.Tutar;
            existing.Tarih = expense.Tarih;
            existing.GiderTipi = expense.GiderTipi;
            _context.Expenses.Update(existing);
        }
        else { _context.Expenses.Add(expense); }

        await _context.SaveChangesAsync();
        return Ok(new { mesaj = "Gider kaydedildi/güncellendi.", veri = expense });
    }

    [HttpGet("gider-listesi")]
    public async Task<ActionResult<IEnumerable<Expense>>> GetExpenses()
    {
        return await _context.Expenses.OrderByDescending(x => x.Tarih).ToListAsync();
    }

    [HttpGet("gider-getir/{giderAdi}")]
    public async Task<ActionResult<Expense>> GetExpense(string giderAdi)
    {
        var expense = await _context.Expenses.FindAsync(giderAdi);
        if (expense == null) return NotFound();
        return expense;
    }

    [HttpDelete("gider-sil/{giderAdi}")]
    public async Task<IActionResult> DeleteExpense(string giderAdi)
    {
        var expense = await _context.Expenses.FindAsync(giderAdi);
        if (expense == null) return NotFound();
        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync();
        return Ok(new { mesaj = "Gider silindi." });
    }


    [HttpPost("gelir-kaydet")]
    public async Task<ActionResult<Income>> SaveIncome(Income income)
    {
        var existing = await _context.Incomes.FirstOrDefaultAsync(x => x.GelirAdi == income.GelirAdi);
        if (existing != null)
        {
            existing.Tutar = income.Tutar;
            existing.Tarih = income.Tarih;
            _context.Incomes.Update(existing);
        }
        else { _context.Incomes.Add(income); }

        await _context.SaveChangesAsync();
        return Ok(new { mesaj = "Gelir kaydedildi/güncellendi.", veri = income });
    }

    [HttpGet("gelir-listesi")]
    public async Task<ActionResult<IEnumerable<Income>>> GetIncomes()
    {
        return await _context.Incomes.OrderByDescending(x => x.Tarih).ToListAsync();
    }

    [HttpGet("gelir-getir/{gelirAdi}")]
    public async Task<ActionResult<Income>> GetIncome(string gelirAdi)
    {
        var income = await _context.Incomes.FindAsync(gelirAdi);
        if (income == null) return NotFound();
        return income;
    }

    [HttpDelete("gelir-sil/{gelirAdi}")]
    public async Task<IActionResult> DeleteIncome(string gelirAdi)
    {
        var income = await _context.Incomes.FindAsync(gelirAdi);
        if (income == null) return NotFound();
        _context.Incomes.Remove(income);
        await _context.SaveChangesAsync();
        return Ok(new { mesaj = "Gelir silindi." });
    }


    [HttpGet("ozet-rapor")]
    public async Task<IActionResult> GetSummary()
    {
        var giderler = await _context.Expenses.ToListAsync();
        var gelirler = await _context.Incomes.ToListAsync();

        return Ok(new {
            sabit_giderler = giderler.Where(x => x.GiderTipi == "Sabit"),
            degisken_giderler = giderler.Where(x => x.GiderTipi == "Değişken"),
            gelirler = gelirler,
            toplam_gider = giderler.Sum(x => x.Tutar),
            toplam_gelir = gelirler.Sum(x => x.Tutar),
            net_kar = gelirler.Sum(x => x.Tutar) - giderler.Sum(x => x.Tutar)
        });
    }
}