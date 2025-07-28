using EmreGaleriApp.Repository.Models;
using EmreGaleriApp.Web.ApiDto;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EmreGaleriApp.Web.Api
{
    [Route("api/reminder")]
    [ApiController]
    [Authorize(Roles = "Yonetici,Yetkili", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ReminderApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ReminderApiController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/reminder
        [HttpGet]
        public async Task<IActionResult> GetMyReminders()
        {
            // Öncelikle NameIdentifier dene, yoksa sub claim dene
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Kullanıcı bilgisi alınamadı.");

            var reminders = await _context.Reminders
                .Where(r => r.AppUserId == userId)
                .OrderBy(r => r.ReminderDate)
                .Select(r => new
                {
                    r.Id,
                    r.Title,
                    r.Description,
                    r.ReminderDate,
                    r.IsCompleted
                })
                .ToListAsync();

            return Ok(reminders);
        }


        [HttpPost]
        public async Task<IActionResult> CreateReminder([FromBody] ReminderDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
                return BadRequest("Başlık boş olamaz.");

            // Kullanıcı Id’sini NameIdentifier veya sub claimlerinden alıyoruz
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Kullanıcı bilgisi alınamadı.");

            var reminder = new Reminder
            {
                Title = model.Title,
                Description = model.Description,
                ReminderDate = model.ReminderDate,
                IsCompleted = model.IsCompleted,
                AppUserId = userId
            };

            _context.Reminders.Add(reminder);
            await _context.SaveChangesAsync();

            model.Id = reminder.Id;
            return Ok(model);
        }


        // Hatırlatmayı tamamlandı/yapılmadı olarak işaretle toggle
        [HttpPost("toggle/{id}")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Kullanıcı bilgisi alınamadı.");

            var reminder = await _context.Reminders.FirstOrDefaultAsync(r => r.Id == id && r.AppUserId == userId);
            if (reminder == null)
                return NotFound("Hatırlatma bulunamadı.");

            reminder.IsCompleted = !reminder.IsCompleted;
            await _context.SaveChangesAsync();

            return Ok(new { reminder.Id, reminder.IsCompleted });
        }

        // Hatırlatmayı sil
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReminder(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Kullanıcı bilgisi alınamadı.");

            var reminder = await _context.Reminders.FirstOrDefaultAsync(r => r.Id == id && r.AppUserId == userId);
            if (reminder == null)
                return NotFound("Hatırlatma bulunamadı.");

            _context.Reminders.Remove(reminder);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Hatırlatma silindi." });
        }



        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("API Çalışıyor");
        }


    }
    
}
