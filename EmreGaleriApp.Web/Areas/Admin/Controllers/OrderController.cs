using EmreGaleriApp.Core.Enums;
using EmreGaleriApp.Repository.Models;
using EmreGaleriApp.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EmreGaleriApp.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Yonetici,Yetkili")]
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IInvoiceService _invoiceService;
        private readonly ICashRegisterService _cashRegisterService;

        public OrderController(AppDbContext context, IEmailService emailService, IInvoiceService invoiceService, ICashRegisterService cashRegisterService)
        {
            _context = context;
            _emailService = emailService;
            _invoiceService = invoiceService;
            _cashRegisterService = cashRegisterService;
        }

        // Siparişleri listele
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Car)
                .Include(o => o.AppUser)
                .OrderByDescending(o => o.StartDate)
                .ToListAsync();

            return View(orders);
        }

        // Siparişi onayla
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Car)
                .Include(o => o.AppUser)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            order.Status = "Onaylandı";

            foreach (var item in order.OrderItems)
            {
                item.Car.IsAvailable = false;
            }

            await _context.SaveChangesAsync();

            // Kasa hareketi oluştur
            var transaction = new CashRegister
            {
                Amount = order.TotalPrice, // Sipariş toplam tutarı (decimal)
                Type = "Gelir",
                Description = $"Araç kiralama - Sipariş No: {order.Id}",
                CreatedAt = DateTime.Now,
                CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                RelatedEntityType = "Order",
                RelatedEntityId = order.Id
            };

            await _cashRegisterService.AddTransactionAsync(transaction);

            // Fatura ve mail işlemleri
            var invoicePdf = _invoiceService.GenerateInvoicePdf(order);
            await _emailService.SendOrderApprovedEmail(order.AppUser.Email!, order.AppUser.UserName!, invoicePdf);

            TempData["SuccessMessage"] = "Sipariş onaylandı, kasa hareketi kaydedildi ve kullanıcıya mail gönderildi.";

            return RedirectToAction(nameof(Index));
        }

        // Siparişi reddet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var order = await _context.Orders
                .Include(o => o.AppUser)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            order.Status = "Reddedildi";

            await _context.SaveChangesAsync();

            // Mail gönderiyoruz, reddedildi bilgisini iletiyoruz
            await _emailService.SendOrderRejectedEmail(order.AppUser.Email!, order.AppUser.UserName!);

            TempData["SuccessMessage"] = "Sipariş reddedildi ve kullanıcıya mail gönderildi.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDeliveryStatus(int id, string status)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Car)
                .Include(o => o.AppUser)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            if (!Enum.TryParse<DeliveryStatus>(status, out var deliveryStatus))
            {
                ModelState.AddModelError("", "Geçersiz teslim durumu.");
                return RedirectToAction("Index");
            }

            order.DeliveryStatus = deliveryStatus;
            await _context.SaveChangesAsync();

            // Eğer teslim edilmedi ise mail gönder
            if (deliveryStatus == DeliveryStatus.NotDelivered)
            {
                int lateDays = (DateTime.Now.Date - order.EndDate.Date).Days;
                if (lateDays < 0) lateDays = 0;

                decimal penaltyPerDay = 0;
                foreach (var item in order.OrderItems)
                {
                    penaltyPerDay = item.Car.DailyPrice * 2;
                    break; // İstersen tüm araçlar için toplam da yapabilirsin
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
        Emre Galeri";

                await _emailService.SendEmailAsync(order.AppUser.Email!, "Araç Teslim Uyarısı", mailBody);
            }

            return RedirectToAction("Index");
        }






    }
}
