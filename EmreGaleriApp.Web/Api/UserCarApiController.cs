using EmreGaleriApp.Repository.Models;
using EmreGaleriApp.Web.ApiDto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmreGaleriApp.Web.Api
{
    [Route("api/usercars")]
    [ApiController]
    public class UserCarApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserCarApiController(AppDbContext context)
        {
            _context = context;
        }

        // Tüm Araçlar (kirada olsa da)
        [HttpGet]
        public async Task<ActionResult<List<UserCarListDto>>> GetAllCars()
        {
            var cars = await _context.Cars.ToListAsync();

            var carDtos = cars.Select(c => new UserCarListDto
            {
                Id = c.Id,
                Brand = c.Brand,
                Model = c.Model,
                Description = c.Description,
                DailyPrice = c.DailyPrice,
                ImageUrl = c.ImageUrl,
                IsAvailable = c.IsAvailable,
                Color = c.Color,
                FuelType = c.FuelType,
                Mileage = c.Mileage,
                ModelYear = c.ModelYear,
                GearType = c.GearType?.ToString() ?? ""
            }).ToList();

            return Ok(carDtos);
        }

        // Sadece Müsait Araçlar (IsAvailable == true)
        [HttpGet("available")]
        public async Task<ActionResult<List<UserCarListDto>>> GetAvailableCars()
        {
            var cars = await _context.Cars
                .Where(c => c.IsAvailable)
                .ToListAsync();

            var carDtos = cars.Select(c => new UserCarListDto
            {
                Id = c.Id,
                Brand = c.Brand,
                Model = c.Model,
                Description = c.Description,
                DailyPrice = c.DailyPrice,
                ImageUrl = c.ImageUrl,
                IsAvailable = c.IsAvailable,
                Color = c.Color,
                FuelType = c.FuelType,
                Mileage = c.Mileage,
                ModelYear = c.ModelYear,
                GearType = c.GearType?.ToString() ?? ""
            }).ToList();

            return Ok(carDtos);
        }

        // Araç Detayları + Yorumlar + Ortalama Puan
        [HttpGet("{id}")]
        public async Task<ActionResult<UserCarDetailDto>> GetCarDetails(int id)
        {
            var car = await _context.Cars
                .Include(c => c.CarLicenseTypes!)
                    .ThenInclude(clt => clt.LicenseType)
                .Include(c => c.CarReviews)
                    .ThenInclude(cr => cr.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (car == null)
                return NotFound();

            var averageRating = car.CarReviews.Any()
                ? (double)car.CarReviews.Average(r => r.Rating)
                : 0;

            var carDetailDto = new UserCarDetailDto
            {
                Id = car.Id,
                Brand = car.Brand,
                Model = car.Model,
                Description = car.Description,
                DailyPrice = car.DailyPrice,
                ImageUrl = car.ImageUrl,
                IsAvailable = car.IsAvailable,
                Mileage = car.Mileage,
                FuelType = car.FuelType,
                Color = car.Color,
                ModelYear = car.ModelYear,
                GearType = car.GearType?.ToString() ?? "",
                AverageRating = averageRating,
                Reviews = car.CarReviews.Select(r => new UserCarReviewDto
                {
                    UserName = r.User.UserName!,
                    Rating = r.Rating,
                    Comment = r.Comment
                }).ToList()
            };

            return Ok(carDetailDto);
        }
    }


}
