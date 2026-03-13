using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Models;
using FabrikaBackend.Data; // DbContext klasörün farklıysa burayı düzelt
using Microsoft.AspNetCore.Authorization;

namespace FabrikaBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CustomerController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCompanyId() =>
            int.Parse(User.FindFirst("company_id")!.Value);

        // 1. TÜM MÜŞTERİLERİ LİSTELE
        [HttpGet("tum-musteri-listesi")]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            var companyId = GetCompanyId();
            return await _context.Customers
                .Where(c => c.CompanyId == companyId)
                .ToListAsync();
        }

        // 2. ID'YE GÖRE MÜŞTERİ BUL (Senin özellikle istediğin kısım)
        [HttpGet("musteri-detay-getir/{id}")]
        public async Task<ActionResult<Customer>> GetCustomerById(int id)
        {
            var companyId = GetCompanyId();
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId);

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
            var companyId = GetCompanyId();
            customer.CompanyId = companyId;

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCustomerById), new { id = customer.Id }, customer);
        }

        // 4. MÜŞTERİ BİLGİLERİNİ GÜNCELLE
        [HttpPut("musteri-bilgisi-guncelle/{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, Customer customer)
        {
            var companyId = GetCompanyId();
            if (id != customer.Id) return BadRequest(new { mesaj = "ID uyuşmazlığı!" });

            var existing = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId);

            if (existing == null)
            {
                return NotFound(new { mesaj = "Güncellenecek müşteri bulunamadı." });
            }

            customer.CompanyId = companyId;
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
            var companyId = GetCompanyId();
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId);
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