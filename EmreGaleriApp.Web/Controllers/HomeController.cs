using System.Diagnostics;
using EmreGaleriApp.Repository.Models;
using EmreGaleriApp.Core.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EmreGaleriApp.Web.Extensions;
using EmreGaleriApp.Web.Services;
using Microsoft.EntityFrameworkCore;
using EmreGaleriApp.Service.Services;

namespace EmreGaleriApp.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly AppDbContext _dbContext;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IEmailService emailService,
            AppDbContext dbContext)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {

            var cars = await _dbContext.Cars.Where(c => c.IsAvailable).ToListAsync();
            return View(cars);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            // 1️⃣ E-posta daha önce kullanılmış mı kontrol et
            var existingUser = await _userManager.FindByEmailAsync(request.Email!.Trim().ToLowerInvariant());
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor.");
                return View(request);
            }

            // 2️⃣ Kayıt işlemi
            var identityResult = await _userManager.CreateAsync(
                new AppUser
                {
                    UserName = request.UserName,
                    PhoneNumber = request.Phone,
                    Email = request.Email
                }, request.Password!);

            if (identityResult.Succeeded)
            {
                TempData["SuccessMessage"] = "Kayıt Başarılı!";
                return RedirectToAction(nameof(SignUp));
            }

            ModelState.AddModelErrorList(identityResult.Errors.Select(x => x.Description).ToList());
            return View(request);
        }


        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(SignInViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Mail veya şifre yanlış!");
                return View(model);
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (signInResult.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Mail veya şifre yanlış!");
            return View(model);
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel request)
        {
            var hasUser = await _userManager.FindByEmailAsync(request.Email!);

            if (hasUser == null)
            {
                ModelState.AddModelError(string.Empty, "Mail bulunamadı!");
                return View();
            }

            string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(hasUser);

            var passwordResetLink = Url.Action(
                "ResetPassword",               // Action
                "Home",                        // Controller
                new { userId = hasUser.Id, token = passwordResetToken },  // Route values
                HttpContext.Request.Scheme,    // "https" veya "http"
                HttpContext.Request.Host.Value // hostname + port, örn: localhost:7191
            );

            await _emailService.SendResetPasswordEmail(passwordResetLink!, hasUser!.Email!);

            TempData["SuccessMessage"] = "Şifre Sıfırlama Maili Gönderildi";
            return RedirectToAction(nameof(ForgotPassword));
        }


        [HttpGet]
        public IActionResult ResetPassword(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Geçersiz kullanıcı veya token.";
                return View();
            }

            var model = new ResetPasswordViewModel
            {
                UserId = userId,
                Token = token
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model!.UserId!);
            if (user == null)
            {
                ModelState.AddModelError("", "Kullanıcı bulunamadı.");
                return View(model);
            }

            var resetResult = await _userManager.ResetPasswordAsync(user, model.Token!, model.Password!);

            if (resetResult.Succeeded)
            {
                TempData["SuccessMessage"] = "Şifreniz başarıyla sıfırlandı.";
                return RedirectToAction("SignIn");
            }
            else
            {
                foreach (var error in resetResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
