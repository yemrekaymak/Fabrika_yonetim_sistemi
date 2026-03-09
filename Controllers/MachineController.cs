using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Models;
using FabrikaBackend.Data;

namespace FabrikaBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MachineController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MachineController(AppDbContext context)
        {
            _context = context;
        }

        // 1. TÜM MAKİNELERİ LİSTELE
        [HttpGet("tum-makine-listesi")]
        public async Task<ActionResult<IEnumerable<Machine>>> GetMachines()
        {
            return await _context.Machines.ToListAsync();
        }

        // 2. ID'YE GÖRE MAKİNE DETAYI GETİR (İstediğin tekil listeleme)
        [HttpGet("makine-detay-getir/{id}")]
        public async Task<ActionResult<Machine>> GetMachineById(int id)
        {
            var machine = await _context.Machines.FindAsync(id);

            if (machine == null)
            {
                return NotFound(new { mesaj = $"{id} numaralı makine kaydı bulunamadı." });
            }

            return machine;
        }

        // 4. YENİ MAKİNE KAYDI EKLE
        [HttpPost("yeni-makine-ekle")]
        public async Task<ActionResult<Machine>> CreateMachine(Machine machine)
        {
            _context.Machines.Add(machine);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMachineById), new { id = machine.Id }, machine);
        }

        // 5. MAKİNE BİLGİSİNİ GÜNCELLE
        [HttpPut("makine-bilgisi-guncelle/{id}")]
        public async Task<IActionResult> UpdateMachine(int id, Machine machine)
        {
            if (id != machine.Id) return BadRequest(new { mesaj = "ID uyuşmazlığı!" });

            _context.Entry(machine).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { mesaj = "Makine bilgileri başarıyla güncellendi." });
        }

        // 6. MAKİNE KAYDINI SİL
        [HttpDelete("makine-kaydi-sil/{id}")]
        public async Task<IActionResult> DeleteMachine(int id)
        {
            var machine = await _context.Machines.FindAsync(id);
            if (machine == null) return NotFound();

            _context.Machines.Remove(machine);
            await _context.SaveChangesAsync();

            return Ok(new { mesaj = "Makine sistemden başarıyla kaldırıldı." });
        }
    }
}