using Esportify.Data;
using Esportify.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Esportify.Controllers
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
        public async Task<IActionResult> Register(string username, string email, string password)
        {
            if (_context.Users.Any(u => u.Username == username))
            {
                ModelState.AddModelError("", "O nome de utilizador já está em uso");
                return View();
            }

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            HttpContext.Session.SetInt32("UserId", user.Id);

            return RedirectToAction("Index", "Home");
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
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Credenciais inválidas");
                return View();
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Username),
    };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            Console.WriteLine($"User {user.Username} signed in");

            HttpContext.Session.SetInt32("UserId", user.Id);
            return RedirectToAction("Index", "Home");
        }

        // POST: /Auth/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var properties = new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(-1),
                IsPersistent = false
            };
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme, properties);

            foreach (var cookie in HttpContext.Request.Cookies.Keys)
            {
                HttpContext.Response.Cookies.Delete(cookie);
            }

            HttpContext.Session.Clear();
            var isAuthenticatedAfterSignOut = HttpContext.User.Identity.IsAuthenticated;
            return RedirectToAction("Index", "Home");
        }

    }
}
