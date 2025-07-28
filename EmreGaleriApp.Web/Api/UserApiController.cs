using EmreGaleriApp.Repository.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using EmreGaleriApp.Web.ApiDto;

namespace EmreGaleriApp.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Yonetici,Yetkili", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserApiController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _dbContext;
        private readonly IWebHostEnvironment _env;

        public UserApiController(UserManager<AppUser> userManager, AppDbContext dbContext, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users
                .Include(u => u.AppUserLicenses)
                .ThenInclude(l => l.LicenseType)
                .ToListAsync();

            var result = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new
                {
                    id = user.Id,
                    name = user.UserName,
                    email = user.Email,
                    phone = user.PhoneNumber,
                    nationalId = user.NationalId,
                    birthDate = user.BirthDate,
                    gender = user.Gender,
                    experience = user.DrivingExperienceYears,
                    picture = user.PictureUrl,
                    licenseTypes = user.AppUserLicenses.Select(l => l.LicenseType.Name).ToList(),
                    roles = roles
                });
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            user.UserName = dto.Name ?? user.UserName;
            user.Email = dto.Email ?? user.Email;
            user.PhoneNumber = dto.Phone ?? user.PhoneNumber;
            user.Gender = dto.Gender ?? user.Gender;
            user.DrivingExperienceYears = dto.Experience ?? user.DrivingExperienceYears;

            if (!string.IsNullOrEmpty(dto.Picture))
            {
                user.PictureUrl = dto.Picture;
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return BadRequest(updateResult.Errors);

            return NoContent();
        }

        [HttpPost("UploadProfilePicture")]
        [RequestSizeLimit(5 * 1024 * 1024)] // max 5 MB
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Dosya seçilmedi.");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "userpictures");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { fileName });
        }

        // --- Önemli: Rol atama endpoint'i ---

        [HttpPost("{id}/AssignRoles")]
        public async Task<IActionResult> AssignRoles(string id, [FromBody] AssignRolesDto dto)
        {
            if (dto == null || dto.Roles == null)
                return BadRequest("Rol bilgisi gönderilmedi.");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);

            // Mevcut rolleri kaldır
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
                return BadRequest(removeResult.Errors);

            // Yeni rolleri ekle
            var addResult = await _userManager.AddToRolesAsync(user, dto.Roles);
            if (!addResult.Succeeded)
                return BadRequest(addResult.Errors);

            // Başarılı, içerik yok döner (204)
            return NoContent();
        }

        [HttpGet("yetkili")]
        public async Task<IActionResult> GetYetkiliUsers()
        {
            var users = await _userManager.GetUsersInRoleAsync("Yetkili");
            var result = new List<object>();

            foreach (var user in users)
            {
                result.Add(new
                {
                    id = user.Id,
                    name = user.UserName,
                    email = user.Email,
                    phone = user.PhoneNumber,
                    // İstersen başka alanlar da ekleyebilirsin
                });
            }

            return Ok(result);
        }



    }
}
