using EmreGaleriApp.Repository.Models;
using EmreGaleriApp.Web.ApiDto;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EmreGaleriApp.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CartApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CartApiController> _logger;

        public CartApiController(AppDbContext context, ILogger<CartApiController> logger)
        {
            _context = context;
            _logger = logger;
        }



        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            _logger.LogInformation("Sipariş oluşturma isteği alındı: {@Request}", request);

            if (request.StartDate >= request.EndDate)
            {
                _logger.LogWarning("Tarih aralığı geçersiz: Start={Start}, End={End}", request.StartDate, request.EndDate);
                return BadRequest("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
            }

            if (request.CartItems == null || request.CartItems.Length == 0)
            {
                _logger.LogWarning("Sepet boş gönderildi.");
                return BadRequest("Sepetiniz boş.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (userId == null)
            {
                _logger.LogError("Kullanıcı kimliği alınamadı.");
                return Unauthorized();
            }

            _logger.LogInformation("Kullanıcı ID: {UserId}", userId);

            var userLicenseIds = await _context.AppUserLicenses
                .Where(ul => ul.AppUserId == userId)
                .Select(ul => ul.LicenseTypeId)
                .ToListAsync();

            _logger.LogInformation("Kullanıcının ehliyetleri: {@Licenses}", userLicenseIds);

            var carIds = request.CartItems.Select(c => c.CarId).ToList();

            var cars = await _context.Cars
                .Include(c => c.CarLicenseTypes)
                .Where(c => carIds.Contains(c.Id))
                .ToListAsync();

            foreach (var car in cars)
            {
                var requiredLicenseIds = car.CarLicenseTypes!.Select(clt => clt.LicenseTypeId).ToList();
                bool hasLicense = requiredLicenseIds.Count == 0 || requiredLicenseIds.Intersect(userLicenseIds).Any();

                if (!hasLicense)
                {
                    _logger.LogWarning("Ehliyet uyuşmazlığı: Kullanıcı '{UserId}' => Gerekli: {@Required} - Sahip: {@Owned}", userId, requiredLicenseIds, userLicenseIds);
                    return BadRequest($"'{car.Brand} {car.Model}' aracını kiralamak için geçerli ehliyetiniz yok.");
                }
            }

            int days = (request.EndDate - request.StartDate).Days;
            if (days <= 0)
            {
                _logger.LogWarning("Geçersiz gün sayısı: {Days}", days);
                return BadRequest("Geçerli bir tarih aralığı giriniz.");
            }

            decimal totalPrice = request.CartItems.Sum(item => item.DailyPrice * days);

            var order = new Order
            {
                AppUserId = userId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TotalPrice = totalPrice,
                Status = "Beklemede",
                OrderItems = request.CartItems.Select(c => new OrderItem
                {
                    CarId = c.CarId,
                    DailyPrice = c.DailyPrice
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Sipariş başarıyla oluşturuldu. OrderId: {OrderId}", order.Id);

            return Ok(new { message = "Sipariş başarıyla oluşturuldu." });
        }
    }
}
