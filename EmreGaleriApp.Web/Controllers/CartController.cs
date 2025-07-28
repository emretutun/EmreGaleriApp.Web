using EmreGaleriApp.Repository.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using EmreGaleriApp.Core.ViewModels;

namespace EmreGaleriApp.Web.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        // 🔸 Sepet Görüntüleme
        public IActionResult Index()
        {
            var cart = GetCartFromSession();
            return View(cart);
        }

        // 🔸 Sepete Araç Ekleme (GET)
        [HttpGet]
        public IActionResult AddToCart(int id)
        {
            var car = _context.Cars
                .Include(c => c.CarLicenseTypes)
                .FirstOrDefault(c => c.Id == id && c.IsAvailable);

            if (car == null)
            {
                return Json(new { success = false, message = "Araç bulunamadı veya müsait değil." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userLicenseIds = _context.AppUserLicenses
                .Where(l => l.AppUserId == userId)
                .Select(l => l.LicenseTypeId)
                .ToList();

            var carLicenseIds = car.CarLicenseTypes.Select(c => c.LicenseTypeId).ToList();

            bool hasRequiredLicense = carLicenseIds.Count == 0 || carLicenseIds.Intersect(userLicenseIds).Any();

            if (!hasRequiredLicense)
            {
                return Json(new { success = false, message = "Bu aracı kiralayacak ehliyet sınıfına sahip değilsiniz." });
            }

            var cart = GetCartFromSession();

            if (!cart.Any(ci => ci.CarId == car.Id))
            {
                cart.Add(new CartItem
                {
                    CarId = car.Id,
                    CarName = $"{car.Brand} {car.Model}",
                    ImageUrl = car.ImageUrl,
                    DailyPrice = car.DailyPrice
                });

                SaveCartToSession(cart);
            }

            return Json(new { success = true, message = "Araç sepete eklendi." });
        }

        // 🔸 Sepetten Araç Silme
        public IActionResult RemoveFromCart(int id)
        {
            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(c => c.CarId == id);

            if (item != null)
            {
                cart.Remove(item);
                SaveCartToSession(cart);
            }

            return RedirectToAction("Index");
        }

        // 🔸 Siparişi Onayla
        [HttpPost]
        public async Task<IActionResult> ConfirmOrder(DateTime StartDate, DateTime EndDate)
        {
            var cart = GetCartFromSession();

            if (StartDate >= EndDate)
            {
                ModelState.AddModelError("", "Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
                return View("Index", cart);
            }

            if (!cart.Any())
            {
                ModelState.AddModelError("", "Sepetiniz boş.");
                return View("Index", cart);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userLicenseIds = _context.AppUserLicenses
                .Where(ul => ul.AppUserId == userId)
                .Select(ul => ul.LicenseTypeId)
                .ToList();

            var carIds = cart.Select(c => c.CarId).ToList();
            var cars = await _context.Cars
                .Include(c => c.CarLicenseTypes)
                .Where(c => carIds.Contains(c.Id))
                .ToListAsync();

            foreach (var car in cars)
            {
                var requiredLicenseIds = car.CarLicenseTypes.Select(clt => clt.LicenseTypeId).ToList();
                bool hasLicense = requiredLicenseIds.Count == 0 || requiredLicenseIds.Intersect(userLicenseIds).Any();

                if (!hasLicense)
                {
                    ModelState.AddModelError("", $"'{car.Brand} {car.Model}' aracını kiralamak için geçerli ehliyetiniz yok.");
                    return View("Index", cart);
                }
            }

            int days = (EndDate - StartDate).Days;
            if (days <= 0)
            {
                ModelState.AddModelError("", "Geçerli bir tarih aralığı giriniz.");
                return View("Index", cart);
            }

            decimal totalPrice = cart.Sum(c => c.DailyPrice * days);

            var order = new Order
            {
                AppUserId = userId,
                StartDate = StartDate,
                EndDate = EndDate,
                TotalPrice = totalPrice,
                Status = "Beklemede",
                OrderItems = cart.Select(c => new OrderItem
                {
                    CarId = c.CarId,
                    DailyPrice = c.DailyPrice
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            SaveCartToSession(new List<CartItem>()); // sepet temizleniyor

            return RedirectToAction("MyOrders", "Order");
        }

        // 🔸 Sepeti Session’dan Al
        private List<CartItem> GetCartFromSession()
        {
            var json = HttpContext.Session.GetString("Cart");
            return string.IsNullOrEmpty(json)
                ? new List<CartItem>()
                : JsonConvert.DeserializeObject<List<CartItem>>(json)!;
        }

        // 🔸 Sepeti Session’a Kaydet
        private void SaveCartToSession(List<CartItem> cart)
        {
            var json = JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString("Cart", json);
        }
    }
}
