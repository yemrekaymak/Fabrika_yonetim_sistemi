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

    private int GetCompanyId() =>
        int.Parse(User.FindFirst("company_id")!.Value);

    // 1. TÜM PERSONEL LİSTESİNİ GETİR
    [HttpGet("tum-personel-listesi")]
    public async Task<ActionResult<IEnumerable<Personnel>>> GetPersonnels()
    {
        var companyId = GetCompanyId();
        return await _context.Personnels
            .Where(p => p.CompanyId == companyId)
            .ToListAsync();
    }

    // 2. ID İLE PERSONEL DETAYLARINI GETİR (TC, Maaş, Performans vb.)
    [HttpGet("personel-detay-getir/{id}")]
    public async Task<ActionResult<Personnel>> GetPersonnelById(int id)
    {
        var companyId = GetCompanyId();
        var personnel = await _context.Personnels
            .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId);

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
        var companyId = GetCompanyId();
        personnel.CompanyId = companyId;

        _context.Personnels.Add(personnel);
        await _context.SaveChangesAsync();
        
        // Ok yerine CreatedAtAction kullanmak daha profesyoneldir
        return CreatedAtAction(nameof(GetPersonnelById), new { id = personnel.Id }, personnel);
    }

    // 4. PERSONEL BİLGİLERİNİ GÜNCELLE
    [HttpPut("personel-bilgisi-guncelle/{id}")]
    public async Task<IActionResult> PutPersonnel(int id, Personnel personnel)
    {
        var companyId = GetCompanyId();
        if (id != personnel.Id)
        {
            return BadRequest(new { mesaj = "ID'ler uyuşmuyor!" });
        }

        var existing = await _context.Personnels
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId);

        if (existing == null)
        {
            return NotFound(new { mesaj = "Güncellenecek personel bulunamadı." });
        }

        personnel.CompanyId = companyId;
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
        var companyId = GetCompanyId();
        var personnel = await _context.Personnels
            .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId);
        if (personnel == null) 
        {
            return NotFound(new { mesaj = "Silinecek personel bulunamadı." });
        }

        _context.Personnels.Remove(personnel);
        await _context.SaveChangesAsync();
        
        return Ok(new { mesaj = "Personel kaydı başarıyla silindi." });
    }
}