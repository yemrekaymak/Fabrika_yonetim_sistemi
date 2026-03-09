using Microsoft.AspNetCore.Mvc;
using FabrikaBackend.Data;
using FabrikaBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace FabrikaBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PersonnelController : ControllerBase
{
    private readonly AppDbContext _context;

    public PersonnelController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Personnel>>> GetPersonnels()
    {
        return await _context.Personnels.ToListAsync();
    }



    // GÜNCELLEME (PUT)
    [HttpPut("{id}")]
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
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }


    [HttpPost]
    public async Task<ActionResult<Personnel>> PostPersonnel(Personnel personnel)
    {
        _context.Personnels.Add(personnel);
        await _context.SaveChangesAsync();
        return Ok(personnel);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePersonnel(int id)
    {
        var personnel = await _context.Personnels.FindAsync(id);
        if (personnel == null) return NotFound();

        _context.Personnels.Remove(personnel);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}