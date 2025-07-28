using EmreGaleriApp.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class CarAvailabilityBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public CarAvailabilityBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Bitiş tarihi geçmiş ve durum onaylı olan siparişlerdeki araçları serbest bırak
                var ordersToRelease = await context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Car)
                    .Where(o => o.Status == "Onaylandı" && o.EndDate < DateTime.Now)
                    .ToListAsync();

                foreach (var order in ordersToRelease)
                {
                    foreach (var item in order.OrderItems)
                    {
                        if (!item.Car.IsAvailable)
                        {
                            item.Car.IsAvailable = true;
                        }
                    }
                    order.Status = "Tamamlandı"; // istersen durumu güncelle
                }

                await context.SaveChangesAsync();
            }

            // 1 saatte bir kontrol et (isteğe göre süreyi değiştir)
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
