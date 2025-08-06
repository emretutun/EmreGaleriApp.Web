using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using EmreGaleriApp.Web.Hubs;
using EmreGaleriApp.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using EmreGaleriApp.Core.ViewModels;
using EmreGaleriApp.Service.Services;
using EmreGaleriApp.Core.Enums;

namespace EmreGaleriApp.Web.Controllers
{
    [Authorize] // Sadece giriş yapanlar erişebilir
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<RentalHub> _rentalHub;
        private readonly ICarReviewService _reviewService;

        public OrderController(AppDbContext context, IHubContext<RentalHub> rentalHub, ICarReviewService reviewService)
        {
            _context = context;
            _rentalHub = rentalHub;
            _reviewService = reviewService;
        }

        // Kullanıcının siparişleri sayfası
        public async Task<IActionResult> MyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var orders = await _context.Orders
                .Where(o => o.AppUserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Car)
                .OrderByDescending(o => o.StartDate)
                .ToListAsync();

            // Kullanıcının yorum yaptığı siparişlerin Id’lerini al
            var reviewedOrderIds = await _context.CarReviews
                .Where(r => r.UserId == userId)
                .Select(r => r.OrderId)
                .Distinct()
                .ToListAsync();

            ViewBag.ReviewedOrderIds = reviewedOrderIds;

            return View(orders);
        }


        // Sipariş iptal etme (POST)
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            order.Status = "Reddedildi";
            await _context.SaveChangesAsync();

            // Kilitleri kaldır
            foreach (var item in order.OrderItems)
            {
                await _rentalHub.Clients.All.SendAsync("CarUnlocked", item.CarId);
            }

            return RedirectToAction(nameof(MyOrders));
        }

        [HttpGet]
        [Authorize(Roles = "Kullanici,Yonetici,Yetkili")]
        public async Task<IActionResult> AddReview(int orderId, int carId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null || order.DeliveryStatus != DeliveryStatus.Delivered)
                return NotFound();

            if (!order.OrderItems.Any(oi => oi.CarId == carId))
                return BadRequest();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var alreadyReviewed = await _reviewService.HasUserReviewedOrderAsync(userId, orderId, carId);
            if (alreadyReviewed)
            {
                // Yorum yapılmışsa tekrar yorum ekranı açılmasın
                TempData["ReviewError"] = "Bu sipariş hakkında zaten yorum yaptınız.";
                return RedirectToAction("MyOrders");
            }

            var vm = new CarReviewViewModel
            {
                OrderId = orderId,
                CarId = carId
            };

            return View(vm);
        }


        [HttpPost]
        [Authorize(Roles = "Kullanici,Yonetici,Yetkili")]
        public async Task<IActionResult> AddReview(CarReviewViewModel model, [FromServices] ICarReviewService reviewService)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var alreadyReviewed = await reviewService.HasUserReviewedOrderAsync(userId, model.OrderId, model.CarId);
            if (alreadyReviewed)
            {
                ModelState.AddModelError("", "Bu siparişe zaten yorum yaptınız.");
                return View(model);
            }

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == model.OrderId);
            if (order == null || order.DeliveryStatus != DeliveryStatus.Delivered)
                return NotFound();

            var review = new CarReview
            {
                // Id atama yok!
                OrderId = model.OrderId,
                CarId = model.CarId,
                UserId = userId,
                Rating = model.Rating,
                Comment = model.Comment!,
                CreatedDate = DateTime.Now
            };

            await reviewService.AddReviewAsync(review);

            return RedirectToAction("MyOrders");
        }

    }
}
