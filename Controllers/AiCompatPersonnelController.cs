using FabrikaBackend.Data;
using FabrikaBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FabrikaBackend.Controllers;

[ApiController]
[Route("api/personnel")]
public class AiCompatPersonnelController : ControllerBase
{
    private readonly AppDbContext _context;

    public AiCompatPersonnelController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var personnel = await _context.Personnels.OrderBy(p => p.PersonelCode).ThenBy(p => p.FirstName).ToListAsync();
        return Ok(personnel.Select(MapPersonnel));
    }

    [HttpGet("{personnelId}")]
    public async Task<IActionResult> Get(string personnelId)
    {
        var person = await _context.Personnels.FindAsync(personnelId);
        return person is null ? NotFound() : Ok(MapPersonnel(person));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AiCompatPersonnelRequest request)
    {
        var personelCode = request.personel_id?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(personelCode))
            return BadRequest(new { detail = "Personel kodu zorunludur." });

        if (await _context.Personnels.AnyAsync(p => p.PersonelCode == personelCode))
            return BadRequest(new { detail = "Bu personel kodu zaten kayıtlı." });

        var tcNo = await GenerateUniqueTcNoAsync(personelCode);
        var person = new Personnel { TcNo = tcNo };
        ApplyPersonnel(person, request);

        _context.Personnels.Add(person);
        await _context.SaveChangesAsync();
        return Ok(MapPersonnel(person));
    }

    [HttpPut("{personnelId}")]
    public async Task<IActionResult> Update(string personnelId, [FromBody] AiCompatPersonnelRequest request)
    {
        var person = await _context.Personnels.FindAsync(personnelId);
        if (person is null)
            return NotFound();

        ApplyPersonnel(person, request);
        await _context.SaveChangesAsync();
        return Ok(MapPersonnel(person));
    }

    [HttpDelete("{personnelId}")]
    public async Task<IActionResult> Delete(string personnelId)
    {
        var person = await _context.Personnels.FindAsync(personnelId);
        if (person is null)
            return NotFound();

        _context.Personnels.Remove(person);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private void ApplyPersonnel(Personnel person, AiCompatPersonnelRequest request)
    {
        person.PersonelCode = request.personel_id?.Trim() ?? string.Empty;
        person.FirstName = request.ad?.Trim() ?? string.Empty;
        person.LastName = request.soyad?.Trim() ?? string.Empty;
        person.PhoneNumber = NormalizePhone(request.telefon);
        person.Department = request.departman?.Trim() ?? string.Empty;
        person.Position = request.pozisyon?.Trim() ?? string.Empty;
        person.Salary = (decimal)request.maas;
        person.TransportAllowance = (decimal)request.yol_ucreti;
        person.MealAllowance = (decimal)request.yemek_ucreti;
        person.HireDate = request.ise_giris_tarihi ?? DateTime.UtcNow;
        person.TotalAnnualLeave = request.yillik_izin_hakki;
        person.UsedLeave = request.kullanilan_izin;
        person.RemainingLeave = request.kalan_izin > 0 ? request.kalan_izin : Math.Max(0, request.yillik_izin_hakki - request.kullanilan_izin);
        person.PerformanceScore = request.performans_puani;
        person.AverageDailyProduction = request.ortalama_gunluk_uretim;
        person.AbsenteeismDays = request.devamsizlik_gun;
        person.OvertimeHours = request.fazla_mesai_saat;
        person.Certifications = request.egitim_sertifikalari?.Trim() ?? string.Empty;
        person.EmergencyContactName = request.acil_durum_kisi?.Trim() ?? string.Empty;
        person.EmergencyContactPhone = NormalizePhone(request.acil_durum_tel);
        person.IsActive = request.is_active;
        if (person.CreatedAt == default)
            person.CreatedAt = DateTime.UtcNow;
    }

    private async Task<string> GenerateUniqueTcNoAsync(string seed)
    {
        var digits = new string((seed ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length == 11 && !await _context.Personnels.AnyAsync(p => p.TcNo == digits))
            return digits;

        long baseValue = 90000000000L;
        var index = await _context.Personnels.CountAsync() + 1;
        while (true)
        {
            var candidate = (baseValue + index).ToString();
            if (!await _context.Personnels.AnyAsync(p => p.TcNo == candidate))
                return candidate;
            index++;
        }
    }

    private static string NormalizePhone(string? raw)
    {
        var digits = new string((raw ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length == 10)
            digits = $"0{digits}";
        return digits.Length == 11 ? digits : "05000000000";
    }

    private static object MapPersonnel(Personnel person) => new
    {
        id = person.TcNo,
        personel_id = person.PersonelCode,
        ad = person.FirstName,
        soyad = person.LastName,
        telefon = person.PhoneNumber,
        departman = person.Department,
        pozisyon = person.Position,
        maas = person.Salary,
        yol_ucreti = person.TransportAllowance,
        yemek_ucreti = person.MealAllowance,
        ise_giris_tarihi = person.HireDate,
        acil_durum_kisi = person.EmergencyContactName,
        acil_durum_tel = person.EmergencyContactPhone,
        yillik_izin_hakki = person.TotalAnnualLeave,
        kullanilan_izin = person.UsedLeave,
        kalan_izin = person.RemainingLeave,
        performans_puani = person.PerformanceScore,
        ortalama_gunluk_uretim = person.AverageDailyProduction,
        devamsizlik_gun = person.AbsenteeismDays,
        fazla_mesai_saat = person.OvertimeHours,
        egitim_sertifikalari = person.Certifications,
        is_active = person.IsActive,
        created_at = person.CreatedAt,
        absenteeism_rate = person.AbsenteeismRate
    };
}

public sealed class AiCompatPersonnelRequest
{
    public string personel_id { get; set; } = string.Empty;
    public string ad { get; set; } = string.Empty;
    public string soyad { get; set; } = string.Empty;
    public string telefon { get; set; } = string.Empty;
    public string departman { get; set; } = string.Empty;
    public string pozisyon { get; set; } = string.Empty;
    public double maas { get; set; }
    public double yol_ucreti { get; set; }
    public double yemek_ucreti { get; set; }
    public DateTime? ise_giris_tarihi { get; set; }
    public string acil_durum_kisi { get; set; } = string.Empty;
    public string acil_durum_tel { get; set; } = string.Empty;
    public int yillik_izin_hakki { get; set; } = 14;
    public int kullanilan_izin { get; set; }
    public int kalan_izin { get; set; } = 14;
    public double performans_puani { get; set; } = 100;
    public double ortalama_gunluk_uretim { get; set; }
    public int devamsizlik_gun { get; set; }
    public double fazla_mesai_saat { get; set; }
    public string egitim_sertifikalari { get; set; } = string.Empty;
    public bool is_active { get; set; } = true;
}
