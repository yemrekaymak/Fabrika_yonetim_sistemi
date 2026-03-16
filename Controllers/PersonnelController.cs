using Microsoft.AspNetCore.Mvc;
using FabrikaBackend.Data;
using FabrikaBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace FabrikaBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class PersonnelController : ControllerBase
{
    private readonly AppDbContext _context;

    public PersonnelController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("tum-personel-listesi")]
    public async Task<ActionResult<IEnumerable<Personnel>>> GetPersonnels()
    {
        return await _context.Personnels.ToListAsync();
    }

    [HttpGet("personel-detay-getir/{id}")]
    public async Task<ActionResult<Personnel>> GetPersonnel(string id)
    {
        var p = await _context.Personnels.FindAsync(id);
        if (p == null) return NotFound(new { Mesaj = "Personel bulunamadı." });
        return p;
    }

    [HttpPost("yeni-personel-ekle")]
    public async Task<ActionResult<Personnel>> PostPersonnel(Personnel personnel)
    {
        if (_context.Personnels.Any(p => p.TcNo == personnel.TcNo))
        {
            return BadRequest(new { Mesaj = "Bu TC No zaten kayıtlı." });
        }

        _context.Personnels.Add(personnel);
        await _context.SaveChangesAsync();
        return Ok(personnel);
    }

    [HttpPut("personel-bilgisi-guncelle/{id}")]
    public async Task<IActionResult> PutPersonnel(string id, Personnel personnel)
    {
        var existing = await _context.Personnels.FindAsync(id);
        if (existing == null) return NotFound(new { Mesaj = "Güncellenecek personel bulunamadı." });
        existing.FirstName = personnel.FirstName;
        existing.LastName = personnel.LastName;
        existing.PhoneNumber = personnel.PhoneNumber;
        existing.Position = personnel.Position;
        existing.Salary = personnel.Salary;
        existing.TransportAllowance = personnel.TransportAllowance;
        existing.MealAllowance = personnel.MealAllowance;
        existing.HireDate = personnel.HireDate;
        existing.TotalAnnualLeave = personnel.TotalAnnualLeave;
        existing.UsedLeave = personnel.UsedLeave;
        existing.RemainingLeave = personnel.RemainingLeave;
        existing.OvertimeHours = personnel.OvertimeHours;
        existing.PerformanceScore = personnel.PerformanceScore;
        existing.AverageDailyProduction = personnel.AverageDailyProduction;
        existing.AbsenteeismDays = personnel.AbsenteeismDays;
        existing.EmergencyContactName = personnel.EmergencyContactName;
        existing.EmergencyContactPhone = personnel.EmergencyContactPhone;
        existing.Certifications = personnel.Certifications;
        await _context.SaveChangesAsync();
        return Ok(new { Mesaj = "Bilgiler güncellendi." });
    }

    [HttpDelete("personel-kaydi-sil/{id}")]
    public async Task<IActionResult> DeletePersonnel(string id)
    {
        var p = await _context.Personnels.FindAsync(id);
        if (p == null) return NotFound(new { Mesaj = "Silinecek kayıt bulunamadı." });
        _context.Personnels.Remove(p);
        await _context.SaveChangesAsync();
        return Ok(new { Mesaj = "Kayıt silindi." });
    }
}