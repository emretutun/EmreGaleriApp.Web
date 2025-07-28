using EmreGaleriApp.Core.ViewModels;
using EmreGaleriApp.Web.Extensions;
using EmreGaleriApp.Repository.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EmreGaleriApp.Web.Controllers
{
    [Authorize]
    public class MemberController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IFileProvider _fileProvider;
        private readonly IWebHostEnvironment _env;
        private readonly AppDbContext _context;

        public MemberController(SignInManager<AppUser> signInManager,
                                UserManager<AppUser> userManager,
                                IFileProvider fileProvider,
                                IWebHostEnvironment env,
                                AppDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _fileProvider = fileProvider;
            _env = env;
            _context = context;
        }

        public async Task<IActionResult> Profile()
        {
            // Kullanıcıyı ve lisanslarını Include ile çekiyoruz
            var user = await _userManager.Users
                .Include(u => u.AppUserLicenses)
                    .ThenInclude(ul => ul.LicenseType)
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            if (user == null)
                return RedirectToAction("SignIn", "Home");

            // Ehliyet isimlerini liste haline getirip string'e dönüştürüyoruz
            var licenseNames = user.AppUserLicenses.Select(ul => ul.LicenseType.Name).ToList();
            ViewData["LicenseTypesDisplay"] = licenseNames.Count > 0 ? string.Join(", ", licenseNames) : "Belirtilmemiş";

            return View(user);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı bulunamadı.");
                return View(model);
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword!, model.NewPassword!);

            if (!result.Succeeded)
            {
                ModelState.AddModelErrorList(result.Errors.Select(x => x.Description).ToList());
                return View(model);
            }

            await _userManager.UpdateSecurityStampAsync(user);
            await _signInManager.SignOutAsync();
            await _signInManager.PasswordSignInAsync(user, model.NewPassword!, true, false);

            ViewBag.PasswordChanged = true;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Tüm ehliyet türlerini çek
            var allLicenseTypes = _context.LicenseTypes
                .Select(lt => new LicenseTypeViewModel
                {
                    Id = lt.Id,
                    Name = lt.Name
                })
                .ToList();

            // Kullanıcının seçtiği ehliyet türlerinin Id'leri
            var userLicenseIds = _context.AppUserLicenses
                .Where(au => au.AppUserId == user.Id)
                .Select(au => au.LicenseTypeId)
                .ToList();

            var model = new UserEditViewModel
            {
                NationalId = user.NationalId,
                Gender = user.Gender,
                BirthDate = user.BirthDate,
                DrivingExperienceYears = user.DrivingExperienceYears,
                PictureUrl = user.PictureUrl,
                LicenseTypes = allLicenseTypes,
                SelectedLicenseTypeIds = userLicenseIds
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(UserEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Model hata alırsa, LicenseTypes listesini tekrar doldur
                model.LicenseTypes = _context.LicenseTypes
                    .Select(lt => new LicenseTypeViewModel
                    {
                        Id = lt.Id,
                        Name = lt.Name
                    })
                    .ToList();

                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (model.Picture != null && model.Picture.Length > 0)
            {
                var userPicturesPath = Path.Combine(_env.WebRootPath, "userpictures");
                if (!Directory.Exists(userPicturesPath))
                    Directory.CreateDirectory(userPicturesPath);

                var randomFileName = $"{Guid.NewGuid()}{Path.GetExtension(model.Picture.FileName)}";
                var savePath = Path.Combine(userPicturesPath, randomFileName);

                using var stream = new FileStream(savePath, FileMode.Create);
                await model.Picture.CopyToAsync(stream);

                user.PictureUrl = randomFileName;
            }

            user.NationalId = model.NationalId;
            user.Gender = model.Gender;
            user.BirthDate = model.BirthDate;
            user.DrivingExperienceYears = model.DrivingExperienceYears;

            // Kullanıcının mevcut ehliyetlerini sil
            var currentUserLicenses = _context.AppUserLicenses.Where(au => au.AppUserId == user.Id);
            _context.AppUserLicenses.RemoveRange(currentUserLicenses);

            // Yeni seçilen ehliyetleri ekle
            foreach (var licenseId in model.SelectedLicenseTypeIds)
            {
                _context.AppUserLicenses.Add(new AppUserLicense
                {
                    AppUserId = user.Id,
                    LicenseTypeId = licenseId
                });
            }

            var result = await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profil başarıyla güncellendi.";
                return RedirectToAction("EditProfile");
            }

            ModelState.AddModelError("", "Güncelleme sırasında bir hata oluştu.");

            // LicenseTypes listesini tekrar doldur
            model.LicenseTypes = _context.LicenseTypes
                .Select(lt => new LicenseTypeViewModel
                {
                    Id = lt.Id,
                    Name = lt.Name
                })
                .ToList();

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
