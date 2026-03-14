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

    // TÜM MAKİNELERİ GETİR
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Machine>>> GetMachines()
    {
        return await _context.Machines.ToListAsync();
    }

    // YENİ MAKİNE EKLE
    [HttpPost]
    public async Task<ActionResult<Machine>> PostMachine(Machine machine)
    {
        _context.Machines.Add(machine);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMachines), new { id = machine.Id }, machine);
    }

    // GÜNCELLEME (PUT)
    [HttpPut("{id}")]
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

    
    // MAKİNE SİL
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMachine(int id)
    {
        var machine = await _context.Machines.FindAsync(id);
        if (machine == null) return NotFound();

        _context.Machines.Remove(machine);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}