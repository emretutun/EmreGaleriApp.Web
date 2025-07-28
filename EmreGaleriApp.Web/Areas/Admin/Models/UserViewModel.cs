namespace EmreGaleriApp.Web.Areas.Admin.Models
{
    public class UserViewModel
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }

        // Yeni alanlar
        public string? NationalId { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? DrivingLicenseType { get; set; }
        public string? Gender { get; set; }
        public int? DrivingExperienceYears { get; set; }
        public string? Picture { get; set; }  // Fotoğraf dosya adı veya URL

        public List<string> LicenseTypes { get; set; } = new();

        public List<string>? Roles { get; set; }

    }
}
