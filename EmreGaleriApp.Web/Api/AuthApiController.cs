using EmreGaleriApp.Repository.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using EmreGaleriApp.Web.ApiDto;

namespace EmreGaleriApp.Web.Api
{
    [Route("api/auth")]
    [ApiController]
    public class AuthApiController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthApiController(UserManager<AppUser> userManager,
                                 SignInManager<AppUser> signInManager,
                                 IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        

        private string FormatPhoneNumber(string raw)
        {
            var digits = new string(raw?.Where(char.IsDigit).ToArray() ?? Array.Empty<char>());
            if (digits.Length != 11 || !digits.StartsWith("0"))
                return raw ?? string.Empty;

            return $"{digits.Substring(0, 1)} {digits.Substring(1, 3)} {digits.Substring(4, 3)} {digits.Substring(7, 4)}";
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Geçersiz model." });

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized(new { message = "Kullanıcı bulunamadı." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new { message = "Şifre hatalı." });

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? "")
            };

            // Kullanıcının rollerini claim olarak ekle
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:DurationInMinutes"] ?? "60")),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                token = tokenString,
                expiration = token.ValidTo,
                roles = roles
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Geçersiz kayıt bilgileri." });

            if (model.Password != model.ConfirmPassword)
                return BadRequest(new { message = "Şifre ve şifre tekrar eşleşmiyor." });

            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return BadRequest(new { message = "Bu kullanıcı adı zaten kullanılıyor." });

            var emailExists = await _userManager.FindByEmailAsync(model.Email);
            if (emailExists != null)
                return BadRequest(new { message = "Bu email zaten kayıtlı." });

            var user = new AppUser
            {
                UserName = model.Username,
                Email = model.Email,
                PhoneNumber = FormatPhoneNumber(model.PhoneNumber),
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { message = $"Kullanıcı oluşturulamadı: {errors}" });
            }

            return Ok(new { message = "Kullanıcı başarıyla oluşturuldu." });
        }
    }
}
