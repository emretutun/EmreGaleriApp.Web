using System.ComponentModel.DataAnnotations;

namespace EmreGaleriApp.Core.ViewModels
{
    public class SignInViewModel
    {
        public SignInViewModel() { }

        public SignInViewModel(string email, string password, bool rememberMe = false)
        {
            Email = email;
            Password = password;
            RememberMe = rememberMe;
        }

        [Required(ErrorMessage = "E-posta adresi gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Şifre gereklidir.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
        