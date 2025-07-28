using EmreGaleriApp.Web.Areas.Admin.Models;
using EmreGaleriApp.Repository.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // ToListAsync için gerekli

namespace EmreGaleriApp.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Yonetici")]
    [Area("Admin")]
    public class RoleController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public RoleController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Rol listesini getir
        public async Task<IActionResult> RoleList()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }

        public IActionResult RoleAdd()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RoleAdd(RoleAddViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var existingRole = await _roleManager.FindByNameAsync(request.Name!);
            if (existingRole != null)
            {
                ModelState.AddModelError("Name", "Bu rol zaten mevcut.");
                return View(request);
            }

            var newRole = new AppRole
            {
                Name = request.Name
            };

            var result = await _roleManager.CreateAsync(newRole);

            if (result.Succeeded)
            {
                return RedirectToAction("RoleList");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(request);
            }
        }

        public async Task<IActionResult> RoleUpdate(string id)
        {

            if (string.IsNullOrEmpty(id))
                return NotFound();

            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
                return NotFound();

            var model = new RoleUpdateViewModel
            {
                Id = role.Id,
                Name = role.Name
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RoleUpdate(RoleUpdateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var role = await _roleManager.FindByIdAsync(model.Id!);
            if (role == null)
                return NotFound();

            // Aynı isimde başka bir rol var mı kontrolü yapabiliriz
            var existingRole = await _roleManager.FindByNameAsync(model.Name!);
            if (existingRole != null && existingRole.Id != model.Id)
            {
                ModelState.AddModelError("Name", "Bu isimde başka bir rol zaten var.");
                return View(model);
            }

            role.Name = model.Name; 
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
                return RedirectToAction("RoleList");

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RoleDelete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound();

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
                return RedirectToAction("RoleList");

            // Eğer hata varsa hata mesajlarını ModelState'e ekleyip geri dönebiliriz veya başka bir sayfaya yönlendirebiliriz.
            TempData["Error"] = "Rol silinirken hata oluştu.";
            return RedirectToAction("RoleList");
        }



    }
}
