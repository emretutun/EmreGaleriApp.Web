using Microsoft.AspNetCore.Identity;

namespace EmreGaleriApp.Repository.Models
{
    public class AppUser : IdentityUser
    {

        public string? NationalId { get; set; }            // TC Kimlik No yerine 
        public string? Gender { get; set; }                 // Cinsiyet
        public DateTime? BirthDate { get; set; }            // Doğum Tarihi
        public int? DrivingExperienceYears { get; set; }    // Sürüş deneyimi (yıl)
        public string? PictureUrl { get; set; }

        public ICollection<AppUserLicense> AppUserLicenses { get; set; } = new List<AppUserLicense>();

    }
}
