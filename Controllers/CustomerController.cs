using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Models;
using FabrikaBackend.Data; // DbContext klasörün farklıysa burayı düzelt

namespace FabrikaBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CustomerController(AppDbContext context)
        {
            _context = context;
        }

        // 1. TÜM MÜŞTERİLERİ LİSTELE
        [HttpGet("tum-musteri-listesi")]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            return await _context.Customers.ToListAsync();
        }

        // 2. ID'YE GÖRE MÜŞTERİ BUL (Senin özellikle istediğin kısım)
        [HttpGet("musteri-detay-getir/{id}")]
        public async Task<ActionResult<Customer>> GetCustomerById(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound(new { mesaj = $"{id} numaralı müşteri sistemde bulunamadı." });
            }

            return customer;
        }

        // 3. YENİ MÜŞTERİ EKLE
        [HttpPost("yeni-musteri-kaydi-ekle")]
        public async Task<ActionResult<Customer>> AddCustomer(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCustomerById), new { id = customer.Id }, customer);
        }

        // 4. MÜŞTERİ BİLGİLERİNİ GÜNCELLE
        [HttpPut("musteri-bilgisi-guncelle/{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, Customer customer)
        {
            if (id != customer.Id) return BadRequest(new { mesaj = "ID uyuşmazlığı!" });

            _context.Entry(customer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id)) return NotFound();
                else throw;
            }

            return Ok(new { mesaj = "Müşteri başarıyla güncellendi." });
        }

// ... (Dosyanın üst kısımları aynı kalacak, sadece en altı veya tamamını değiştirebilirsin)

        [HttpDelete("musteri-kaydi-sil/{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return Ok(new { mesaj = "Müşteri sistemden silindi." });
        }

        private bool CustomerExists(int id)
        {
            // DÜZELTİLDİ: = yerine == kullanıldı
            return _context.Customers.Any(e => e.Id == id); 
        }
    }
}