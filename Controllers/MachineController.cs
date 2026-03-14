using Microsoft.AspNetCore.Mvc;
using FabrikaBackend.Data;
using FabrikaBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace FabrikaBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MachineController : ControllerBase
{
    private readonly AppDbContext _context;

    public MachineController(AppDbContext context)
    {
        _context = context;
    }

    // Frontend: api.get('/api/Machine/tum-makine-listesi')
    [HttpGet("tum-makine-listesi")]
    public async Task<ActionResult<IEnumerable<Machine>>> GetMachines()
    {
        return await _context.Machines.ToListAsync();
    }

    // Frontend: api.get(`/api/Machine/makine-detay-getir/${id}`)
    [HttpGet("makine-detay-getir/{id}")]
    public async Task<ActionResult<Machine>> GetMachine(int id)
    {
        var machine = await _context.Machines.FindAsync(id);
        if (machine == null) return NotFound("Makine bulunamadı.");
        return machine;
    }

    // Frontend: api.post('/api/Machine/yeni-makine-ekle', body)
    [HttpPost("yeni-makine-ekle")]
    public async Task<ActionResult<Machine>> PostMachine(Machine machine)
    {
        _context.Machines.Add(machine);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMachine), new { id = machine.Id }, machine);
    }

    // Frontend: api.put(`/api/Machine/makine-bilgisi-guncelle/${id}`, body)
    [HttpPut("makine-bilgisi-guncelle/{id}")]
    public async Task<IActionResult> PutMachine(int id, Machine machine)
    {
        if (id != machine.Id)
        {
            return BadRequest("ID'ler uyuşmuyor!");
        }

        _context.Entry(machine).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Machines.Any(e => e.Id == id))
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

    // Frontend: api.delete(`/api/Machine/makine-kaydi-sil/${id}`)
    [HttpDelete("makine-kaydi-sil/{id}")]
    public async Task<IActionResult> DeleteMachine(int id)
    {
        var machine = await _context.Machines.FindAsync(id);
        if (machine == null) return NotFound();

        _context.Machines.Remove(machine);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}