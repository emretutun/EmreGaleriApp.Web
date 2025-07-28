using EmreGaleriApp.Web.Areas.Admin.Models;
using EmreGaleriApp.Repository.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmreGaleriApp.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Yonetici")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly AppDbContext _dbContext; // <-- Burayı kendi DbContext’in ile değiştir

        public UserController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, AppDbContext dbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
        }

        [Authorize(Roles = "Yonetici")]
        [HttpGet]
        public async Task<IActionResult> AssignRoles(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var allRoles = _roleManager.Roles.ToList();
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new AssignRolesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Roles = allRoles.Select(r => new RoleCheckboxItem
                {
                    RoleId = r.Id,
                    RoleName = r.Name!,
                    IsSelected = userRoles.Contains(r.Name!)
                }).ToList()
            };

            return View(model);
        }

        [Authorize(Roles = "Yonetici")]
        [HttpPost]
        public async Task<IActionResult> AssignRoles(AssignRolesViewModel model, string[] selectedRoles)
        {
            if (string.IsNullOrEmpty(model.UserId))
                return NotFound();

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            selectedRoles ??= Array.Empty<string>();

            var rolesToRemove = userRoles.Except(selectedRoles);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError("", "Roller güncellenirken hata oluştu (kaldırma).");
                return View(model);
            }

            var rolesToAdd = selectedRoles.Except(userRoles);
            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);

            if (!addResult.Succeeded)
            {
                ModelState.AddModelError("", "Roller güncellenirken hata oluştu (ekleme).");
                return View(model);
            }

            return RedirectToAction("UserList", "Home", new { area = "Admin" });
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.Users
                .Include(u => u.AppUserLicenses)
                    .ThenInclude(ul => ul.LicenseType)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            var model = new UserViewModel
            {
                Id = user.Id,
                Name = user.UserName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                NationalId = user.NationalId,
                BirthDate = user.BirthDate,
                Gender = user.Gender,
                DrivingExperienceYears = user.DrivingExperienceYears,
                Picture = user.PictureUrl,
                LicenseTypes = user.AppUserLicenses.Select(l => l.LicenseType.Name).ToList(),
                Roles = (await _userManager.GetRolesAsync(user)).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.Users
                .Include(u => u.AppUserLicenses)
                    .ThenInclude(ul => ul.LicenseType)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            var userLicenseTypeNames = user.AppUserLicenses.Select(l => l.LicenseType.Name).ToList();

            var allLicenseTypes = await _dbContext.Set<LicenseType>().ToListAsync();

            var licenseTypeCheckboxItems = allLicenseTypes.Select(lt => new LicenseTypeCheckboxItem
            {
                Id = lt.Id,
                Name = lt.Name,
                IsSelected = userLicenseTypeNames.Contains(lt.Name)
            }).ToList();

            var model = new UserChangeViewModel
            {
                Id = user.Id,
                Name = user.UserName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                NationalId = user.NationalId,
                BirthDate = user.BirthDate,
                Gender = user.Gender,
                DrivingExperienceYears = user.DrivingExperienceYears,
                LicenseTypes = licenseTypeCheckboxItems
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserChangeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model!.Id!);
            if (user == null)
                return NotFound();

            // Kullanıcı bilgilerini güncelle
            user.UserName = model.Name;
            user.Email = model.Email;
            user.PhoneNumber = model.Phone;
            user.NationalId = model.NationalId;
            user.BirthDate = model.BirthDate;
            user.Gender = model.Gender;
            user.DrivingExperienceYears = model.DrivingExperienceYears;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }

            // Lisansları güncelle
            var existingLicenses = _dbContext.Set<AppUserLicense>().Where(ul => ul.AppUserId == user.Id);
            var selectedLicenseIds = model.LicenseTypes.Where(lt => lt.IsSelected).Select(lt => lt.Id).ToList();

            // Seçilmeyenleri sil
            var toRemove = existingLicenses.Where(el => !selectedLicenseIds.Contains(el.LicenseTypeId));
            _dbContext.Set<AppUserLicense>().RemoveRange(toRemove);

            // Yeni eklenenleri bul
            var existingLicenseTypeIds = existingLicenses.Select(el => el.LicenseTypeId).ToList();
            var toAdd = selectedLicenseIds.Where(id => !existingLicenseTypeIds.Contains(id))
                .Select(id => new AppUserLicense
                {
                    AppUserId = user.Id,
                    LicenseTypeId = id
                });

            await _dbContext.Set<AppUserLicense>().AddRangeAsync(toAdd);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("UserList", "Home", new { area = "Admin" });
        }




        [Authorize(Roles = "Yonetici")]
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var model = new UserViewModel
            {
                Id = user.Id,
                Name = user.UserName,
                Email = user.Email,
            };

            return View(model);
        }

        [Authorize(Roles = "Yonetici")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                var model = new UserViewModel
                {
                    Id = user.Id,
                    Name = user.UserName,
                    Email = user.Email,
                };
                return View(model);
            }

            return RedirectToAction("UserList", "Home", new { area = "Admin" });
        }
    }
}
