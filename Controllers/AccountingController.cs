using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Data;
using FabrikaBackend.Models;
using FabrikaBackend.Services;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;
using System.Security.Claims;

namespace FabrikaBackend.Controllers;

[Route("api/Accounting")]
[ApiController]
[Authorize]
public class AccountingController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly AiIntegrationService _aiIntegrationService;

    public AccountingController(AppDbContext context, AiIntegrationService aiIntegrationService)
    {
        _context = context;
        _aiIntegrationService = aiIntegrationService;
    }

    private int? GetCompanyId()
    {
        var claim = User.FindFirst("company_id")?.Value
                    ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }

    [HttpPost("gider-kaydet")]
    public async Task<ActionResult<Expense>> SaveExpense(Expense expense)
    {
        var companyId = GetCompanyId();
        if (companyId == null) return Unauthorized("Kimlik bulunamadı.");

        var giderAdi = CleanRecordName(expense.GiderAdi);
        if (string.IsNullOrWhiteSpace(giderAdi))
            return BadRequest(new { Mesaj = "Gider adı zorunludur." });

        var scopedGiderAdi = BuildScopedRecordName(companyId.Value, giderAdi);
        var existing = await _context.Expenses
            .FirstOrDefaultAsync(x =>
                x.CompanyId == companyId.Value &&
                (x.GiderAdi == scopedGiderAdi || x.GiderAdi == giderAdi));

        if (existing != null)
        {
            existing.GiderAdi = scopedGiderAdi;
            existing.Tutar = expense.Tutar;
            existing.Tarih = expense.Tarih;
            existing.GiderTipi = NormalizeExpenseType(expense.GiderTipi);
            _context.Expenses.Update(existing);
        }
        else
        {
            expense.CompanyId = companyId.Value;
            expense.GiderAdi = scopedGiderAdi;
            expense.GiderTipi = NormalizeExpenseType(expense.GiderTipi);
            _context.Expenses.Add(expense);
            existing = expense;
        }

        await _context.SaveChangesAsync();
        return Ok(ToClientExpense(existing));
    }

    [HttpGet("gider-listesi")]
    public async Task<ActionResult<IEnumerable<Expense>>> GetExpenses()
    {
        var companyId = GetCompanyId();
        if (companyId == null) return Unauthorized("Kimlik bulunamadı.");

        var expenses = await _context.Expenses
            .Where(x => x.CompanyId == companyId.Value)
            .OrderByDescending(x => x.Tarih)
            .ToListAsync();

        return expenses.Select(ToClientExpense).ToList();
    }

    [HttpDelete("gider-sil/{giderAdi}")]
    public async Task<IActionResult> DeleteExpense(string giderAdi)
    {
        var companyId = GetCompanyId();
        if (companyId == null) return Unauthorized("Kimlik bulunamadı.");

        var scopedGiderAdi = BuildScopedRecordName(companyId.Value, giderAdi);
        var legacyGiderAdi = CleanRecordName(giderAdi);
        var expense = await _context.Expenses
            .FirstOrDefaultAsync(x =>
                x.CompanyId == companyId.Value &&
                (x.GiderAdi == scopedGiderAdi || x.GiderAdi == legacyGiderAdi));

        if (expense == null) return NotFound();

        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync();
        return Ok(new { mesaj = "Gider silindi." });
    }

    [HttpPost("gelir-kaydet")]
    public async Task<ActionResult<Income>> SaveIncome(Income income)
    {
        var companyId = GetCompanyId();
        if (companyId == null) return Unauthorized("Kimlik bulunamadı.");

        var gelirAdi = CleanRecordName(income.GelirAdi);
        if (string.IsNullOrWhiteSpace(gelirAdi))
            return BadRequest(new { Mesaj = "Gelir adı zorunludur." });

        var scopedGelirAdi = BuildScopedRecordName(companyId.Value, gelirAdi);
        var existing = await _context.Incomes
            .FirstOrDefaultAsync(x =>
                x.CompanyId == companyId.Value &&
                (x.GelirAdi == scopedGelirAdi || x.GelirAdi == gelirAdi));

        if (existing != null)
        {
            existing.GelirAdi = scopedGelirAdi;
            existing.Tutar = income.Tutar;
            existing.Tarih = income.Tarih;
            _context.Incomes.Update(existing);
        }
        else
        {
            income.CompanyId = companyId.Value;
            income.GelirAdi = scopedGelirAdi;
            _context.Incomes.Add(income);
            existing = income;
        }

        await _context.SaveChangesAsync();
        return Ok(ToClientIncome(existing));
    }

    [HttpGet("gelir-listesi")]
    public async Task<ActionResult<IEnumerable<Income>>> GetIncomes()
    {
        var companyId = GetCompanyId();
        if (companyId == null) return Unauthorized("Kimlik bulunamadı.");

        var incomes = await _context.Incomes
            .Where(x => x.CompanyId == companyId.Value)
            .OrderByDescending(x => x.Tarih)
            .ToListAsync();

        return incomes.Select(ToClientIncome).ToList();
    }

    [HttpDelete("gelir-sil/{gelirAdi}")]
    public async Task<IActionResult> DeleteIncome(string gelirAdi)
    {
        var companyId = GetCompanyId();
        if (companyId == null) return Unauthorized("Kimlik bulunamadı.");

        var scopedGelirAdi = BuildScopedRecordName(companyId.Value, gelirAdi);
        var legacyGelirAdi = CleanRecordName(gelirAdi);
        var income = await _context.Incomes
            .FirstOrDefaultAsync(x =>
                x.CompanyId == companyId.Value &&
                (x.GelirAdi == scopedGelirAdi || x.GelirAdi == legacyGelirAdi));

        if (income == null) return NotFound();

        _context.Incomes.Remove(income);
        await _context.SaveChangesAsync();
        return Ok(new { mesaj = "Gelir silindi." });
    }

    [HttpGet("ozet-rapor")]
    public async Task<IActionResult> GetSummary()
    {
        var companyId = GetCompanyId();
        if (companyId == null) return Unauthorized("Kimlik bulunamadı.");

        var giderler = await _context.Expenses
            .Where(x => x.CompanyId == companyId.Value)
            .ToListAsync();
        var gelirler = await _context.Incomes
            .Where(x => x.CompanyId == companyId.Value)
            .ToListAsync();

        return Ok(new
        {
            sabit_giderler = giderler.Where(IsFixedExpense).Select(ToClientExpense),
            degisken_giderler = giderler.Where(IsVariableExpense).Select(ToClientExpense),
            gelirler = gelirler.Select(ToClientIncome),
            toplam_gider = giderler.Sum(x => x.Tutar),
            toplam_gelir = gelirler.Sum(x => x.Tutar),
            net_kar = gelirler.Sum(x => x.Tutar) - giderler.Sum(x => x.Tutar)
        });
    }

    [HttpPost("ai-analiz")]
    public async Task<IActionResult> GetAiAnalysis([FromBody] AccountingAiRequest? request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        if (companyId == null) return Unauthorized("Kimlik bulunamadı.");

        try
        {
            var giderler = await _context.Expenses
                .Where(x => x.CompanyId == companyId.Value)
                .ToListAsync(cancellationToken);
            var gelirler = await _context.Incomes
                .Where(x => x.CompanyId == companyId.Value)
                .ToListAsync(cancellationToken);
            var factoryState = await AiStateBuilder.BuildFactoryStateAsync(_context, cancellationToken);

            var body = await _aiIntegrationService.PostAnalyzeAsync(new
            {
                mode = "accounting_analysis",
                factory_state = factoryState,
                accounting = new
                {
                    prompt = request?.Prompt ?? "Muhasebe özet analizi",
                    toplam_gider = giderler.Sum(x => x.Tutar),
                    toplam_gelir = gelirler.Sum(x => x.Tutar),
                    net_kar = gelirler.Sum(x => x.Tutar) - giderler.Sum(x => x.Tutar),
                    gider_kalemleri = giderler
                        .OrderByDescending(x => x.Tutar)
                        .Take(10)
                        .Select(x => new { GiderAdi = CleanRecordName(x.GiderAdi), GiderTipi = NormalizeExpenseType(x.GiderTipi), x.Tutar, x.Tarih }),
                    gelir_kalemleri = gelirler
                        .OrderByDescending(x => x.Tutar)
                        .Take(10)
                        .Select(x => new { GelirAdi = CleanRecordName(x.GelirAdi), x.Tutar, x.Tarih })
                }
            }, cancellationToken);

            return Content(body, "application/json");
        }
        catch (HttpRequestException ex) when (ex.StatusCode.HasValue)
        {
            return StatusCode((int)ex.StatusCode.Value, ex.Message);
        }
        catch (Exception ex)
        {
            return Problem($"Muhasebe AI analizi alınamadı: {ex.Message}", statusCode: 502);
        }
    }

    private static string BuildScopedRecordName(int companyId, string? rawName)
    {
        return $"company:{companyId}:{CleanRecordName(rawName)}";
    }

    private static string CleanRecordName(string? rawName)
    {
        var trimmed = (rawName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            return string.Empty;

        if (!trimmed.StartsWith("company:", StringComparison.OrdinalIgnoreCase))
            return trimmed;

        var firstSeparator = trimmed.IndexOf(':');
        var secondSeparator = firstSeparator >= 0 ? trimmed.IndexOf(':', firstSeparator + 1) : -1;
        return secondSeparator >= 0 && secondSeparator < trimmed.Length - 1
            ? trimmed[(secondSeparator + 1)..].Trim()
            : trimmed;
    }

    private static string NormalizeExpenseType(string? giderTipi)
    {
        var normalized = (giderTipi ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            return "Sabit";

        var comparable = normalized.ToLower(new CultureInfo("tr-TR"));
        return comparable switch
        {
            "degisken" or "değişken" => "Değişken",
            _ => "Sabit"
        };
    }

    private static bool IsFixedExpense(Expense expense)
    {
        return NormalizeExpenseType(expense.GiderTipi) == "Sabit";
    }

    private static bool IsVariableExpense(Expense expense)
    {
        return NormalizeExpenseType(expense.GiderTipi) == "Değişken";
    }

    private static Expense ToClientExpense(Expense expense)
    {
        return new Expense
        {
            GiderAdi = CleanRecordName(expense.GiderAdi),
            Tutar = expense.Tutar,
            GiderTipi = NormalizeExpenseType(expense.GiderTipi),
            Tarih = expense.Tarih,
            CompanyId = expense.CompanyId
        };
    }

    private static Income ToClientIncome(Income income)
    {
        return new Income
        {
            GelirAdi = CleanRecordName(income.GelirAdi),
            Tutar = income.Tutar,
            Tarih = income.Tarih,
            CompanyId = income.CompanyId
        };
    }
}

public sealed class AccountingAiRequest
{
    public string Prompt { get; set; } = "Muhasebe özet analizi";
}
