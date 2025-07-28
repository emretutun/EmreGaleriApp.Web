using System.ComponentModel.DataAnnotations;

namespace EmreGaleriApp.Core.ViewModels
{
    public class SignUpViewModel
    {
        public SignUpViewModel() { } // Parametresiz ctor eklendi

        public SignUpViewModel(string? userName, string? email, string? phone, string? password)
        {
            UserName = userName;
            Email = email;
            Phone = phone;
            Password = password;
        }


        [Required(ErrorMessage = "Kullanıcı Adı Boş Olamaz!")]
        [Display(Name = "Kullanıcı Adı :")]
        public string? UserName { get; set; }

        [EmailAddress(ErrorMessage ="Mail Formatı Hatalı!")]
        [Required(ErrorMessage = "Mail Boş Olamaz!")]
        [Display(Name = "Mail Adresi :")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Telefon Numarası Boş Olamaz!")]
        [Display(Name = "Telefon Numarası :")]
        [RegularExpression(@"^\d{1} \d{3} \d{3} \d{4}$", ErrorMessage = "Telefon numarası formatı: X XXX XXX XXXX şeklinde olmalıdır.")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Şifre Zorunludur!")]
        [Display(Name = "Şifre :")]
        public string? Password { get; set; }

        [Compare(nameof(Password),ErrorMessage ="Şifreler aynı değil!")]
        [Required(ErrorMessage = "Şifre Tekrar Zorunludur!")]
        [Display(Name = "Şifre Tekrar :")]
        public string? PasswordConfirm { get; set; }
    }
}
