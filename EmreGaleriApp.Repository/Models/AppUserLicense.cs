namespace EmreGaleriApp.Repository.Models
{
    public class AppUserLicense
    {
        public string AppUserId { get; set; } = null!;
        public AppUser AppUser { get; set; } = null!;

        public int LicenseTypeId { get; set; }
        public LicenseType LicenseType { get; set; } = null!;
    }
}
