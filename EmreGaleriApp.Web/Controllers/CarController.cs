using EmreGaleriApp.Repository.Models;
using EmreGaleriApp.Core.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using EmreGaleriApp.Core.Enums;
using System.Security.Claims;



namespace EmreGaleriApp.Web.Controllers
{
    public class CarController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CarController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index(string? gearType, string? fuelType, string? color)
        {
            var carsQuery = _context.Cars
                .Where(c => c.IsAvailable)
                .AsQueryable();

            if (!string.IsNullOrEmpty(gearType) && Enum.TryParse<GearType>(gearType, out var parsedGear))
            {
                carsQuery = carsQuery.Where(c => c.GearType == parsedGear);
            }

            if (!string.IsNullOrEmpty(fuelType))
            {
                carsQuery = carsQuery.Where(c => c.FuelType == fuelType);
            }

            if (!string.IsNullOrEmpty(color))
            {
                carsQuery = carsQuery.Where(c => c.Color == color);
            }

            var cars = await carsQuery.ToListAsync();

            // DropDownList için SelectList gönder
            ViewBag.GearTypes = new SelectList(Enum.GetValues(typeof(GearType)).Cast<GearType>()
                .Select(gt => new { Value = gt.ToString(), Text = gt == GearType.YariOtomatik ? "Yarı Otomatik" : gt.ToString() }),
                "Value", "Text", gearType);

            ViewBag.FuelTypes = new SelectList(
                new[] { "Benzin", "Dizel", "LPG", "Elektrik" },
                fuelType
            );

            ViewBag.Colors = new SelectList(
                new[] { "Siyah", "Beyaz", "Gri", "Kırmızı", "Mavi" },
                color
            );

            return View(cars);
        }



        public IActionResult AllCars()
        {
            var cars = _context.Cars.ToList();
            return View(cars);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            bool hasReviewed = false;
            if (userId != null)
            {
                hasReviewed = await _context.CarReviews
                    .AnyAsync(r => r.CarId == id && r.UserId == userId);
            }

            ViewBag.HasReviewed = hasReviewed;


            var car = await _context.Cars
                .Include(c => c.CarLicenseTypes!)
                    .ThenInclude(clt => clt.LicenseType)
                .Include(c => c.CarReviews)  // Yorumları da dahil ediyoruz
                    .ThenInclude(cr => cr.User) // Yorum yapan kullanıcı bilgisi
                .FirstOrDefaultAsync(c => c.Id == id);

            if (car == null)
                return NotFound();

            // Ortalama puan hesapla (opsiyonel)
            double averageRating = 0;
            if (car.CarReviews.Any())
            {
                averageRating = car.CarReviews.Average(r => r.Rating);
            }
            ViewBag.AverageRating = averageRating;

            return View(car);
        }


        [Authorize(Roles = "Yonetici,Yetkili")]
        public async Task<IActionResult> Create()
        {
            ViewBag.FuelTypes = new List<string> { "Benzin", "Dizel", "LPG", "Elektrik" };
            ViewBag.Colors = new List<string> { "Siyah", "Beyaz", "Gri", "Kırmızı", "Mavi" };

            ViewBag.GearTypes = Enum.GetValues(typeof(GearType))
                       .Cast<GearType>()
                       .Select(g => new SelectListItem
                       {
                           Text = g == GearType.YariOtomatik ? "Yarı Otomatik" : g.ToString(),
                           Value = ((int)g).ToString()
                       }).ToList();

            var model = new CarEditViewModel
            {
                LicenseTypeList = await _context.LicenseTypes
                    .Select(lt => new SelectListItem
                    {
                        Text = lt.Name,
                        Value = lt.Id.ToString()
                    }).ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Yonetici,Yetkili")]
        public async Task<IActionResult> Create(CarEditViewModel viewModel)
        {
            ViewBag.FuelTypes = new List<string> { "Benzin", "Dizel", "LPG", "Elektrik" };
            ViewBag.Colors = new List<string> { "Siyah", "Beyaz", "Gri", "Kırmızı", "Mavi" };

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

                foreach (var error in errors)
                {
                    Console.WriteLine($"ModelState Error: {error}");
                }

                viewModel.LicenseTypeList = await _context.LicenseTypes
                    .Select(lt => new SelectListItem
                    {
                        Text = lt.Name,
                        Value = lt.Id.ToString()
                    }).ToListAsync();

                return View(viewModel);
            }

            var car = new Car
            {
                Brand = viewModel.Brand!,
                Model = viewModel.Model!,
                Description = viewModel.Description!,
                DailyPrice = viewModel.DailyPrice,
                FuelType = viewModel.FuelType!,
                Color = viewModel.Color!,
                ModelYear = viewModel.ModelYear,
                Mileage = viewModel.Mileage,
                IsAvailable = true, // Varsayılan
                ImageUrl = null!,
                GearType = viewModel.GearType  // Burada eklendi
            };

            if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(viewModel.ImageFile.FileName);
                var filePath = Path.Combine(_environment.WebRootPath, "images", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await viewModel.ImageFile.CopyToAsync(stream);
                }

                car.ImageUrl = "/images/" + fileName;
            }

            _context.Cars.Add(car);
            await _context.SaveChangesAsync();

            if (viewModel.SelectedLicenseTypeIds != null && viewModel.SelectedLicenseTypeIds.Count > 0)
            {
                var carLicenses = viewModel.SelectedLicenseTypeIds.Select(id => new CarLicenseType
                {
                    CarId = car.Id,
                    LicenseTypeId = id
                });

                _context.CarLicenseTypes.AddRange(carLicenses);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(AllCars));
        }


        [Authorize(Roles = "Yonetici,Yetkili")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var car = await _context.Cars.FirstOrDefaultAsync(m => m.Id == id);
            if (car == null) return NotFound();

            return View(car);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Yonetici,Yetkili")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car != null)
            {
                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("AllCars", "Car");
        }

        [HttpGet]
        [Authorize(Roles = "Yonetici,Yetkili")]
        public async Task<IActionResult> Edit(int id)
        {
            var car = await _context.Cars
                .Include(c => c.CarLicenseTypes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (car == null)
                return NotFound();

            var model = new CarEditViewModel
            {
                Id = car.Id,
                Brand = car.Brand,
                Model = car.Model,
                Description = car.Description,
                DailyPrice = car.DailyPrice,
                FuelType = car.FuelType,
                Color = car.Color,
                ModelYear = car.ModelYear,
                Mileage = car.Mileage,
                ImageUrl = car.ImageUrl,
                SelectedLicenseTypeIds = car.CarLicenseTypes!.Select(clt => clt.LicenseTypeId).ToList(),
                GearType = car.GearType  // Burada eklendi
            };

            model.LicenseTypeList = await _context.LicenseTypes
                .Select(lt => new SelectListItem
                {
                    Text = lt.Name,
                    Value = lt.Id.ToString(),
                    Selected = model.SelectedLicenseTypeIds.Contains(lt.Id)
                }).ToListAsync();

            ViewBag.FuelTypes = new List<string> { "Benzin", "Dizel", "LPG", "Elektrik" };
            ViewBag.Colors = new List<string> { "Siyah", "Beyaz", "Gri", "Kırmızı", "Mavi" };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Yonetici,Yetkili")]
        public async Task<IActionResult> Edit(CarEditViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.LicenseTypeList = await _context.LicenseTypes
                    .Select(lt => new SelectListItem
                    {
                        Text = lt.Name,
                        Value = lt.Id.ToString(),
                        Selected = viewModel.SelectedLicenseTypeIds.Contains(lt.Id)
                    }).ToListAsync();

                ViewBag.FuelTypes = new List<string> { "Benzin", "Dizel", "LPG", "Elektrik" };
                ViewBag.Colors = new List<string> { "Siyah", "Beyaz", "Gri", "Kırmızı", "Mavi" };
                return View(viewModel);
            }

            var car = await _context.Cars
                .Include(c => c.CarLicenseTypes)
                .FirstOrDefaultAsync(c => c.Id == viewModel.Id);

            if (car == null)
                return NotFound();

            car.Brand = viewModel.Brand!;
            car.Model = viewModel.Model!;
            car.Description = viewModel.Description!;
            car.DailyPrice = viewModel.DailyPrice;
            car.FuelType = viewModel.FuelType!;
            car.Color = viewModel.Color!;
            car.ModelYear = viewModel.ModelYear;
            car.Mileage = viewModel.Mileage;
            car.GearType = viewModel.GearType;  // Burada eklendi

            if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(viewModel.ImageFile.FileName);
                var filePath = Path.Combine(_environment.WebRootPath, "images", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await viewModel.ImageFile.CopyToAsync(stream);
                }

                car.ImageUrl = "/images/" + fileName;
            }

            _context.CarLicenseTypes.RemoveRange(car.CarLicenseTypes!);

            car.CarLicenseTypes = viewModel.SelectedLicenseTypeIds
                .Select(id => new CarLicenseType
                {
                    CarId = car.Id,
                    LicenseTypeId = id
                }).ToList();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Güncelleme hatası: {ex.Message}");

                viewModel.LicenseTypeList = await _context.LicenseTypes
                    .Select(lt => new SelectListItem
                    {
                        Text = lt.Name,
                        Value = lt.Id.ToString(),
                        Selected = viewModel.SelectedLicenseTypeIds.Contains(lt.Id)
                    }).ToListAsync();

                ViewBag.FuelTypes = new List<string> { "Benzin", "Dizel", "LPG", "Elektrik" };
                ViewBag.Colors = new List<string> { "Siyah", "Beyaz", "Gri", "Kırmızı", "Mavi" };
                return View(viewModel);
            }

            return RedirectToAction(nameof(AllCars));
        }
    }
}
