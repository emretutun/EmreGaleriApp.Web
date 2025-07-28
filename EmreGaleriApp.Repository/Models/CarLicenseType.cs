namespace EmreGaleriApp.Repository.Models
{
    public class CarLicenseType
    {
        public int CarId { get; set; }
        public Car? Car { get; set; }

        public int LicenseTypeId { get; set; }
        public LicenseType? LicenseType { get; set; }
    }

}
