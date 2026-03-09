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

    // 1. TÜM PERSONEL LİSTESİNİ GETİR
    [HttpGet("tum-personel-listesi")]
    public async Task<ActionResult<IEnumerable<Personnel>>> GetPersonnels()
    {
        return await _context.Personnels.ToListAsync();
    }

    // 2. ID İLE PERSONEL DETAYLARINI GETİR (TC, Maaş, Performans vb.)
    [HttpGet("personel-detay-getir/{id}")]
    public async Task<ActionResult<Personnel>> GetPersonnelById(int id)
    {
        var personnel = await _context.Personnels.FindAsync(id);

        if (personnel == null)
        {
            return NotFound(new { mesaj = $"{id} ID'li personel kaydı bulunamadı." });
        }

        return personnel;
    }

    // 3. YENİ PERSONEL KAYDI OLUŞTUR
    [HttpPost("yeni-personel-ekle")]
    public async Task<ActionResult<Personnel>> PostPersonnel(Personnel personnel)
    {
        _context.Personnels.Add(personnel);
        await _context.SaveChangesAsync();
        
        // Ok yerine CreatedAtAction kullanmak daha profesyoneldir
        return CreatedAtAction(nameof(GetPersonnelById), new { id = personnel.Id }, personnel);
    }

    // 4. PERSONEL BİLGİLERİNİ GÜNCELLE
    [HttpPut("personel-bilgisi-guncelle/{id}")]
    public async Task<IActionResult> PutPersonnel(int id, Personnel personnel)
    {
        if (id != personnel.Id)
        {
            return BadRequest(new { mesaj = "ID'ler uyuşmuyor!" });
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
                return NotFound(new { mesaj = "Güncellenecek personel bulunamadı." });
            }
            else
            {
                throw;
            }
        }

        return Ok(new { mesaj = "Personel bilgileri başarıyla güncellendi.", guncellenenId = id });
    }

    // 5. PERSONEL KAYDINI SİSTEMDEN SİL
    [HttpDelete("personel-kaydi-sil/{id}")]
    public async Task<IActionResult> DeletePersonnel(int id)
    {
        var personnel = await _context.Personnels.FindAsync(id);
        if (personnel == null) 
        {
            return NotFound(new { mesaj = "Silinecek personel bulunamadı." });
        }

        _context.Personnels.Remove(personnel);
        await _context.SaveChangesAsync();
        
        return Ok(new { mesaj = "Personel kaydı başarıyla silindi." });
    }
}