namespace EmreGaleriApp.Repository.Models
{
    public class LicenseType
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<AppUserLicense> AppUserLicenses { get; set; } = new List<AppUserLicense>();
    }
}
