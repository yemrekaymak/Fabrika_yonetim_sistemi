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

                    // Yalnızca veritabanı bağlantısını ve tablonun hazır olup olmadığını kontrol ediyoruz.
                    if (await context.Database.CanConnectAsync(stoppingToken))
                    {
                        _logger.LogInformation("--> [KONTROL] Veritabanı bağlantısı sağlıklı.");
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