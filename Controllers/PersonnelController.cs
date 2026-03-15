using Microsoft.AspNetCore.Mvc;
using FabrikaBackend.Data;
using FabrikaBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace FabrikaBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
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

    // GÜNCELLEME: URL'den tcNo istemiyoruz, objenin içindekini kullanıyoruz
    [HttpPut("personel-bilgisi-guncelle")]
    public async Task<IActionResult> PutPersonnel(Personnel personnel)
    {
        // Objenin içindeki TcNo'yu kontrol ediyoruz
        var existing = await _context.Personnels.AnyAsync(p => p.TcNo == personnel.TcNo);
        if (!existing)
        {
            return NotFound(new { Mesaj = "Güncellenecek personel bulunamadı." });
        }

        _context.Entry(personnel).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return Ok(new { Mesaj = "Bilgiler güncellendi." });
    }

    // SİLME: URL'den tcNo istemiyoruz, sadece personelin kendisini (body) gönderiyorlar
    [HttpDelete("personel-kaydi-sil")]
    public async Task<IActionResult> DeletePersonnel(Personnel personnel)
    {
        // Frontend objeyi gönderdiğinde içindeki TcNo üzerinden bulup siliyoruz
        var p = await _context.Personnels.FindAsync(personnel.TcNo);
        if (p == null) return NotFound(new { Mesaj = "Silinecek kayıt bulunamadı." });

        _context.Personnels.Remove(p);
        await _context.SaveChangesAsync();
        return Ok(new { Mesaj = "Kayıt silindi." });
    }
}