using EmreGaleriApp.Repository.Models;
using EmreGaleriApp.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmreGaleriApp.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Yonetici,Yetkili")]
    public class ReminderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ReminderController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Tüm notlar (sadece giriş yapan kişinin)
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var reminders = await _context.Reminders
                .Where(r => r.AppUserId == user.Id)
                .OrderBy(r => r.ReminderDate)
                .ToListAsync();

            return View(reminders);
        }

        // Yeni not oluşturma GET
        public IActionResult Create()
        {
            return View();
        }

        // Yeni not oluşturma POST
        [HttpPost]
        public async Task<IActionResult> Create(ReminderViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);

            var reminder = new Reminder
            {
                Title = model.Title,
                Description = model.Description,
                ReminderDate = model.ReminderDate,
                AppUserId = user.Id
            };

            _context.Reminders.Add(reminder);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Notu tamamlandı olarak işaretle
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var reminder = await _context.Reminders.FirstOrDefaultAsync(r => r.Id == id && r.AppUserId == user.Id);
            if (reminder == null) return NotFound();

            reminder.IsCompleted = !reminder.IsCompleted;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Sil
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var reminder = await _context.Reminders.FirstOrDefaultAsync(r => r.Id == id && r.AppUserId == user.Id);
            if (reminder == null) return NotFound();

            _context.Reminders.Remove(reminder);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
