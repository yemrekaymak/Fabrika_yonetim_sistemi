using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using FabrikaBackend.Data;
using FabrikaBackend.Models;

namespace FabrikaBackend.Controllers;

/// <summary>Frontend'den gelen ürün ekleme isteği. Machines gönderilse bile eklemede kullanılmaz (ilişki karışıklığını önlemek için).</summary>
public class ProductCreateRequest
{
    public string? urun_kodu { get; set; }
    public string? urun_adi { get; set; }
    public string? ham_madde { get; set; }
    public string? malzeme_tipi { get; set; }
    public string? pres_kategorisi { get; set; }
    public double brut_agirlik_kg { get; set; }
    public double net_agirlik_kg { get; set; }
    public double hurda_orani { get; set; }
    public double base_cost { get; set; }
    public double? sale_price { get; set; }
    public double? birim_uretim_suresi_saat { get; set; }
    public int current_stock { get; set; }
}

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class ProductController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Product/liste
    [HttpGet("liste")]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        var list = await _context.Products
            .Include(p => p.Machines)
            .OrderBy(p => p.UrunKodu)
            .ToListAsync();
        return list;
    }

    // GET: api/Product/urun-getir/{urunKodu}
    [HttpGet("urun-getir/{urunKodu}")]
    public async Task<ActionResult<Product>> GetProduct(string urunKodu)
    {
        var product = await _context.Products.Include(p => p.Machines).FirstOrDefaultAsync(p => p.UrunKodu == urunKodu);
        if (product == null) return NotFound(new { Mesaj = "Ürün bulunamadı." });
        return product;
    }

    // POST: api/Product/yeni-urun-ekle
    [HttpPost("yeni-urun-ekle")]
    public async Task<ActionResult<Product>> PostProduct([FromBody] ProductCreateRequest? request)
    {
        try
        {
            if (request == null)
                return BadRequest(new { Mesaj = "İstek gövdesi boş veya geçersiz JSON." });

            var urunKodu = (request.urun_kodu ?? "").Trim();
            if (string.IsNullOrEmpty(urunKodu))
                return BadRequest(new { Mesaj = "Ürün kodu zorunludur." });

            if (_context.Products.Any(p => p.UrunKodu == urunKodu))
                return BadRequest(new { Mesaj = "Bu ürün kodu zaten kayıtlı!" });

            var product = new Product
            {
                UrunKodu = urunKodu,
                UrunAdi = (request.urun_adi ?? "").Trim().Length > 0 ? (request.urun_adi ?? "").Trim() : urunKodu,
                HamMadde = (request.ham_madde ?? "").Trim() ?? "",
                MalzemeTipi = (request.malzeme_tipi ?? "").Trim().Length > 0 ? (request.malzeme_tipi ?? "").Trim() : "-",
                PresKategorisi = (request.pres_kategorisi ?? "").Trim().Length > 0 ? (request.pres_kategorisi ?? "").Trim() : "-",
                BrutAgirlikKg = request.brut_agirlik_kg,
                NetAgirlikKg = request.net_agirlik_kg,
                HurdaOrani = request.hurda_orani,
                BaseCost = request.base_cost,
                SalePrice = request.sale_price,
                BirimUretimSuresiSaat = request.birim_uretim_suresi_saat,
                CurrentStock = Math.Max(0, request.current_stock),
                Machines = new List<Machine>()
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(product);
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new { Mesaj = "Veritabanı hatası. Tablo şeması güncel olmayabilir.", Detay = ex.Message });
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            return StatusCode(500, new { Mesaj = "Ürün eklenirken hata oluştu.", Detay = msg });
        }
    }

    // PUT: api/Product/urun-guncelle/{urunKodu}
    [HttpPut("urun-guncelle/{urunKodu}")]
    public async Task<ActionResult<Product>> PutProduct(string urunKodu, Product product)
    {
        if (urunKodu != product.UrunKodu)
        {
            var existingByNew = await _context.Products.FindAsync(product.UrunKodu);
            if (existingByNew != null) return BadRequest(new { Mesaj = "Bu ürün kodu zaten başka kayıtta kullanılıyor." });
        }
        var existing = await _context.Products.FindAsync(urunKodu);
        if (existing == null) return NotFound(new { Mesaj = "Ürün bulunamadı." });
        existing.UrunAdi = product.UrunAdi;
        existing.HamMadde = product.HamMadde;
        existing.MalzemeTipi = product.MalzemeTipi;
        existing.PresKategorisi = product.PresKategorisi;
        existing.BrutAgirlikKg = product.BrutAgirlikKg;
        existing.NetAgirlikKg = product.NetAgirlikKg;
        existing.HurdaOrani = product.HurdaOrani;
        existing.BaseCost = product.BaseCost;
        existing.SalePrice = product.SalePrice;
        existing.BirimUretimSuresiSaat = product.BirimUretimSuresiSaat;
        existing.CurrentStock = product.CurrentStock;
        if (urunKodu != product.UrunKodu)
        {
            _context.Products.Remove(existing);
            existing.UrunKodu = product.UrunKodu;
            _context.Products.Add(existing);
        }
        await _context.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("sil")]
    public async Task<IActionResult> DeleteProduct(Product product)
    {
        if (string.IsNullOrEmpty(product?.UrunKodu)) return BadRequest(new { Mesaj = "UrunKodu gerekli." });
        var p = await _context.Products.FindAsync(product.UrunKodu);
        if (p == null) return NotFound(new { Mesaj = "Ürün bulunamadı." });
        _context.Products.Remove(p);
        await _context.SaveChangesAsync();
        return Ok(new { Mesaj = "Ürün silindi." });
    }

    [HttpDelete("sil/{urunKodu}")]
    public async Task<IActionResult> DeleteProductByCode(string urunKodu)
    {
        var p = await _context.Products.FindAsync(urunKodu);
        if (p == null) return NotFound(new { Mesaj = "Ürün bulunamadı." });
        _context.Products.Remove(p);
        await _context.SaveChangesAsync();
        return Ok(new { Mesaj = "Ürün silindi." });
    }
}