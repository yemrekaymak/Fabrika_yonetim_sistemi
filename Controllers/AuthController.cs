using Microsoft.AspNetCore.Mvc;
using FabrikaBackend.Data;
using FabrikaBackend.Models;
using FabrikaBackend.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FabrikaBackend.Controllers;

[Route("api/[controller]")] // Burası api/Auth rotasını oluşturur
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

    // --- 1. KAYIT OL (REGISTER) ---
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto request)
    {
        if (_context.Users.Any(u => u.Email == request.Email))
        {
            return BadRequest(new { Mesaj = "Bu email zaten kullanılıyor." });
        }

        var newUser = new User
        {
            Email = request.Email,
            Password = request.Password, // Gerçek hayatta şifrelenir
            Role = "User" // Varsayılan rol
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return Ok(new { Mesaj = "Kayıt başarılı!", Kullanici = newUser });
    }

    // --- 2. GİRİŞ YAP (LOGIN) ---
    [HttpPost("login")]
    public IActionResult Login(UserLoginDto request)
    {
        // Büyük/Küçük harf duyarlılığını kaldırmak için ToLower() kullanabilirsin
        var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);

        if (user == null || user.Password != request.Password)
        {
            return BadRequest(new { Mesaj = "Email veya şifre hatalı!" });
        }

        // --- JWT ÜRETİMİ ---
        var tokenHandler = new JwtSecurityTokenHandler();
        
        // KRİTİK DÜZELTME 1: Anahtar (Key) Program.cs ile BİREBİR aynı olmalı
        // Config'den çekmek en güvenlisidir, yoksa Program.cs'deki fallback'i kullanıyoruz.
        var keyString = _configuration["Jwt:Key"] ?? "CokGizliAnahtar123!";
        var key = Encoding.UTF8.GetBytes(keyString); 
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Email), // Name claim'i önemli
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("role", user.Role ?? "User"), // Frontend'in beklediği küçük harf 'role'
                new Claim("company_id", "1") // AccountingController'ın beklediği kritik bilgi!
            }),
            Expires = DateTime.UtcNow.AddHours(3), // Süreyi 3 saate çıkardım
            // KRİTİK DÜZELTME 2: Issuer ve Audience Program.cs'dekilerle eşleşmeli
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtString = tokenHandler.WriteToken(token);

        // Frontend'in (authService.js) tam beklediği JSON formatı:
        return Ok(new { 
            mesaj = "Başarıyla giriş yaptınız!", 
            email = user.Email, 
            rol = user.Role,
            token = jwtString // Küçük harf 'token' frontend için daha güvenli
        });
    }
}