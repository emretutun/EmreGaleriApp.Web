namespace EmreGaleriApp.Web.ApiDto
{
    public class UserProfileDto
    {
        public string Id { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? NationalId { get; set; }
        public string? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public int? DrivingExperienceYears { get; set; }
        public string? PictureUrl { get; set; }
        public List<UserLicenseTypeDto> LicenseTypes { get; set; } = new();
    }

    public class UserLicenseTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public class UserProfileUpdateDto
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? NationalId { get; set; }
        public string? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public int? DrivingExperienceYears { get; set; }
        public string? PictureUrl { get; set; }
        public List<int>? LicenseTypeIds { get; set; }
    }
}
