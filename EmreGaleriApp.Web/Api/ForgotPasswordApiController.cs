using EmreGaleriApp.Service.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Net;
using EmreGaleriApp.Web.ApiDto;
using EmreGaleriApp.Repository.Models; // AppUser burada tanımlı

namespace EmreGaleriApp.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForgotPasswordApiController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;

        public ForgotPasswordApiController(UserManager<AppUser> userManager, IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email))
            {
                return BadRequest(new { message = "Email boş olamaz." });
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Kullanıcı yoksa bile güvenlik için aynı mesajı döneriz
                    return Ok(new { message = "Eğer mailiniz sistemde kayıtlıysa şifre sıfırlama linki gönderildi." });
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = WebUtility.UrlEncode(token);

                // Burada "Home" controller ve "ResetPassword" action’a göre link oluşturuyoruz
                var resetLink = Url.Action(
                    action: "ResetPassword",
                    controller: "Home",    // Web projenizdeki ResetPassword controller adı genelde Home olur
                    values: new { userId = user.Id, token = encodedToken },
                    protocol: Request.Scheme);

                if (string.IsNullOrEmpty(resetLink))
                {
                    return StatusCode(500, new { message = "Şifre sıfırlama linki oluşturulamadı." });
                }

                await _emailService.SendResetPasswordEmail(resetLink, user.Email!);

                return Ok(new { message = "Şifre sıfırlama linki mailinize gönderildi." });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası: " + ex.Message });
            }
        }
    }
}
