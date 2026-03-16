using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Data;
using FabrikaBackend.Models;
using Microsoft.AspNetCore.Authorization;

namespace FabrikaBackend.Controllers;

[Route("api/Accounting")] // Frontend: api/Accounting/... bekliyor
[ApiController]
[Microsoft.AspNetCore.Authorization.AllowAnonymous]
public class AccountingController : ControllerBase
{
    private readonly AppDbContext _context;

    public AccountingController(AppDbContext context)
    {
        _context = context;
    }

    private int GetCompanyId()
    {
        var claim = User.FindFirst("company_id")?.Value;
        return int.TryParse(claim, out var id) ? id : 1;
    }

    // Frontend: api.post('/api/Accounting/gider-kaydet', body)
    [HttpPost("gider-kaydet")]
    public async Task<ActionResult<Expense>> SaveExpense(Expense expense)
    {
        var companyId = GetCompanyId();
        expense.CompanyId = companyId;

        var existing = await _context.Expenses
            .FirstOrDefaultAsync(x => x.GiderAdi == expense.GiderAdi && x.CompanyId == companyId);
            
        if (existing != null)
        {
            existing.Tutar = expense.Tutar;
            existing.Tarih = expense.Tarih;
            existing.GiderTipi = expense.GiderTipi;
            _context.Expenses.Update(existing);
        }
        else { _context.Expenses.Add(expense); }

        await _context.SaveChangesAsync();
        return Ok(expense); // Frontend expenseFromApi fonksiyonu direkt objeyi bekliyor
    }

    // Frontend: api.get('/api/Accounting/gider-listesi')
    [HttpGet("gider-listesi")]
    public async Task<ActionResult<IEnumerable<Expense>>> GetExpenses()
    {
        var companyId = GetCompanyId();
        return await _context.Expenses
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.Tarih)
            .ToListAsync();
    }

    // Frontend: api.delete(`/api/Accounting/gider-sil/${giderAdi}`)
    [HttpDelete("gider-sil/{giderAdi}")]
    public async Task<IActionResult> DeleteExpense(string giderAdi)
    {
        var companyId = GetCompanyId();
        var expense = await _context.Expenses
            .FirstOrDefaultAsync(x => x.GiderAdi == giderAdi && x.CompanyId == companyId);
            
        if (expense == null) return NotFound();
        
        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync();
        return Ok(new { mesaj = "Gider silindi." });
    }

    // Frontend: api.post('/api/Accounting/gelir-kaydet', body)
    [HttpPost("gelir-kaydet")]
    public async Task<ActionResult<Income>> SaveIncome(Income income)
    {
        var companyId = GetCompanyId();
        income.CompanyId = companyId;

        var existing = await _context.Incomes
            .FirstOrDefaultAsync(x => x.GelirAdi == income.GelirAdi && x.CompanyId == companyId);
            
        if (existing != null)
        {
            existing.Tutar = income.Tutar;
            existing.Tarih = income.Tarih;
            _context.Incomes.Update(existing);
        }
        else { _context.Incomes.Add(income); }

        await _context.SaveChangesAsync();
        return Ok(income);
    }

    // Frontend: api.get('/api/Accounting/gelir-listesi')
    [HttpGet("gelir-listesi")]
    public async Task<ActionResult<IEnumerable<Income>>> GetIncomes()
    {
        var companyId = GetCompanyId();
        return await _context.Incomes
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.Tarih)
            .ToListAsync();
    }

    // Frontend: api.delete(`/api/Accounting/gelir-sil/${gelirAdi}`)
    [HttpDelete("gelir-sil/{gelirAdi}")]
    public async Task<IActionResult> DeleteIncome(string gelirAdi)
    {
        var companyId = GetCompanyId();
        var income = await _context.Incomes
            .FirstOrDefaultAsync(x => x.GelirAdi == gelirAdi && x.CompanyId == companyId);
            
        if (income == null) return NotFound();
        
        _context.Incomes.Remove(income);
        await _context.SaveChangesAsync();
        return Ok(new { mesaj = "Gelir silindi." });
    }

    // Frontend: api.get('/api/Accounting/ozet-rapor')
    [HttpGet("ozet-rapor")]
    public async Task<IActionResult> GetSummary()
    {
        var companyId = GetCompanyId();
        var giderler = await _context.Expenses
            .Where(x => x.CompanyId == companyId)
            .ToListAsync();
        var gelirler = await _context.Incomes
            .Where(x => x.CompanyId == companyId)
            .ToListAsync();

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