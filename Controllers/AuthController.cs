using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Camp_booking_api.Data;
using Camp_booking_api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Camp_booking_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) 
                )
            {
                return BadRequest("All fields are required");
            }

            var normalizedEmail = request.Email.Trim().ToLower();

            var existingUser = _context.Users.FirstOrDefault(u => u.Email == normalizedEmail);
            if (existingUser != null)
            {
                return BadRequest("Email already exists");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = normalizedEmail,
                Password = hashedPassword
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256
                )
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                token = jwt
            });
        }
        [HttpPost("login")]
public IActionResult Login(LoginRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Email) ||
        string.IsNullOrWhiteSpace(request.Password))
    {
        return BadRequest("Email and password are required");
    }

    var normalizedEmail = request.Email.Trim().ToLower();

    var user = _context.Users.FirstOrDefault(u => u.Email == normalizedEmail);

    if (user == null)
    {
        return Unauthorized("Invalid email or password");
    }

    var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);

    if (!isPasswordValid)
    {
        return Unauthorized("Invalid email or password");
    }

    var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, user.FullName)
    };

    var token = new JwtSecurityToken(
        issuer: _configuration["Jwt:Issuer"],
        audience: _configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddDays(1),
        signingCredentials: new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256
        )
    );

    var jwt = new JwtSecurityTokenHandler().WriteToken(token);

    return Ok(new
    {
        token = jwt
    });
}
    }
}

