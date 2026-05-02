using FabrikaBackend.Data;
using FabrikaBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FabrikaBackend.Controllers;

[ApiController]
[Route("api/products")]
public class AiCompatProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AiCompatProductsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("/")]
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var products = await _context.Products
            .Include(p => p.Machines)
            .OrderBy(p => p.UrunKodu)
            .ToListAsync();

        return Ok(products.Select(MapProduct));
    }

    [HttpGet("{productId}")]
    public async Task<IActionResult> Get(string productId)
    {
        var product = await _context.Products
            .Include(p => p.Machines)
            .FirstOrDefaultAsync(p => p.UrunKodu == productId);

        return product is null ? NotFound() : Ok(MapProduct(product));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AiCompatProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.urun_kodu))
            return BadRequest(new { detail = "Ürün kodu zorunludur." });

        if (await _context.Products.AnyAsync(p => p.UrunKodu == request.urun_kodu))
            return BadRequest(new { detail = "Bu ürün kodu zaten kayıtlı." });

        var product = new Product();
        ApplyProduct(product, request);

        _context.Products.Add(product);
        await SyncMachinesAsync(product, request.machines);
        await _context.SaveChangesAsync();

        return Ok(MapProduct(product));
    }

    [HttpPut("{productId}")]
    public async Task<IActionResult> Update(string productId, [FromBody] AiCompatProductRequest request)
    {
        var product = await _context.Products
            .Include(p => p.Machines)
            .FirstOrDefaultAsync(p => p.UrunKodu == productId);

        if (product is null)
            return NotFound();

        ApplyProduct(product, request);
        await SyncMachinesAsync(product, request.machines);
        await _context.SaveChangesAsync();

        return Ok(MapProduct(product));
    }

    [HttpDelete("{productId}")]
    public async Task<IActionResult> Delete(string productId)
    {
        var product = await _context.Products
            .Include(p => p.Machines)
            .FirstOrDefaultAsync(p => p.UrunKodu == productId);

        if (product is null)
            return NotFound();

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private void ApplyProduct(Product product, AiCompatProductRequest request)
    {
        product.UrunKodu = request.urun_kodu?.Trim() ?? string.Empty;
        product.UrunAdi = string.IsNullOrWhiteSpace(request.urun_adi) ? product.UrunKodu : request.urun_adi.Trim();
        product.HamMadde = request.ham_madde?.Trim() ?? string.Empty;
        product.MalzemeTipi = request.malzeme_tipi?.Trim() ?? string.Empty;
        product.PresKategorisi = request.pres_kategorisi?.Trim() ?? string.Empty;
        product.BrutAgirlikKg = request.brut_agirlik_kg;
        product.NetAgirlikKg = request.net_agirlik_kg;
        product.HurdaOrani = request.hurda_orani;
        product.GunlukUretim = request.gunluk_uretim;
        product.BaseCost = request.base_cost;
        product.SalePrice = request.sale_price;
        product.BirimUretimSuresiSaat = request.birim_uretim_suresi_saat;
        product.CurrentStock = Math.Max(0, request.current_stock);
    }

    private Task SyncMachinesAsync(Product product, List<AiCompatMachineRequest>? machines)
    {
        product.Machines.Clear();
        foreach (var machine in machines ?? new List<AiCompatMachineRequest>())
        {
            if (string.IsNullOrWhiteSpace(machine.machine_name))
                continue;

            product.Machines.Add(new Machine
            {
                MachineName = machine.machine_name.Trim(),
                Details = "Uyumluluk endpoint'i ile güncellendi"
            });
        }

        return Task.CompletedTask;
    }

    private static object MapProduct(Product product) => new
    {
        id = product.UrunKodu,
        urun_kodu = product.UrunKodu,
        urun_adi = product.UrunAdi,
        ham_madde = product.HamMadde,
        malzeme_tipi = product.MalzemeTipi,
        pres_kategorisi = product.PresKategorisi,
        brut_agirlik_kg = product.BrutAgirlikKg,
        net_agirlik_kg = product.NetAgirlikKg,
        hurda_orani = product.HurdaOrani,
        gunluk_uretim = product.GunlukUretim,
        base_cost = product.BaseCost,
        sale_price = product.SalePrice ?? 0,
        birim_uretim_suresi_saat = product.BirimUretimSuresiSaat ?? 0,
        current_stock = product.CurrentStock,
        machines = product.Machines.Select(m => new { machine_name = m.MachineName, is_used = true }).ToList()
    };
}

public sealed class AiCompatProductRequest
{
    public string urun_kodu { get; set; } = string.Empty;
    public string urun_adi { get; set; } = string.Empty;
    public string ham_madde { get; set; } = string.Empty;
    public string malzeme_tipi { get; set; } = string.Empty;
    public string pres_kategorisi { get; set; } = string.Empty;
    public double brut_agirlik_kg { get; set; }
    public double net_agirlik_kg { get; set; }
    public double hurda_orani { get; set; }
    public int gunluk_uretim { get; set; }
    public double base_cost { get; set; }
    public double sale_price { get; set; }
    public double birim_uretim_suresi_saat { get; set; }
    public int current_stock { get; set; }
    public List<AiCompatMachineRequest> machines { get; set; } = new();
}

public sealed class AiCompatMachineRequest
{
    public string machine_name { get; set; } = string.Empty;
    public bool is_used { get; set; } = true;
}
