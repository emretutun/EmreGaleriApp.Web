using EmreGaleriApp.Web.Localization;
using EmreGaleriApp.Repository.Models;
using Microsoft.AspNetCore.Identity;

namespace EmreGaleriApp.Web.Extensions
{
    public static class StartUpExtensions
    {
        public static void AddIdentityWithExt(this IServiceCollection services)
        {
            services.Configure<DataProtectionTokenProviderOptions>(opt =>
            {
                opt.TokenLifespan = TimeSpan.FromHours(1);

            });

            services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                // Her kullanıcının benzersiz (unique) e-posta adresi olması zorunlu. Aynı e-posta ile birden fazla hesap açılamaz.

                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyz0123456789_.-";
                // Kullanıcı adı karakterleri sadece küçük harf, rakam, alt çizgi ve nokta ile sınırlı. Özel karakterler ve büyük harf yok.

                options.Password.RequiredLength = 8;
                // Şifre minimum 8 karakter olmalı.

                options.Password.RequireNonAlphanumeric = true;
                // Şifrede mutlaka harf veya rakam olmayan (örn. !, #, $ gibi) özel karakterlerden biri olmalı.

                options.Password.RequireDigit = true;
                // Şifrede en az bir rakam (0-9) bulunmalı.

                options.Password.RequireLowercase = true;
                // Şifrede en az bir küçük harf bulunmalı.

                options.Password.RequireUppercase = true;
                // Şifrede en az bir büyük harf bulunmalı.

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(3);
                // Çok fazla hatalı giriş yapıldığında, 3 dakika boyunca hesabın kilitlenmesi.

                options.Lockout.MaxFailedAccessAttempts = 5;
                // 5 başarısız giriş denemesinden sonra kilitle.

                options.Lockout.AllowedForNewUsers = true;
                // Yeni kullanıcılar da kilitlenmeye tabi


            }).AddErrorDescriber<LocalizationIdentityErrorDescriber>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
        }


    }
}