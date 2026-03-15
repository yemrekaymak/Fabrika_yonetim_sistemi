using Microsoft.AspNetCore.Mvc;
using FabrikaBackend.Data;
using FabrikaBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace FabrikaBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Token olmadan erişimi engeller
public class MachineController : ControllerBase
{
    private readonly AppDbContext _context;

    public MachineController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Machine/tum-makine-listesi
    [HttpGet("tum-makine-listesi")]
    public async Task<ActionResult<IEnumerable<Machine>>> GetMachines()
    {
        // Yeni modeldeki 3 alanla listeyi getirir: Id, MachineName, Details
        return await _context.Machines.ToListAsync();
    }

    // GET: api/Machine/makine-detay-getir/5
    [HttpGet("makine-detay-getir/{id}")]
    public async Task<ActionResult<Machine>> GetMachine(int id)
    {
        var machine = await _context.Machines.FindAsync(id);
        if (machine == null) return NotFound(new { Mesaj = "Makine bulunamadı." });
        return Ok(machine);
    }

    // POST: api/Machine/yeni-makine-ekle
    [HttpPost("yeni-makine-ekle")]
    public async Task<ActionResult<Machine>> PostMachine(Machine machine)
    {
        // Sadece MachineName ve Details alanlarını işler
        _context.Machines.Add(machine);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetMachine), new { id = machine.Id }, machine);
    }

    // PUT: api/Machine/makine-bilgisi-guncelle/5
    [HttpPut("makine-bilgisi-guncelle/{id}")]
    public async Task<IActionResult> PutMachine(int id, Machine machine)
    {
        if (id != machine.Id)
        {
            return BadRequest(new { Mesaj = "ID uyuşmazlığı!" });
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
                return NotFound(new { Mesaj = "Güncellenecek makine bulunamadı." });
            }
            throw;
        }

        return Ok(new { Mesaj = "Makine bilgileri güncellendi." });
    }

    // DELETE: api/Machine/makine-kaydi-sil/5
    [HttpDelete("makine-kaydi-sil/{id}")]
    public async Task<IActionResult> DeleteMachine(int id)
    {
        var machine = await _context.Machines.FindAsync(id);
        if (machine == null) return NotFound(new { Mesaj = "Silinecek makine bulunamadı." });

        _context.Machines.Remove(machine);
        await _context.SaveChangesAsync();
        
        return Ok(new { Mesaj = "Makine kaydı başarıyla silindi." });
    }
}