using EmreGaleriApp.Repository.Models;
using EmreGaleriApp.Core.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using EmreGaleriApp.Web.ApiDto;

namespace EmreGaleriApp.Web.Api
{
    [Route("api/cars")]
    [ApiController]
    public class CarApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CarApiController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public AppDbContext Get_context()
        {
            return _context;
        }

        // Tüm araçları detaylı getir (LicenseTypes dahil)
        [HttpGet]
        public async Task<ActionResult<List<CarDetailDto>>> GetCars(AppDbContext _context)
        {
            var cars = await _context.Cars
                .Include(c => c.CarLicenseTypes!)
                    .ThenInclude(clt => clt.LicenseType)
                .ToListAsync();

            var listDto = cars.Select(car => new CarDetailDto
            {
                Id = car.Id,
                Brand = car.Brand,
                Model = car.Model,
                ModelYear = car.ModelYear,
                DailyPrice = (double)car.DailyPrice,
                Description = car.Description,
                FuelType = car.FuelType,
                Mileage = car.Mileage,
                GearType = (int)car.GearType!,
                Color = car.Color,
                ImageUrl = car.ImageUrl,
                LicenseTypes = car.CarLicenseTypes!.Select(clt => new LicenseTypeDto
                {
                    Id = clt.LicenseType!.Id,
                    Name = clt.LicenseType.Name
                }).ToList()
            }).ToList();

            return Ok(listDto);
        }

        // Tek araç detay
        [HttpGet("{id}")]
        public async Task<ActionResult<CarDetailDto>> GetCar(int id)
        {
            var car = await _context.Cars
                .Include(c => c.CarLicenseTypes!)
                    .ThenInclude(clt => clt.LicenseType)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (car == null)
                return NotFound();

            var dto = new CarDetailDto
            {
                Id = car.Id,
                Brand = car.Brand,
                Model = car.Model,
                ModelYear = car.ModelYear,
                DailyPrice = (double)car.DailyPrice,
                Description = car.Description,
                FuelType = car.FuelType,
                Mileage = car.Mileage,
                GearType = (int)car.GearType!,
                Color = car.Color,
                ImageUrl = car.ImageUrl,
                LicenseTypes = car.CarLicenseTypes!.Select(clt => new LicenseTypeDto
                {
                    Id = clt.LicenseType!.Id,
                    Name = clt.LicenseType.Name
                }).ToList()
            };

            return Ok(dto);
        }

        // Araç güncelle
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCar(int id, [FromBody] CarUpdateDto dto)
        {
            var car = await _context.Cars
                .Include(c => c.CarLicenseTypes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (car == null)
                return NotFound();

            car.Brand = dto.Brand!;
            car.Model = dto.Model!;
            car.ModelYear = dto.ModelYear;
            car.DailyPrice = Convert.ToDecimal(dto.DailyPrice);
            car.Description = dto.Description!;
            car.FuelType = dto.FuelType!;
            car.Mileage = dto.Mileage;
            car.GearType = (GearType)dto.GearType;
            car.Color = dto.Color!;
            car.ImageUrl = dto.ImageUrl ?? car.ImageUrl;

            // Ehliyet tiplerini güncelle
            car.CarLicenseTypes!.Clear();
            foreach (var licenseId in dto.LicenseTypeIds!)
            {
                car.CarLicenseTypes.Add(new CarLicenseType
                {
                    LicenseTypeId = licenseId,
                    CarId = car.Id
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Araç başarıyla güncellendi." });
        }

        // Araç sil
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCar(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
                return NotFound();

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Araç başarıyla silindi." });
        }

        // Resim yükleme API
        [HttpPost("/api/upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Resim dosyası gönderilmedi.");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "images");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            var imageUrl = $"/images/{fileName}";
            return Ok(new { imageUrl });
        }

        // Yeni araç ekle
        [HttpPost]
        public async Task<IActionResult> AddCar([FromBody] CarCreateDto carDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var car = new Car
            {
                Brand = carDto.Brand!,
                Model = carDto.Model!,
                ModelYear = carDto.ModelYear,
                DailyPrice = Convert.ToDecimal(carDto.DailyPrice),
                Description = carDto.Description!,
                ImageUrl = carDto.ImageUrl!,
                FuelType = carDto.FuelType!,
                Mileage = carDto.Mileage,
                GearType = (GearType)carDto.GearType,
                Color = carDto.Color!,
            };

            _context.Cars.Add(car);
            await _context.SaveChangesAsync();

            foreach (var licenseTypeId in carDto.LicenseTypeIds!)
            {
                _context.CarLicenseTypes.Add(new CarLicenseType
                {
                    CarId = car.Id,
                    LicenseTypeId = licenseTypeId
                });
            }
            await _context.SaveChangesAsync();

            return Ok(new { message = "Araç başarıyla eklendi." });
        }
    }

    // LicenseTypes API - Tüm ehliyet türlerini getir
    [Route("api/license-types")]
    [ApiController]
    public class LicenseTypeApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LicenseTypeApiController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<LicenseTypeDto>>> GetLicenseTypes()
        {
            var list = await _context.LicenseTypes.Select(lt => new LicenseTypeDto
            {
                Id = lt.Id,
                Name = lt.Name
            }).ToListAsync();

            return Ok(list);
        }
    }




}
