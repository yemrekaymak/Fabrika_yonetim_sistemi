using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Data;
using FabrikaBackend.Models;

namespace FabrikaBackend.Controllers;

[Route("api/musteriler")]
[ApiController]
public class CustomerController : ControllerBase
{
    private readonly AppDbContext _context;

    public CustomerController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/musteriler/musteri-listele
    [HttpGet("musteri-listele")]
    public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
    {
        return await _context.Customers.ToListAsync();
    }

    // POST: api/musteriler/musteri-ekle
    [HttpPost("musteri-ekle")]
    public async Task<ActionResult<Customer>> CreateCustomer(Customer customer)
    {
        if (string.IsNullOrEmpty(customer.Id))
        {
            customer.Id = "M-" + new Random().Next(1000, 9999);
        }

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return Ok(customer);
    }

    // PUT: api/musteriler/musteri-guncelle/M1
    [HttpPut("musteri-guncelle/{id}")]
    public async Task<IActionResult> UpdateCustomer(string id, Customer customer)
    {
        if (id != customer.Id) return BadRequest("Girdiğiniz ID ile müşteri ID'si uyuşmuyor!");

        _context.Entry(customer).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Customers.Any(e => e.Id == id)) return NotFound("Güncellenecek müşteri bulunamadı.");
            else throw;
        }

        return NoContent();
    }

    // DELETE: api/musteriler/musteri-sil/M1
    [HttpDelete("musteri-sil/{id}")]
    public async Task<IActionResult> DeleteCustomer(string id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound("Silinecek müşteri bulunamadı.");

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}