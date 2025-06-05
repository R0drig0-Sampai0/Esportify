using Esportify.Data;
using Esportify.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

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
                PasswordHash = password
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

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
                .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == password);

            if (user == null)
            {
                ModelState.AddModelError("", "Credenciais inválidas");
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            return RedirectToAction("Index", "Home");
        }

        // GET: /Auth/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

    }
}
