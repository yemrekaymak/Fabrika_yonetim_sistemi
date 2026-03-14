using FabrikaBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace FabrikaBackend.Services;

public class ProductionTrackerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProductionTrackerService> _logger;

    public ProductionTrackerService(IServiceProvider serviceProvider, ILogger<ProductionTrackerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 🚀 ZIRH 1: Railway diski hazırlarken 15 saniye bekliyoruz.
        // Bu, 'no such table' hatasını engellemek için hayati bir süredir.
        _logger.LogInformation("--> [OTONOM SİSTEM] Motorlar ısınıyor, 15 saniye bekleniyor...");
        await Task.Delay(15000, stoppingToken); 

        while (!stoppingToken.IsCancellationRequested)
        {
            try 
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // 🚀 ZIRH 2: Veritabanı bağlantısını sorgulamadan işleme girmez.
                    if (await context.Database.CanConnectAsync(stoppingToken))
                    {
                        _logger.LogInformation("--> [KONTROL] Üretim hattı taranıyor...");

                        // Sorgu alanı: Tablo yoksa catch bloğuna düşer, uygulama ÇÖKMEZ.
                        var activeOrders = await context.Orders
                            .Where(o => o.Status == "Producing")
                            .ToListAsync(stoppingToken);

                        foreach (var order in activeOrders)
                        {
                            // 5 saniye kuralı: Otonom üretim tamamlama
                            if (DateTime.UtcNow >= order.CreatedAt.AddSeconds(5)) 
                            {
                                _logger.LogInformation("--> [TAMAMLANDI] Sipariş {id} üretildi!", order.Id);
                                order.Status = "Completed"; 
                            }
                        }
                        
                        if (activeOrders.Any())
                        {
                            await context.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 🚀 ZIRH 3: Hata alınsa da 'fail: Microsoft.Extensions.Hosting' uyarısı gelmez.
                // Sistem 30 saniye sonra tekrar dener ve o sırada tablo hazır olur.
                _logger.LogWarning("--> [UYARI] Tablo henüz hazır değil veya kilitli. Hata: {msg}", ex.Message);
            }

            // 30 saniyede bir otonom kontrol döngüsü
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}   