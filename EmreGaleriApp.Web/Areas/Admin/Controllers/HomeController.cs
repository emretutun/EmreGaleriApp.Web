using EmreGaleriApp.Web.Areas.Admin.Models;
using EmreGaleriApp.Repository.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmreGaleriApp.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Yonetici,Yetkili")]
    [Area("Admin")]
    public class HomeController : Controller
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _appDbContext;

        public HomeController(UserManager<AppUser> userManager, AppDbContext appDbContext)
        {
            _userManager = userManager;
            _appDbContext = appDbContext;
        }

        public DateTime CreateDate { get; set; } = DateTime.Now;

        public async Task<IActionResult> Index()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            ViewBag.TotalUsers = totalUsers;

            var totalCars = await _appDbContext.Cars.CountAsync();
            ViewBag.TotalCars = totalCars;

             ViewBag.RentedCarsCount = _appDbContext.Orders
           .Where(o => o.Status == "Onaylandı" && o.EndDate >= DateTime.Now)
           .Select(o => o.Id)
           .Distinct()
           .Count();

            // Kirada olmayan araç sayısı
            ViewBag.AvailableCarsCount = ViewBag.TotalCars - ViewBag.RentedCarsCount;

            return View();
        }

        public async Task<IActionResult> UserList()
        {
            var userList = await _userManager.Users
                .Include(u => u.AppUserLicenses)
                .ThenInclude(ul => ul.LicenseType)
                .ToListAsync();

            var userViewModelList = new List<UserViewModel>();

            foreach (var user in userList)
            {
                var roles = await _userManager.GetRolesAsync(user);

                var vm = new UserViewModel()
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
                    Roles = roles.ToList()
                };

                userViewModelList.Add(vm);
            }

            return View(userViewModelList);
        }

    }
}
