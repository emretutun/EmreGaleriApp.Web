namespace EmreGaleriApp.Web.ApiDto
{
    // DTO'lar
    public class UserCarListDto
    {
        public int Id { get; set; }
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal DailyPrice { get; set; }
        public string ImageUrl { get; set; } = null!;
        public bool IsAvailable { get; set; }
        public string Color { get; set; } = null!;
        public string FuelType { get; set; } = null!;
        public int Mileage { get; set; }
        public int ModelYear { get; set; }
        public string GearType { get; set; } = null!;
    }

    public class UserCarDetailDto : UserCarListDto
    {
        public double AverageRating { get; set; }
        public List<UserCarReviewDto> Reviews { get; set; } = new();
    }

    public class UserCarReviewDto
    {
        public string UserName { get; set; } = null!;
        public int Rating { get; set; }
        public string Comment { get; set; } = null!;
    }
}
