using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmreGaleriApp.Repository.Models.EntityConfigurations
{
    public class PersonelDetailConfiguration : IEntityTypeConfiguration<PersonelDetail>
    {
        public void Configure(EntityTypeBuilder<PersonelDetail> builder)
        {
            builder.HasOne(pd => pd.User)
                   .WithOne() // AppUser içinde navigation yoksa böyle bırakılır
                   .HasForeignKey<PersonelDetail>(pd => pd.UserId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
