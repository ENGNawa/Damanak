using Damanak.Data;
using Damanak.Models;
using Damanak.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Damanak.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<AppUser> _passwordHasher;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<AppUser>();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email = model.Email.Trim().ToLower();

            var emailExists = await _context.AppUsers.AnyAsync(u => u.Email == email);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "البريد الإلكتروني مستخدم مسبقًا");
                return View(model);
            }

            var user = new AppUser
            {
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Email = email,
                City = model.City,
                Role = "User",
                CreatedAt = DateTime.Now
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

            _context.AppUsers.Add(user);
            await _context.SaveChangesAsync();

            await SignInUser(user);

            return RedirectToAction("Index", "Guarantees");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email = model.Email.Trim().ToLower();

            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                ModelState.AddModelError("", "البريد الإلكتروني أو كلمة المرور غير صحيحة");
                return View(model);
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "البريد الإلكتروني أو كلمة المرور غير صحيحة");
                return View(model);
            }

            await SignInUser(user);

            return RedirectToAction("Index", "Guarantees");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        private async Task SignInUser(AppUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );
        }
    }
}