using EmreGaleriApp.Core.Enums;

namespace EmreGaleriApp.Repository.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public decimal DailyPrice { get; set; }
        public bool IsAvailable { get; set; } = true;

        public int Mileage { get; set; }              // Kilometre
        public string FuelType { get; set; } = null!; // Yakıt tipi: Benzin, Dizel, LPG, Elektrik
        public string Color { get; set; } = null!;     // Renk
        public int ModelYear { get; set; }            // Model yılı

        public GearType? GearType { get; set; }



        public ICollection<CarReview> CarReviews { get; set; } = new List<CarReview>();
        public ICollection<CarLicenseType>? CarLicenseTypes { get; set; }
    }



}
