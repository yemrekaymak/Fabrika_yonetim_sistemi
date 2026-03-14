using Microsoft.AspNetCore.Mvc;
using FabrikaBackend.Data;
using FabrikaBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace FabrikaBackend.Controllers;

[Route("api/personel")]
[ApiController]
public class PersonnelController : ControllerBase
{
    private readonly AppDbContext _context;

    public PersonnelController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/personel/personel-listele
    [HttpGet("personel-listele")]
    public async Task<ActionResult<IEnumerable<Personnel>>> GetPersonnels()
    {
        return await _context.Personnels.ToListAsync();
    }

    // POST: api/personel/personel-ekle
    [HttpPost("personel-ekle")]
    public async Task<ActionResult<Personnel>> PostPersonnel(Personnel personnel)
    {
        _context.Personnels.Add(personnel);
        await _context.SaveChangesAsync();
        return Ok(personnel);
    }

    // PUT: api/personel/personel-guncelle/5
    [HttpPut("personel-guncelle/{id}")]
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

    // DELETE: api/personel/personel-sil/5
    [HttpDelete("personel-sil/{id}")]
    public async Task<IActionResult> DeletePersonnel(int id)
    {
        var personnel = await _context.Personnels.FindAsync(id);
        if (personnel == null) return NotFound("Silinecek personel bulunamadı.");

        _context.Personnels.Remove(personnel);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}