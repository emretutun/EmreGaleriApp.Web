using Microsoft.AspNetCore.Identity;

namespace EmreGaleriApp.Web.Localization
{
    public class LocalizationIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DuplicateUserName(string userName)
        {
            return new()
            {
                Code = "DuplicateUserName",
                Description = $"{userName} adı başka bir kullanıcı tarafından alınmıştır."
            };
        }

        public override IdentityError DuplicateEmail(string email)
        {
            return new()
            {
                Code = "DuplicateEmail",
                Description = $"{email} adresi başka bir kullanıcı tarafından alınmıştır."
            };
        }

        public override IdentityError PasswordTooShort(int length)
        {
            return new()
            {
                Code = "PasswordTooShort",
                Description = $"Şifre en az {length} karakter uzunluğunda olmalıdır."
            };
        }

        public override IdentityError PasswordRequiresNonAlphanumeric()
        {
            return new()
            {
                Code = "PasswordRequiresNonAlphanumeric",
                Description = "Şifre en az bir özel karakter içermelidir (örn. !, @, #, vb)."
            };
        }

        public override IdentityError PasswordRequiresUpper()
        {
            return new()
            {
                Code = "PasswordRequiresUpper",
                Description = "Şifre en az bir büyük harf içermelidir (A-Z)."
            };
        }

        public override IdentityError PasswordRequiresLower()
        {
            return new()
            {
                Code = "PasswordRequiresLower",
                Description = "Şifre en az bir küçük harf içermelidir (a-z)."
            };
        }

        public override IdentityError PasswordRequiresDigit()
        {
            return new()
            {
                Code = "PasswordRequiresDigit",
                Description = "Şifre en az bir rakam içermelidir (0-9)."
            };
        }

        public override IdentityError InvalidUserName(string? userName)
        {
            return new()
            {
                Code = "InvalidUserName",
                Description = $"Kullanıcı adı '{userName}' geçersizdir. Sadece harf ve rakam içerebilir."
            };
        }



        // İstersen diğer hata mesajlarını da buraya ekleyebilirsin.
    }
}
