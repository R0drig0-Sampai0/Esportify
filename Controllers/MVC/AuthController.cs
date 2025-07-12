using Esportify.Data;
using Esportify.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Esportify.Controllers.MVC
{
    public class AuthController : Controller
    {
        private readonly EsportifyContext _context;

        public AuthController(EsportifyContext context)
        {
            _context = context;
        }

        // GET: /Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Auth/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            // Validate model
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Dados inválidos. Verifique os campos.");
                return View(model);
            }

            // Validate username (min 3 characters)
            if (string.IsNullOrWhiteSpace(model.Username) || model.Username.Length < 3)
            {
                ModelState.AddModelError("Username", "O nome de utilizador deve ter pelo menos 3 caracteres");
                return View(model);
            }

            // Validate email
            if (string.IsNullOrWhiteSpace(model.Email) || !model.Email.Contains("@"))
            {
                ModelState.AddModelError("Email", "O email deve ser válido");
                return View(model);
            }

            // Validate password (min 8 chars, uppercase, digit, special char)
            if (string.IsNullOrWhiteSpace(model.Password) || model.Password.Length < 8 ||
                !System.Text.RegularExpressions.Regex.IsMatch(model.Password, @"[A-Z]") ||
                !System.Text.RegularExpressions.Regex.IsMatch(model.Password, @"[0-9]") ||
                !System.Text.RegularExpressions.Regex.IsMatch(model.Password, @"[^A-Za-z0-9]"))
            {
                ModelState.AddModelError("Password", "A palavra-passe deve ter pelo menos 8 caracteres, incluindo uma maiúscula, um número e um caractere especial");
                return View(model);
            }

            // Check password confirmation
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "As palavras-passe não coincidem");
                return View(model);
            }

            // Check for existing user
            if (await _context.Users.AnyAsync(u => u.UserName == model.Username))
            {
                ModelState.AddModelError("Username", "O nome de utilizador já está em uso");
                return View(model);
            }
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "O email já está em uso");
                return View(model);
            }

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = model.Username,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                IsAdmin = model.Email.EndsWith("@admin.esportify.com") || model.Username == "admin" // Flexible admin assignment
            };

            var userProfile = new UserProfile
            {
                UserId = user.Id,
                AvatarUrl = "/images/default-avatar.jpg",
                Earnings = 0m
            };

            try
            {
                _context.Users.Add(user);
                _context.UserProfiles.Add(userProfile);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Erro ao salvar utilizador: {ex.Message}");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            HttpContext.Session.SetString("UserId", user.Id);

            return RedirectToAction("Index", "LandingPage");
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Nome de utilizador e palavra-passe são obrigatórios");
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Credenciais inválidas");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            Console.WriteLine($"User {user.UserName} signed in, IsAdmin: {user.IsAdmin}");

            HttpContext.Session.SetString("UserId", user.Id);
            return RedirectToAction("Index", "LandingPage");
        }

        // POST: /Auth/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "LandingPage");
        }
    }
}