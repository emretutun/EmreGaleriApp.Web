using System.ComponentModel.DataAnnotations;

namespace EmreGaleriApp.Core.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mevcut şifrenizi giriniz.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Şifre")]
        public string? CurrentPassword { get; set; }

        [Required(ErrorMessage = "Yeni şifre giriniz.")]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        public string? NewPassword { get; set; }

        [Required(ErrorMessage = "Yeni şifre tekrar giriniz.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Şifreler uyuşmuyor.")]
        [Display(Name = "Yeni Şifre (Tekrar)")]
        public string? ConfirmNewPassword { get; set; }
    }
}
