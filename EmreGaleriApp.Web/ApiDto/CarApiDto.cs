namespace EmreGaleriApp.Web.ApiDto
{
    public class LicenseTypeDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class CarDetailDto
    {
        public int Id { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public int ModelYear { get; set; }
        public double DailyPrice { get; set; }
        public string? Description { get; set; }
        public string? FuelType { get; set; }
        public int Mileage { get; set; }
        public int GearType { get; set; }
        public string? Color { get; set; }
        public string? ImageUrl { get; set; }
        public List<LicenseTypeDto> LicenseTypes { get; set; } = new List<LicenseTypeDto>();
    }

    public class CarCreateDto
    {
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public int ModelYear { get; set; }
        public double DailyPrice { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? FuelType { get; set; }
        public int Mileage { get; set; }
        public int GearType { get; set; }
        public string? Color { get; set; }
        public List<int>? LicenseTypeIds { get; set; }
    }

    public class CarUpdateDto
    {
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public int ModelYear { get; set; }
        public double DailyPrice { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? FuelType { get; set; }
        public int Mileage { get; set; }
        public int GearType { get; set; }
        public List<int>? LicenseTypeIds { get; set; }
        public string? Color { get; set; }
    }
}
