using EmreGaleriApp.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmreGaleriApp.Repository.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.TotalPrice)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(o => o.Status)
                   .IsRequired();

            builder.Property(o => o.StartDate)
                   .IsRequired();

            builder.Property(o => o.EndDate)
                   .IsRequired();

            builder.Property(o => o.DeliveryStatus)
                   .IsRequired();

            builder.HasOne(o => o.AppUser)
                   .WithMany() // Eğer AppUser içinde Orders listesi yoksa
                   .HasForeignKey(o => o.AppUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
