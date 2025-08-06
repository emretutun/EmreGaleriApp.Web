using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EmreGaleriApp.Repository.Models;

namespace EmreGaleriApp.Repository.Models.EntityConfigurations
{
    public class CarConfiguration : IEntityTypeConfiguration<Car>
    {
        public void Configure(EntityTypeBuilder<Car> builder)
        {
            builder.Property(c => c.DailyPrice).HasPrecision(18, 2);

            builder.HasMany(c => c.CarReviews)
                   .WithOne(cr => cr.Car)
                   .HasForeignKey(cr => cr.CarId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.CarLicenseTypes)
                   .WithOne(cl => cl.Car)
                   .HasForeignKey(cl => cl.CarId);
        }
    }
}
