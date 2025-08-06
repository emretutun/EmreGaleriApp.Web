using EmreGaleriApp.Repository.Models;
using EmreGaleriApp.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using EmreGaleriApp.Web.Services;
using EmreGaleriApp.Core.Enums;

namespace EmreGaleriApp.Web.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class OrderApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _mailService;
        private readonly IInvoiceService _invoiceService;

        public OrderApiController(AppDbContext context, IEmailService mailService, IInvoiceService invoiceService)
        {
            _context = context;
            _mailService = mailService;
            _invoiceService = invoiceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.AppUser)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Car)
                .ToListAsync();

            var result = orders.Select(o => new
            {
                o.Id,
                UserName = o.AppUser.UserName,
                StartDate = o.StartDate.ToString("yyyy-MM-dd"),
                EndDate = o.EndDate.ToString("yyyy-MM-dd"),
                o.TotalPrice,
                Status = o.Status,
                DeliveryStatus = o.DeliveryStatus,
                Cars = o.OrderItems.Select(oi => new
                {
                    oi.Car.Id,
                    Brand = oi.Car.Brand,
                    Model = oi.Car.Model,
                    ImageUrl = oi.Car.ImageUrl,
                    oi.DailyPrice
                }),
            });

            return Ok(result);
        }

        [HttpPost("{orderId}/approve")]
        public async Task<IActionResult> ApproveOrder(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Car)
                .Include(o => o.AppUser)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            if (order.Status != "Beklemede")
                return BadRequest("Sipariş zaten işlenmiş.");

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            if (adminId == null)
                return Unauthorized("Admin ID bulunamadı.");

            var adminUser = await _context.Users.FindAsync(adminId);
            var adminUserName = adminUser?.UserName ?? "Bilinmeyen";

            order.Status = "Onaylandı";

            foreach (var orderItem in order.OrderItems)
            {
                var car = orderItem.Car;
                if (car != null)
                {
                    car.IsAvailable = false;
                    _context.Cars.Update(car);
                }
            }

            // 🔥 Türkiye saatine göre kasaya kayıt
            var turkiyeZamani = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"));

            var kasaHareket = new CashRegister
            {
                Amount = order.TotalPrice,
                Description = $"Araç kiralama geliri - Sipariş #{order.Id} | Onaylayan: {adminUserName}",
                CreatedAt = turkiyeZamani,
                Type = "Gelir",
                CreatedByUserId = adminId,
                RelatedEntityType = "Order",
                RelatedEntityId = order.Id
            };
            _context.CashRegisters.Add(kasaHareket);

            await _context.SaveChangesAsync();

            try
            {
                var invoicePdf = _invoiceService.GenerateInvoicePdf(order);
                await _mailService.SendOrderApprovedEmail(order.AppUser.Email, order.AppUser.UserName, invoicePdf);
            }
            catch (Exception)
            {
                // Loglama yapılabilir
            }

            return Ok();
        }

        [HttpPost("{orderId}/reject")]
        public async Task<IActionResult> RejectOrder(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.AppUser)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            if (order.Status != "Beklemede")
                return BadRequest("Sipariş zaten işlenmiş.");

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier)
              ?? User.FindFirstValue("sub");
            if (adminId == null)
                return Unauthorized();

            order.Status = "Reddedildi";

            await _context.SaveChangesAsync();

            try
            {
                await _mailService.SendOrderRejectedEmail(order.AppUser.Email, order.AppUser.UserName);
            }
            catch (Exception)
            {
                // Loglama yapılabilir
            }

            return Ok();
        }

        [HttpPost("set-delivery-status")]
        public async Task<IActionResult> SetDeliveryStatus(int orderId, string status)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Car)
                .Include(o => o.AppUser)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound("Sipariş bulunamadı.");

            if (!Enum.TryParse<DeliveryStatus>(status, out var deliveryStatus))
                return BadRequest("Geçersiz teslim durumu.");

            order.DeliveryStatus = deliveryStatus;

            foreach (var item in order.OrderItems)
            {
                item.Car.IsAvailable = true;
            }

            await _context.SaveChangesAsync();

            if (deliveryStatus == DeliveryStatus.NotDelivered)
            {
                int lateDays = (DateTime.Now.Date - order.EndDate.Date).Days;
                if (lateDays < 0) lateDays = 0;

                decimal penaltyPerDay = 0;
                foreach (var item in order.OrderItems)
                {
                    penaltyPerDay = item.Car.DailyPrice * 2;
                    break;
                }

                decimal totalPenalty = penaltyPerDay * lateDays;

                string mailBody = $@"
Sayın {order.AppUser.UserName},

Kiralama süreniz {order.EndDate:dd.MM.yyyy} tarihinde sona ermiştir ancak aracı teslim etmediğiniz görünmektedir.

Geciken Gün Sayısı: {lateDays}
Günlük Ceza: {penaltyPerDay}₺
Toplam Ceza: {totalPenalty}₺

Lütfen en kısa sürede bizimle iletişime geçiniz.

İyi günler dileriz.
Emre Galeri
";

                await _mailService.SendEmailAsync(order.AppUser.Email!, "Araç Teslim Uyarısı", mailBody);
            }

            return Ok(new { message = "Teslim durumu güncellendi." });
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("fix-notdelivered-cars")]
        public async Task<IActionResult> FixNotDeliveredCars()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Car)
                .Where(o => o.DeliveryStatus == DeliveryStatus.NotDelivered)
                .ToListAsync();

            foreach (var order in orders)
            {
                foreach (var item in order.OrderItems)
                {
                    item.Car.IsAvailable = true;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Teslim edilmedi araçların durumu düzeltildi." });
        }

        [HttpGet("test")]
        [AllowAnonymous]
        public IActionResult Test()
        {
            return Ok("API çalışıyor");
        }
    }
}
