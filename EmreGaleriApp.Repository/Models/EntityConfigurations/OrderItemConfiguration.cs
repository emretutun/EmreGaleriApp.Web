using EmreGaleriApp.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmreGaleriApp.Repository.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.HasKey(oi => oi.Id);

            builder.Property(oi => oi.DailyPrice)
                   .HasPrecision(18, 2)
                   .IsRequired();

            builder.HasOne(oi => oi.Order)
                   .WithMany(o => o.OrderItems)
                   .HasForeignKey(oi => oi.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(oi => oi.Car)
                   .WithMany()
                   .HasForeignKey(oi => oi.CarId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
