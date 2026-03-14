using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Data;
using FabrikaBackend.Models;

namespace FabrikaBackend.Controllers;

[Route("api/products")] // Frontend: /api/products bekliyor
[ApiController]
public class ProductController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductController(AppDbContext context)
    {
        _context = context;
    }

    // Frontend: api.get('/api/products')
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        return await _context.Products.ToListAsync();
    }

    // Frontend: api.get(`/api/products/${id}`)
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound("Ürün bulunamadı.");
        return product;
    }

    // Frontend: api.post('/api/products', body)
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    // Frontend: api.put(`/api/products/${id}`, body)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, Product product)
    {
        if (id != product.Id) return BadRequest("ID uyuşmazlığı.");

        _context.Entry(product).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Products.Any(e => e.Id == id)) return NotFound("Güncellenecek ürün bulunamadı.");
            else throw;
        }

        return NoContent();
    }

    // Frontend: api.delete(`/api/products/${id}`)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound("Silinecek ürün bulunamadı.");

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}