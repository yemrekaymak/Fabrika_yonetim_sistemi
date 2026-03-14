using Microsoft.AspNetCore.Mvc;
using FabrikaBackend.Data;
using FabrikaBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace FabrikaBackend.Controllers;

[Route("api/Personnel")] // Frontend: api/Personnel/... bekliyor
[ApiController]
public class PersonnelController : ControllerBase
{
    private readonly AppDbContext _context;

    public PersonnelController(AppDbContext context)
    {
        _context = context;
    }

    // Frontend: api.get('/api/Personnel/tum-personel-listesi')
    [HttpGet("tum-personel-listesi")]
    public async Task<ActionResult<IEnumerable<Personnel>>> GetPersonnels()
    {
        return await _context.Personnels.ToListAsync();
    }

    // Frontend: api.get(`/api/Personnel/personel-detay-getir/${id}`)
    [HttpGet("personel-detay-getir/{id}")]
    public async Task<ActionResult<Personnel>> GetPersonnel(int id)
    {
        var personnel = await _context.Personnels.FindAsync(id);
        if (personnel == null) return NotFound("Personel bulunamadı.");
        return personnel;
    }

    // Frontend: api.post('/api/Personnel/yeni-personel-ekle', body)
    [HttpPost("yeni-personel-ekle")]
    public async Task<ActionResult<Personnel>> PostPersonnel(Personnel personnel)
    {
        _context.Personnels.Add(personnel);
        await _context.SaveChangesAsync();
        return Ok(personnel);
    }

    // Frontend: api.put(`/api/Personnel/personel-bilgisi-guncelle/${id}`, body)
    [HttpPut("personel-bilgisi-guncelle/{id}")]
    public async Task<IActionResult> PutPersonnel(int id, Personnel personnel)
    {
        if (id != personnel.Id)
        {
            return BadRequest("ID'ler uyuşmuyor!");
        }

        _context.Entry(personnel).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Personnels.Any(e => e.Id == id))
            {
                return NotFound("Güncellenecek personel bulunamadı.");
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // Frontend: api.delete(`/api/Personnel/personel-kaydi-sil/${id}`)
    [HttpDelete("personel-kaydi-sil/{id}")]
    public async Task<IActionResult> DeletePersonnel(int id)
    {
        var personnel = await _context.Personnels.FindAsync(id);
        if (personnel == null) return NotFound("Silinecek personel bulunamadı.");

        _context.Personnels.Remove(personnel);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}