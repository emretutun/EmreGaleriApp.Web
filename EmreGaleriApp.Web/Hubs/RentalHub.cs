using EmreGaleriApp.Repository.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace EmreGaleriApp.Web.Hubs
{
    public class RentalHub : Hub
    {
        private readonly AppDbContext _context;

        // Anlık kilitli araçlar: CarId -> ConnectionId
        private static ConcurrentDictionary<int, string> lockedCars = new();

        public RentalHub(AppDbContext context)
        {
            _context = context;
        }

        // Araç müsait mi? (DB + anlık kilit)
        public async Task<bool> CheckIfCarAvailable(int carId)
        {
            bool isAvailableInDb = !await _context.OrderItems
                .Include(oi => oi.Order)
                .AnyAsync(oi => oi.CarId == carId && oi.Order.Status == "Beklemede");

            bool isLocked = lockedCars.ContainsKey(carId);

            return isAvailableInDb && !isLocked;
        }

        // Aracı geçici olarak kilitle
        public async Task<bool> LockCar(int carId)
        {
            if (lockedCars.ContainsKey(carId))
                return false;

            lockedCars[carId] = Context.ConnectionId;
            await Clients.Others.SendAsync("CarLocked", carId);

            return true;
        }

        // Kilidi kaldır (sipariş iptali gibi)
        public async Task UnlockCar(int carId)
        {
            lockedCars.TryRemove(carId, out _);
            await Clients.All.SendAsync("CarUnlocked", carId);
        }

        // Bağlantı koparsa sadece sipariş oluşmamış araçlar için kilidi kaldır
        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            var connectionId = Context.ConnectionId;

            var carsToUnlock = lockedCars
                .Where(kvp => kvp.Value == connectionId)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var carId in carsToUnlock)
            {
                // Sipariş verilmiş mi kontrol et
                bool hasPendingOrder = await _context.OrderItems
                    .Include(oi => oi.Order)
                    .AnyAsync(oi => oi.CarId == carId && oi.Order.Status == "Beklemede");

                // Eğer henüz sipariş oluşmadıysa kilidi kaldır
                if (!hasPendingOrder)
                {
                    lockedCars.TryRemove(carId, out _);
                    await Clients.Others.SendAsync("CarUnlocked", carId);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
