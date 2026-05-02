using FabrikaBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace FabrikaBackend.Services;

public class ProductionTrackerService : BackgroundService
{
    private static readonly string[] ProducingStatuses = ["Üretimde", "in_production", "Producing"];
    private const string CompletedStatus = "Sevk Edildi";
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProductionTrackerService> _logger;

    public ProductionTrackerService(IServiceProvider serviceProvider, ILogger<ProductionTrackerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("--> [OTONOM SİSTEM] Motorlar ısınıyor, 15 saniye bekleniyor...");
        await Task.Delay(15000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                if (await context.Database.CanConnectAsync(stoppingToken))
                {
                    _logger.LogInformation("--> [KONTROL] Üretim hattı taranıyor...");

                    var activeOrders = await context.Orders
                        .Where(o => ProducingStatuses.Contains(o.Status))
                        .ToListAsync(stoppingToken);

                    foreach (var order in activeOrders)
                    {
                        if (DateTime.UtcNow >= order.CreatedAt.AddSeconds(5))
                        {
                            _logger.LogInformation("--> [TAMAMLANDI] Sipariş {id} üretildi!", order.Id);
                            order.Status = CompletedStatus;
                        }
                    }

                    if (activeOrders.Any())
                    {
                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("--> [UYARI] Tablo henüz hazır değil veya kilitli. Hata: {msg}", ex.Message);
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
