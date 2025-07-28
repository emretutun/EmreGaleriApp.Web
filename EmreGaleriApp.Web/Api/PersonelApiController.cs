using Microsoft.AspNetCore.Mvc;
using EmreGaleriApp.Repository.Models;
using EmreGaleriApp.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using EmreGaleriApp.Web.ApiDto;

namespace EmreGaleriApp.Web.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Yonetici,Yetkili")]
    public class PersonelApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public PersonelApiController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/personelapi
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.PersonelDetails
                   .Include(p => p.User)
                   .Select(p => new
                   {
                       p.Id,
                       p.UserId,
                       UserName = p.User!.UserName,
                       Email = p.User.Email,
                       PhoneNumber = p.User.PhoneNumber,
                       p.Position,
                       p.Salary,
                       p.StartDate
                   })
                   .ToListAsync();

            return Ok(data);
        }

        // GET: api/personelapi/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _context.PersonelDetails
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (p == null) return NotFound();

            return Ok(new
            {
                p.Id,
                p.UserId,
                UserName = p.User?.UserName,
                p.Position,
                p.Salary,
                p.StartDate
            });
        }

        // POST: api/personelapi
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PersonelCreateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // UserId geçerli mi kontrol et
            var userExists = await _userManager.FindByIdAsync(model.UserId);
            if (userExists == null)
                return BadRequest("Verilen kullanıcı bulunamadı.");

            var newPersonel = new PersonelDetail
            {
                UserId = model.UserId,
                Position = model.Position,
                Salary = model.Salary,
                StartDate = model.StartDate
            };

            _context.PersonelDetails.Add(newPersonel);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = newPersonel.Id }, newPersonel);
        }

        // PUT: api/personelapi/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PersonelUpdateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != model.Id)
                return BadRequest("Id uyuşmuyor.");

            var existing = await _context.PersonelDetails.FindAsync(id);
            if (existing == null)
                return NotFound();

            var userExists = await _userManager.FindByIdAsync(model.UserId);
            if (userExists == null)
                return BadRequest("Verilen kullanıcı bulunamadı.");

            existing.UserId = model.UserId;
            existing.Position = model.Position;
            existing.Salary = model.Salary;
            existing.StartDate = model.StartDate;

            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        // DELETE: api/personelapi/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _context.PersonelDetails.FindAsync(id);
            if (p == null) return NotFound();

            _context.PersonelDetails.Remove(p);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/personelapi/{id}/pay
        [HttpPost("{id}/pay")]
        public async Task<IActionResult> PaySalary(int id, [FromQuery] int monthCount = 1)
        {
            var personel = await _context.PersonelDetails.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);
            if (personel == null)
                return NotFound();

            decimal totalSalary = personel.Salary * monthCount;
            var userName = personel.User?.UserName ?? "Bilinmeyen Kullanıcı";

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized("Kullanıcı bilgisi alınamadı.");

            var currentUser = await _userManager.FindByIdAsync(adminId);
            var currentUserName = currentUser?.UserName ?? "Bilinmeyen";

            var currentMonthName = DateTime.Now.ToString("MMMM", new System.Globalization.CultureInfo("tr-TR"));

            var cashEntry = new CashRegister
            {
                Amount = -totalSalary,
                Description = $"{currentMonthName} ayı Maaş Ödemesi - {personel.Position} - {userName}",
                Type = "Gider",
                CreatedByUserId = adminId
            };

            _context.CashRegisters.Add(cashEntry);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"{currentMonthName} maaş ödemesi başarılı şekilde yapıldı.",
                amount = totalSalary,
                paidBy = currentUserName
            });
        }
    }

    // DTO Sınıfları
    public class PersonelCreateDto
    {
        public string UserId { get; set; } = null!;
        public string Position { get; set; } = null!;
        public decimal Salary { get; set; }
        public DateTime StartDate { get; set; }
    }


}
