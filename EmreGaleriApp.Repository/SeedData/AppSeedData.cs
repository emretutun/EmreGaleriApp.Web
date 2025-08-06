using EmreGaleriApp.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace EmreGaleriApp.Repository.SeedData
{
    public static class AppSeedData
    {
        public static void SeedLicenseTypes(this ModelBuilder builder)
        {
            builder.Entity<LicenseType>().HasData(
                new LicenseType { Id = 1, Name = "A" },
                new LicenseType { Id = 2, Name = "A1" },
                new LicenseType { Id = 3, Name = "A2" },
                new LicenseType { Id = 4, Name = "M" },
                new LicenseType { Id = 5, Name = "B" },
                new LicenseType { Id = 6, Name = "B1" },
                new LicenseType { Id = 7, Name = "BE" },
                new LicenseType { Id = 8, Name = "C" },
                new LicenseType { Id = 9, Name = "C1" },
                new LicenseType { Id = 10, Name = "CE" },
                new LicenseType { Id = 11, Name = "C1E" },
                new LicenseType { Id = 12, Name = "D" },
                new LicenseType { Id = 13, Name = "D1" },
                new LicenseType { Id = 14, Name = "DE" },
                new LicenseType { Id = 15, Name = "D1E" },
                new LicenseType { Id = 16, Name = "G" },
                new LicenseType { Id = 17, Name = "F" }
            );
        }
    }
}
