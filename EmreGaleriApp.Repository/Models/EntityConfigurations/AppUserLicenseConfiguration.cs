using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmreGaleriApp.Repository.Models.EntityConfigurations
{
    public class AppUserLicenseConfiguration : IEntityTypeConfiguration<AppUserLicense>
    {
        public void Configure(EntityTypeBuilder<AppUserLicense> builder)
        {
            builder.HasKey(au => new { au.AppUserId, au.LicenseTypeId });

            builder.HasOne(au => au.AppUser)
                   .WithMany(u => u.AppUserLicenses)
                   .HasForeignKey(au => au.AppUserId);

            builder.HasOne(au => au.LicenseType)
                   .WithMany(l => l.AppUserLicenses)
                   .HasForeignKey(au => au.LicenseTypeId);
        }
    }
}
