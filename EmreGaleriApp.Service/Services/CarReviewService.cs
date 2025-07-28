using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmreGaleriApp.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace EmreGaleriApp.Service.Services
{
    public class CarReviewService : ICarReviewService
    {
        private readonly AppDbContext _context;

        public CarReviewService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasUserReviewedOrderAsync(string userId, int orderId, int carId)
        {
            return await _context.CarReviews
                .AnyAsync(r => r.UserId == userId && r.OrderId == orderId && r.CarId == carId);
        }


        public async Task AddReviewAsync(CarReview review)
        {
            review.CreatedDate = DateTime.Now;
            _context.CarReviews.Add(review);
            await _context.SaveChangesAsync();
        }

        public async Task<List<CarReview>> GetReviewsByCarIdAsync(int carId)
        {
            return await _context.CarReviews
                .Include(r => r.User)
                .Where(r => r.CarId == carId)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<double> GetAverageRatingAsync(int carId)
        {
            var reviews = await _context.CarReviews
                .Where(r => r.CarId == carId)
                .ToListAsync();

            if (reviews.Count == 0)
                return 0;

            return reviews.Average(r => r.Rating);
        }


        public async Task DeleteReviewAsync(int reviewId)
        {
            var review = await _context.CarReviews.FindAsync(reviewId);
            if (review != null)
            {
                _context.CarReviews.Remove(review);
                await _context.SaveChangesAsync();
            }
        }
    }
}
