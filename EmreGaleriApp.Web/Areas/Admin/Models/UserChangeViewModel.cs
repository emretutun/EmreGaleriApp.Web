namespace EmreGaleriApp.Web.Areas.Admin.Models
{
    public class UserChangeViewModel
    {
        public string? Id { get; set; }  // FindByIdAsync için lazım
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? NationalId { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Gender { get; set; }
        public int? DrivingExperienceYears { get; set; }

        // Ehliyet tipleri checkbox listesi
        public List<LicenseTypeCheckboxItem> LicenseTypes { get; set; } = new List<LicenseTypeCheckboxItem>();
    }

    public class LicenseTypeCheckboxItem
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool IsSelected { get; set; }
    }
}
