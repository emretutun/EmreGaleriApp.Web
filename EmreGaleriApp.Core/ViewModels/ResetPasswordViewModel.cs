using System.ComponentModel.DataAnnotations;

namespace EmreGaleriApp.Core.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string? UserId { get; set; }   // Kullanıcı id'si için

        [Required]
        public string? Token { get; set; }    // Token için

        [Required(ErrorMessage = "Yeni Şifre Zorunludur!")]
        [Display(Name = "Şifre :")]
        public string? Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Yeni Şifreler aynı değil!")]
        [Required(ErrorMessage = "Şifre Tekrar Zorunludur!")]
        [Display(Name = "Yeni Şifre Tekrar :")]
        public string? PasswordConfirm { get; set; }
    }
}
