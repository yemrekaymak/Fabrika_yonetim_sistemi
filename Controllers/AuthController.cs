using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Data;
using FabrikaBackend.Models;
using FabrikaBackend.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FabrikaBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto request)
    {
        var email = (request.Email ?? "").Trim();
        var password = request.Password ?? "";
        if (string.IsNullOrEmpty(email))
        {
            return BadRequest(new { Mesaj = "E-posta giriniz." });
        }
        if (await _context.Users.AnyAsync(u => u.Email == email))
        {
            return BadRequest(new { Mesaj = "Bu email zaten kullanılıyor." });
        }

        var newUser = new User
        {
            Email = email,
            Password = password // Gerçek hayatta şifrelenmeli!
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return Ok(new { Mesaj = "Kayıt başarılı!", Kullanici = newUser });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto request)
    {
        var email = (request.Email ?? "").Trim();
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || user.Password != (request.Password ?? ""))
        {
            return BadRequest(new { Mesaj = "Email veya şifre hatalı!" });
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var keyString = _configuration["Jwt:Key"] ?? "CokGizliAnahtar123!";
        var key = Encoding.UTF8.GetBytes(keyString); 
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                // HATA DÜZELTİLDİ: Role veritabanında yoksa buraya sabit "User" yazıyoruz
                new Claim("role", "User"), 
                new Claim("company_id", "1") 
            }),
            Expires = DateTime.UtcNow.AddHours(3),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtString = tokenHandler.WriteToken(token);

        return Ok(new { 
            mesaj = "Başarıyla giriş yaptınız!", 
            email = user.Email, 
            rol = "User", // Frontend patlamasın diye sabit değer döndürüyoruz
            token = jwtString 
        });
    }
}