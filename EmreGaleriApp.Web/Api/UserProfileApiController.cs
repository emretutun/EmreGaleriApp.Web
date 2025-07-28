using EmreGaleriApp.Repository.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.IO;
using EmreGaleriApp.Web.ApiDto;

namespace EmreGaleriApp.Web.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserProfileApiController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public UserProfileApiController(UserManager<AppUser> userManager, AppDbContext context, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _context = context;
            _env = env;
        }

        // ✅ Kullanıcı profilini getir
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.Users
                .Include(u => u.AppUserLicenses)
                    .ThenInclude(ul => ul.LicenseType)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();

            var profileDto = new UserProfileDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                NationalId = user.NationalId,
                Gender = user.Gender,
                BirthDate = user.BirthDate,
                DrivingExperienceYears = user.DrivingExperienceYears,
                PictureUrl = user.PictureUrl,
                LicenseTypes = user.AppUserLicenses
                    .Select(l => new UserLicenseTypeDto
                    {
                        Id = l.LicenseType.Id,
                        Name = l.LicenseType.Name
                    })
                    .ToList()
            };

            return Ok(profileDto);
        }

        // ✅ Kullanıcı profilini güncelle
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileUpdateDto updateDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.Users
                .Include(u => u.AppUserLicenses)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();

            // 📌 Temel alanlar
            if (!string.IsNullOrWhiteSpace(updateDto.UserName)) user.UserName = updateDto.UserName;
            if (!string.IsNullOrWhiteSpace(updateDto.Email)) user.Email = updateDto.Email;

            user.NationalId = updateDto.NationalId;
            user.Gender = updateDto.Gender;
            user.BirthDate = updateDto.BirthDate;
            user.DrivingExperienceYears = updateDto.DrivingExperienceYears;

            if (!string.IsNullOrEmpty(updateDto.PictureUrl))
                user.PictureUrl = Path.GetFileName(updateDto.PictureUrl);

            // 📌 Ehliyet güncellemesi
            if (updateDto.LicenseTypeIds is not null)
            {
                var validIds = await _context.LicenseTypes
                    .Where(l => updateDto.LicenseTypeIds.Contains(l.Id))
                    .Select(l => l.Id)
                    .ToListAsync();

                // Eski ilişkileri sil
                var existingLicenses = await _context.AppUserLicenses
                    .Where(x => x.AppUserId == user.Id)
                    .ToListAsync();

                _context.AppUserLicenses.RemoveRange(existingLicenses);

                // Yeni ilişkileri ekle
                foreach (var licenseId in validIds)
                {
                    _context.AppUserLicenses.Add(new AppUserLicense
                    {
                        AppUserId = user.Id,
                        LicenseTypeId = licenseId
                    });
                }
            }

            // 🧠 Kullanıcı bilgilerini güncelle
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ✅ Kullanıcı düzenleme ekranı için tüm verileri getir
        [HttpGet("edit-data")]
        public async Task<IActionResult> GetEditProfileData()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.Users
                .Include(u => u.AppUserLicenses)
                    .ThenInclude(ul => ul.LicenseType)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();

            var userProfile = new UserProfileDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                NationalId = user.NationalId,
                Gender = user.Gender,
                BirthDate = user.BirthDate,
                DrivingExperienceYears = user.DrivingExperienceYears,
                PictureUrl = user.PictureUrl,
                LicenseTypes = user.AppUserLicenses
                    .Select(l => new UserLicenseTypeDto
                    {
                        Id = l.LicenseType.Id,
                        Name = l.LicenseType.Name
                    })
                    .ToList()
            };

            var allLicenseTypes = await _context.LicenseTypes
                .Select(l => new UserLicenseTypeDto
                {
                    Id = l.Id,
                    Name = l.Name
                })
                .ToListAsync();

            return Ok(new
            {
                profile = userProfile,
                allLicenseTypes = allLicenseTypes
            });
        }

        // ✅ Resim yükleme
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Resim dosyası gönderilmedi.");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "userpictures");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            var imageUrl = $"/userpictures/{fileName}";
            return Ok(new { imageUrl });
        }
    }

}
