using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace EmreGaleriApp.Repository.Models
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<LicenseType> LicenseTypes { get; set; }
        public DbSet<AppUserLicense> AppUserLicenses { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<Firm> Firms { get; set; }
        public DbSet<StockItem> StockItems { get; set; }
        public DbSet<CarReview> CarReviews { get; set; }

        public DbSet<CashRegister> CashRegisters { get; set; }

        public DbSet<PersonelDetail> PersonelDetails { get; set; }


        // ✅ Yeni ilişki tablosu:
        public DbSet<CarLicenseType> CarLicenseTypes { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<PersonelDetail>()
                .HasOne(pd => pd.User)
                .WithOne()  // AppUser sınıfında navigation property yoksa WithOne() şeklinde bırakılır
                .HasForeignKey<PersonelDetail>(pd => pd.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);




            // Kullanıcı ↔ Ehliyet (zaten vardı)
            builder.Entity<AppUserLicense>()
                .HasKey(au => new { au.AppUserId, au.LicenseTypeId });

            builder.Entity<AppUserLicense>()
                .HasOne(au => au.AppUser)
                .WithMany(u => u.AppUserLicenses)
                .HasForeignKey(au => au.AppUserId);

            builder.Entity<AppUserLicense>()
                .HasOne(au => au.LicenseType)
                .WithMany(l => l.AppUserLicenses)
                .HasForeignKey(au => au.LicenseTypeId);

            // Araç ↔ Ehliyet (Yeni eklenen yapı)
            builder.Entity<CarLicenseType>()
                .HasKey(cl => new { cl.CarId, cl.LicenseTypeId });

            builder.Entity<CarLicenseType>()
                .HasOne(cl => cl.Car)
                .WithMany(c => c.CarLicenseTypes)
                .HasForeignKey(cl => cl.CarId);

            builder.Entity<CarLicenseType>()
                .HasOne(cl => cl.LicenseType)
                .WithMany()
                .HasForeignKey(cl => cl.LicenseTypeId);

            // Hassasiyet ayarları
            builder.Entity<Car>()
                .Property(c => c.DailyPrice)
                .HasPrecision(18, 2);

            builder.Entity<Order>()
                .Property(o => o.TotalPrice)
                .HasPrecision(18, 2);

            builder.Entity<OrderItem>()
                .Property(oi => oi.DailyPrice)
                .HasPrecision(18, 2);

            // CarReview ilişkileri
            builder.Entity<CarReview>()
                .HasOne(cr => cr.User)
                .WithMany()
                .HasForeignKey(cr => cr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CarReview>()
                .HasOne(cr => cr.Car)
                .WithMany(c => c.CarReviews)
                .HasForeignKey(cr => cr.CarId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed Data
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
