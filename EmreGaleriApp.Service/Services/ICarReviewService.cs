using EmreGaleriApp.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmreGaleriApp.Service.Services
{
    public interface ICarReviewService
    {
        Task AddReviewAsync(CarReview review);
        Task<List<CarReview>> GetReviewsByCarIdAsync(int carId);
        Task<double> GetAverageRatingAsync(int carId);
        // ICarReviewService.cs
        Task<bool> HasUserReviewedOrderAsync(string userId, int orderId, int carId);

        Task DeleteReviewAsync(int reviewId);
    }

}
