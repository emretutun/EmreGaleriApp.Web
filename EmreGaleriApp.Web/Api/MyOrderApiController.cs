using EmreGaleriApp.Repository.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EmreGaleriApp.Web.Api
{
    [Route("api/myorders")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MyOrderApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MyOrderApiController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Kullanıcının siparişlerini döndür
        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var orders = await _context.Orders
                .Where(o => o.AppUserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Car)
                .OrderByDescending(o => o.StartDate)
                .Select(o => new
                {
                    o.Id,
                    o.Status,
                    o.StartDate,
                    o.EndDate,
                    o.TotalPrice,
                    o.DeliveryStatus,
                    OrderItems = o.OrderItems.Select(oi => new {
                        oi.CarId,
                        Brand = oi.Car.Brand,
                        Model = oi.Car.Model,
                        ImageUrl = oi.Car.ImageUrl
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }

        public class CarReviewCreateDto
        {
            public int OrderId { get; set; }
            public int CarId { get; set; }
            public int Rating { get; set; }
            public string Comment { get; set; } = null!;
        }

        [HttpPost("reviews")]
        public async Task<IActionResult> AddCarReview([FromBody] CarReviewCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Aynı kullanıcı ve sipariş için yorum var mı kontrol et
            bool exists = await _context.CarReviews
                .AnyAsync(r => r.OrderId == dto.OrderId && r.UserId == userId);

            if (exists)
                return BadRequest("Bu siparişe zaten yorum yaptınız.");

            // Yeni yorum oluştur
            var review = new CarReview
            {
                OrderId = dto.OrderId,
                CarId = dto.CarId,
                UserId = userId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedDate = DateTime.UtcNow
            };

            _context.CarReviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Yorumunuz kaydedildi." });
        }



    }
}
