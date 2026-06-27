using Esportify.Data;
using Esportify.DTOs;
using Esportify.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Esportify.Controllers.API
{
    [Route("api/auth")]
    [ApiController]
    public class AuthApiController : ControllerBase
    {
        private readonly EsportifyContext _context;
        private readonly ILogger<AuthApiController> _logger;
        private readonly SignInManager<User>? _signInManager;

        public AuthApiController(
            EsportifyContext context,
            ILogger<AuthApiController> logger,
            IServiceProvider serviceProvider)
        {
            _context = context;
            _logger = logger;
            _signInManager = serviceProvider.GetService<SignInManager<User>>();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null || string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Credenciais inválidas." });
            }

            if (_signInManager != null)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
            }
            else
            {
                await SignInWithCookieAsync(user);
            }

            HttpContext.Session.SetString("UserId", user.Id);

            return Ok(new { message = "Login efetuado com sucesso." });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            if (_signInManager != null)
            {
                await _signInManager.SignOutAsync();
            }
            else
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            HttpContext.Session.Clear();

            return Ok(new { message = "Logout efetuado com sucesso." });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var user = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    id = u.Id,
                    username = u.UserName,
                    email = u.Email
                })
                .FirstOrDefaultAsync();

            if (user == null) return Unauthorized();

            return Ok(user);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            _logger.LogInformation("Register attempt: Username={Username}, Email={Email}", model?.Username, model?.Email);

            // Enhanced model validation
            if (!ModelState.IsValid || model == null)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                if (model == null)
                {
                    errors.Add("O corpo do pedido é nulo");
                }
                _logger.LogWarning("Model state invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(new { error = "Dados inválidos: " + string.Join(", ", errors) });
            }

            // Validate username and email
            if (string.IsNullOrWhiteSpace(model.Username) || model.Username.Length < 3)
            {
                _logger.LogWarning("Invalid username: {Username}", model.Username);
                return BadRequest(new { error = "O nome de utilizador deve ter pelo menos 3 caracteres" });
            }

            if (string.IsNullOrWhiteSpace(model.Email) || !model.Email.Contains("@"))
            {
                _logger.LogWarning("Invalid email: {Email}", model.Email);
                return BadRequest(new { error = "O email deve ser válido" });
            }

            // Password validation: minimum 8 characters, 1 uppercase, 1 digit, 1 special character
            if (string.IsNullOrWhiteSpace(model.Password) || model.Password.Length < 8 ||
                !System.Text.RegularExpressions.Regex.IsMatch(model.Password, @"[A-Z]") ||
                !System.Text.RegularExpressions.Regex.IsMatch(model.Password, @"[0-9]") ||
                !System.Text.RegularExpressions.Regex.IsMatch(model.Password, @"[^A-Za-z0-9]"))
            {
                _logger.LogWarning("Invalid password for username: {Username}", model.Username);
                return BadRequest(new { error = "A palavra-passe deve ter pelo menos 8 caracteres, incluindo uma maiúscula, um número e um caractere especial" });
            }

            if (model.Password != model.ConfirmPassword)
            {
                _logger.LogWarning("Password mismatch for username: {Username}", model.Username);
                return BadRequest(new { error = "As palavras-passe não coincidem" });
            }

            // Check for existing user
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == model.Username || u.Email == model.Email);
            if (existingUser != null)
            {
                var errorMessage = existingUser.UserName == model.Username ? "Nome de utilizador já em uso" : "Email já em uso";
                _logger.LogWarning("Registration failed: {Error}", errorMessage);
                return BadRequest(new { error = errorMessage });
            }

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = model.Username,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                IsAdmin = false
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
                _logger.LogError(ex, "Database error during registration for username: {Username}", model.Username);
                return StatusCode(500, new { error = "Erro ao salvar utilizador. Tente novamente." });
            }

            // Set up authentication with role claim
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            _logger.LogInformation("User registered and signed in: {Username}, IsAdmin: {IsAdmin}", user.UserName, user.IsAdmin);
            return Ok(new { message = "Registo bem-sucedido" });
        }

        [HttpPost("check-username")]
        public async Task<IActionResult> CheckUsername([FromBody] UsernameCheckModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Username))
            {
                return BadRequest(new { message = "O nome de utilizador é obrigatório", available = false });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == model.Username);
            return Ok(new { message = user == null ? "Nome de utilizador disponível" : "Nome de utilizador já em uso", available = user == null });
        }

        [HttpPost("validate-password")]
        public IActionResult ValidatePassword([FromBody] PasswordModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                return Ok(new { isValid = false, message = "A palavra-passe é obrigatória" });
            }

            bool isValid = model.Password.Length >= 8 &&
                           System.Text.RegularExpressions.Regex.IsMatch(model.Password, @"[A-Z]") &&
                           System.Text.RegularExpressions.Regex.IsMatch(model.Password, @"[0-9]") &&
                           System.Text.RegularExpressions.Regex.IsMatch(model.Password, @"[^A-Za-z0-9]");
            return Ok(new { isValid, message = isValid ? "Palavra-passe forte" : "A palavra-passe deve ter pelo menos 8 caracteres, incluindo uma maiúscula, um número e um caractere especial" });
        }

        private async Task SignInWithCookieAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User"),
                new Claim("IsOrganizer", user.IsOrganizer.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }
    }
}
