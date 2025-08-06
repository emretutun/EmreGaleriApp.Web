using EmreGaleriApp.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmreGaleriApp.Repository.Configurations
{
    public class CarReviewConfiguration : IEntityTypeConfiguration<CarReview>
    {
        public void Configure(EntityTypeBuilder<CarReview> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Comment)
                .HasMaxLength(500);

            builder.Property(x => x.Rating)
                .IsRequired();

            builder.Property(x => x.CreatedDate)
                .IsRequired();

            builder.HasOne(x => x.Car)
                .WithMany(x => x.CarReviews)
                .HasForeignKey(x => x.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
