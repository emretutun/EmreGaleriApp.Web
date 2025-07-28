using Microsoft.AspNetCore.Mvc;
using EmreGaleriApp.Repository.Models;
using EmreGaleriApp.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace EmreGaleriApp.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Yonetici,Yetkili")]
    public class PersonelController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public PersonelController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Listeleme
        public async Task<IActionResult> Index()
        {
            var personeller = await _context.PersonelDetails.Include(p => p.User).ToListAsync();
            return View(personeller);
        }

        // Detay
        public async Task<IActionResult> Details(int id)
        {
            var personel = await _context.PersonelDetails
                .Include(p => p.User) // User ilişkisini yüklüyoruz
                .FirstOrDefaultAsync(p => p.Id == id);

            if (personel == null)
                return NotFound();

            return View(personel);
        }

        // Create GET
        public async Task<IActionResult> Create()
        {
            // Sadece "Yetkili" rolündeki kullanıcıları al ve personel tablosunda olmayanları filtrele
            var yetkililer = await _userManager.GetUsersInRoleAsync("Yetkili");

            var personelOlmayanlar = yetkililer
                .Where(u => !_context.PersonelDetails.Any(p => p.UserId == u.Id))
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.UserName
                }).ToList();

            ViewBag.UserList = personelOlmayanlar;

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PersonelDetail personel)
        {
            Console.WriteLine($"Gelen UserId: {personel.UserId}, Maaş: {personel.Salary}, Pozisyon: {personel.Position}");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.PersonelDetails.Add(personel);
                    var saved = await _context.SaveChangesAsync();
                    Console.WriteLine($"SaveChanges sonucu: {saved}");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Kayıt sırasında hata oluştu: " + ex.Message);
                }
            }
            else
            {
                // ModelState'deki tüm hataları al ve konsola yazdır
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();

                foreach (var error in errors)
                {
                    Console.WriteLine("ModelState Hatası: " + error);
                }
                // Eğer istersen bu hataları ViewBag ile view'a da gönder
                ViewBag.Errors = errors;
            }

            // Kullanıcı listesini tekrar yükle ki dropdown dolsun
            var users = _userManager.GetUsersInRoleAsync("Yetkili").Result
                        .Where(u => !_context.PersonelDetails.Any(p => p.UserId == u.Id))
                        .Select(u => new SelectListItem { Value = u.Id, Text = u.UserName })
                        .ToList();

            ViewBag.UserList = users;

            return View(personel);
        }


        // Edit GET
        public async Task<IActionResult> Edit(int id)
        {
            var personel = await _context.PersonelDetails.FindAsync(id);
            if (personel == null) return NotFound();

            // Tüm Yetkili kullanıcıları getiriyoruz, dropdown için
            var yetkililer = await _userManager.GetUsersInRoleAsync("Yetkili");
            var userList = yetkililer.Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = u.UserName
            }).ToList();

            ViewBag.UserList = userList;

            return View(personel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PersonelDetail personel)
        {
            if (id != personel.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                var existingPersonel = await _context.PersonelDetails.FindAsync(id);
                if (existingPersonel == null)
                    return NotFound();

                // Alanları güncelle
                existingPersonel.UserId = personel.UserId;
                existingPersonel.Position = personel.Position;
                existingPersonel.Salary = personel.Salary;
                existingPersonel.StartDate = personel.StartDate;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Eğer validation başarısızsa dropdown'u tekrar yükle
            var yetkililer = await _userManager.GetUsersInRoleAsync("Yetkili");
            ViewBag.UserList = yetkililer.Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = u.UserName
            }).ToList();

            return View(personel);
        }


        // Delete GET
        public async Task<IActionResult> Delete(int id)
        {
            var personel = await _context.PersonelDetails.Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (personel == null) return NotFound();

            return View(personel);
        }


        // Delete POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var personel = await _context.PersonelDetails.FindAsync(id);
            if (personel == null) return NotFound();

            _context.PersonelDetails.Remove(personel);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PaySalary(int id, int monthCount = 1)
        {
            var personel = await _context.PersonelDetails.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);
            if (personel == null) return NotFound();

            decimal totalSalary = personel.Salary * monthCount;
            var userName = personel.User?.UserName ?? "Bilinmeyen Kullanıcı";

            // Şu anki giriş yapmış kullanıcının Id'sini al
            var currentUserId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var currentMonthName = DateTime.Now.ToString("MMMM", new System.Globalization.CultureInfo("tr-TR"));

            var cashEntry = new CashRegister
            {
                Amount = -totalSalary,
                Description = $"{currentMonthName} ayı Maaş Ödemesi - {personel.Position} - {userName}",
                Type = "Gider",
                CreatedByUserId = _userManager.GetUserId(User!)
            };


            _context.CashRegisters.Add(cashEntry);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{currentMonthName} ayı Maaş Ödemesi - {personel.Position} - {userName}";

            return RedirectToAction(nameof(Details), new { id });
        }





    }
}
