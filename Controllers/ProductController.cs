using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using FabrikaBackend.Data;
using FabrikaBackend.Models;

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

    // GET: api/Product/liste
    [HttpGet("liste")]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        // Makineleriyle birlikte getiriyoruz
        return await _context.Products.Include(p => p.Machines).ToListAsync();
    }

    // POST: api/Product/yeni-urun-ekle
    [HttpPost("yeni-urun-ekle")]
    public async Task<ActionResult<Product>> PostProduct(Product product)
    {
        if (_context.Products.Any(p => p.UrunKodu == product.UrunKodu))
        {
            return BadRequest(new { Mesaj = "Bu ürün kodu zaten kayıtlı!" });
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return Ok(product);
    }

    // DELETE: api/Product/sil
    [HttpDelete("sil")]
    public async Task<IActionResult> DeleteProduct(Product product)
    {
        var p = await _context.Products.FindAsync(product.UrunKodu);
        if (p == null) return NotFound(new { Mesaj = "Ürün bulunamadı." });

        _context.Products.Remove(p);
        await _context.SaveChangesAsync();
        return Ok(new { Mesaj = "Ürün silindi." });
    }
}