using Microsoft.AspNetCore.Mvc;
using FabrikaBackend.Data;
using FabrikaBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // [Authorize] kullanacaksan gerekli

namespace FabrikaBackend.Controllers;

[Route("api/[controller]")] // [controller] kullanımı 'Personnel' ismini otomatik alır
[ApiController]
[Authorize] // Güvenlik için: Token olmadan kimse personel listesine erişemesin
public class PersonnelController : ControllerBase
{
    private readonly AppDbContext _context;

    public PersonnelController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Personnel/tum-personel-listesi
    [HttpGet("tum-personel-listesi")]
    public async Task<ActionResult<IEnumerable<Personnel>>> GetPersonnels()
    {
        // Veritabanından departman alanı silindiği için ToList demen yeterli, EF otomatik eşler.
        return await _context.Personnels.ToListAsync();
    }

    // GET: api/Personnel/personel-detay-getir/5
    [HttpGet("personel-detay-getir/{id}")]
    public async Task<ActionResult<Personnel>> GetPersonnel(int id)
    {
        var personnel = await _context.Personnels.FindAsync(id);
        if (personnel == null) return NotFound(new { Mesaj = "Personel bulunamadı." });
        return Ok(personnel);
    }

    // POST: api/Personnel/yeni-personel-ekle
    [HttpPost("yeni-personel-ekle")]
    public async Task<ActionResult<Personnel>> PostPersonnel(Personnel personnel)
    {
        // Department alanı Model'den silindiyse, gelen JSON'da olsa bile EF bunu görmezden gelir.
        _context.Personnels.Add(personnel);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetPersonnel), new { id = personnel.Id }, personnel);
    }

    // PUT: api/Personnel/personel-bilgisi-guncelle/5
    [HttpPut("personel-bilgisi-guncelle/{id}")]
    public async Task<IActionResult> PutPersonnel(int id, Personnel personnel)
    {
        if (id != personnel.Id)
        {
            return BadRequest(new { Mesaj = "ID uyuşmazlığı!" });
        }

        // Entity State'i direkt değiştirmek yerine veritabanındaki kaydı kontrol etmek daha güvenlidir
        _context.Entry(personnel).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Personnels.Any(e => e.Id == id))
            {
                return NotFound(new { Mesaj = "Güncellenecek personel bulunamadı." });
            }
            throw;
        }

        return Ok(new { Mesaj = "Personel başarıyla güncellendi." });
    }

    // DELETE: api/Personnel/personel-kaydi-sil/5
    [HttpDelete("personel-kaydi-sil/{id}")]
    public async Task<IActionResult> DeletePersonnel(int id)
    {
        var personnel = await _context.Personnels.FindAsync(id);
        if (personnel == null) return NotFound(new { Mesaj = "Silinecek personel bulunamadı." });

        _context.Personnels.Remove(personnel);
        await _context.SaveChangesAsync();
        
        return Ok(new { Mesaj = "Personel kaydı sistemden silindi." });
    }
}