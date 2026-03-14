using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Data;
using FabrikaBackend.Models;

namespace FabrikaBackend.Controllers;

[Route("api/Customer")] // Frontend: api/Customer/... bekliyor
[ApiController]
public class CustomerController : ControllerBase
{
    private readonly AppDbContext _context;

    public CustomerController(AppDbContext context)
    {
        _context = context;
    }

    // Frontend: api.get('/api/Customer/tum-musteri-listesi')
    [HttpGet("tum-musteri-listesi")]
    public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
    {
        return await _context.Customers.ToListAsync();
    }

    // Frontend: api.get(`/api/Customer/musteri-detay-getir/${id}`)
    [HttpGet("musteri-detay-getir/{id}")]
    public async Task<ActionResult<Customer>> GetCustomer(string id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound("Müşteri bulunamadı.");
        return customer;
    }

    // Frontend: api.post('/api/Customer/yeni-musteri-kaydi-ekle', body)
    [HttpPost("yeni-musteri-kaydi-ekle")]
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

    // Frontend: api.put(`/api/Customer/musteri-bilgisi-guncelle/${id}`, body)
    [HttpPut("musteri-bilgisi-guncelle/{id}")]
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

    // Frontend: api.delete(`/api/Customer/musteri-kaydi-sil/${id}`)
    [HttpDelete("musteri-kaydi-sil/{id}")]
    public async Task<IActionResult> DeleteCustomer(string id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound("Silinecek müşteri bulunamadı.");

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}