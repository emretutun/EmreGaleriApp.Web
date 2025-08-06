using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmreGaleriApp.Repository.Models.EntityConfigurations
{
    public class CarLicenseTypeConfiguration : IEntityTypeConfiguration<CarLicenseType>
    {
        public void Configure(EntityTypeBuilder<CarLicenseType> builder)
        {
            builder.HasKey(cl => new { cl.CarId, cl.LicenseTypeId });

            builder.HasOne(cl => cl.Car)
                   .WithMany(c => c.CarLicenseTypes)
                   .HasForeignKey(cl => cl.CarId);

            builder.HasOne(cl => cl.LicenseType)
                   .WithMany()
                   .HasForeignKey(cl => cl.LicenseTypeId);
        }
    }
}
