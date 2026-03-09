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

    // 1. GET: api/musteriler (Tüm müşterileri listele - Front-end'in istediği)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
    {
        return await _context.Customers.ToListAsync();
    }

    // 3. POST: api/musteriler (Yeni Müşteri Ekle - Admin için!)
    [HttpPost]
    public async Task<ActionResult<Customer>> CreateCustomer(Customer customer)
    {
        // Eğer Front-end Id göndermezse, biz "M-1045" formatında otomatik üretelim!
        if (string.IsNullOrEmpty(customer.Id))
        {
            customer.Id = "M-" + new Random().Next(1000, 9999);
        }

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return Ok(customer);
    }

    // 4. PUT: api/musteriler/M1 (Müşteri Bilgilerini Güncelle)
    [HttpPut("{id}")]
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

    // 5. DELETE: api/musteriler/M1 (Müşteriyi Sistemden Sil)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(string id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound("Silinecek müşteri bulunamadı.");

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}