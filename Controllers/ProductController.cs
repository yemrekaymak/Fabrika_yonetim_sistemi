using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Data;
using FabrikaBackend.Models;
using Microsoft.AspNetCore.Authorization;

namespace FabrikaBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductController(AppDbContext context)
    {
        _context = context;
    }

    private int GetCompanyId() =>
        int.Parse(User.FindFirst("company_id")!.Value);

    // 1. TÜM ÜRÜN LİSTESİNİ GETİR (Frontend Stok Tablosu İçin Optimize Edildi)
    [HttpGet("tum-urun-listesi")]
    public async Task<ActionResult<IEnumerable<object>>> GetProducts()
    {
        var companyId = GetCompanyId();
        var products = await _context.Products
            .Where(p => p.CompanyId == companyId)
            .ToListAsync();

        // Arkadaşının mappers.js dosyasındaki beklentilerine göre veriyi şekillendiriyoruz
        var response = products.Select(p => new
        {
            p.Id,
            kod = p.UrunKodu,
            ad = p.UrunAdi,
            brutAgirlik = p.BrutAgirlik, // Artık "—" yerine değer gelecek
            netAgirlik = p.NetAgirlik,
            hurdaOrani = p.HurdaOrani,
            kapasite = p.Kapasite,
            kritik = p.KritikSeviye,
            miktar = p.CurrentStock, // Grafiklerin dolmasını sağlayacak miktar
            birimMaliyet = p.Maliyet,
            birimFiyat = p.Fiyat,
            // DURUM HESAPLAMASI: Miktar kritik seviyenin altındaysa "kritik" döner
            durum = p.CurrentStock <= p.KritikSeviye ? "kritik" : "yeterli"
        });

        return Ok(response);
    }

    // 2. ID İLE TEKİL ÜRÜN DETAYI GETİR
    [HttpGet("urun-detay-getir/{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var companyId = GetCompanyId();
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId);
        if (product == null) 
        {
            return NotFound(new { mesaj = $"{id} ID'li ürün sistemde bulunamadı." });
        }
        return product;
    }

    // 3. YENİ ÜRÜN TANIMLA
    [HttpPost("yeni-urun-tanimla")]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        var companyId = GetCompanyId();
        product.CompanyId = companyId;

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    // 4. ÜRÜN BİLGİLERİNİ GÜNCELLE
    [HttpPut("urun-bilgisi-guncelle/{id}")]
    public async Task<IActionResult> UpdateProduct(int id, Product product)
    {
        var companyId = GetCompanyId();
        if (id != product.Id) 
        {
            return BadRequest(new { mesaj = "ID uyuşmazlığı saptandı!" });
        }

        var existing = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId);

        if (existing == null)
        {
            return NotFound(new { mesaj = "Güncellenecek ürün bulunamadı." });
        }

        product.CompanyId = companyId;
        _context.Entry(product).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Products.Any(e => e.Id == id)) 
            {
                return NotFound(new { mesaj = "Güncellenecek ürün bulunamadı." });
            }
            else throw;
        }

        return Ok(new { mesaj = "Ürün bilgileri başarıyla güncellendi.", guncellenenId = id });
    }

    // 5. ÜRÜNÜ SİSTEMDEN KALDIR
    [HttpDelete("urun-kaydi-sil/{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var companyId = GetCompanyId();
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId);
        if (product == null) 
        {
            return NotFound(new { mesaj = "Silinecek ürün bulunamadı." });
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return Ok(new { mesaj = "Ürün kaydı başarıyla silindi." });
    }
}