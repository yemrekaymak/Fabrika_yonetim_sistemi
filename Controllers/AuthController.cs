using Microsoft.AspNetCore.Mvc;
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

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    // --- 1. KAYIT OL (REGISTER) ---
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto request)
    {
        // Email daha önce alınmış mı kontrolü (varsa sende aynen kalsın)
        if (_context.Users.Any(u => u.Email == request.Email))
        {
            return BadRequest("Bu email zaten kullanılıyor.");
        }

        // İŞTE SİLİNEN O EFSANE KISIM! Tertemiz haliyle baştan tanımlıyoruz:
        var newUser = new User
        {
            Email = request.Email,
            Password = request.Password, // Gerçek hayatta şifrelenir
            Role = "Admin" // DTO'dan kestiğimiz için buraya ellerimizle sabitledik!
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return Ok(new { Mesaj = "Kayıt başarılı!", Kullanici = newUser });
    }

    // --- 2. GİRİŞ YAP (LOGIN) VE YAKA KARTI VER ---
    [HttpPost("login")]
    public IActionResult Login(UserLoginDto request)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);

        if (user == null)
        {
            return NotFound("Kayıtlı kullanıcı bulunamadı!");
        }

        if (user.Password != request.Password)
        {
            return BadRequest("Şifreniz hatalı!");
        }

        // --- İŞTE YENİ EKLENEN YAKA KARTI (JWT) ÜRETİM MERKEZİ ---
        var tokenHandler = new JwtSecurityTokenHandler();
        // Şifreyi Program.cs'deki ile aynı yapıyoruz
        var key = Encoding.UTF8.GetBytes("BenimCokGizliFabrikaSifrem123456789!"); 
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role) // Rolü yaka kartına basıyoruz
            }),
            Expires = DateTime.UtcNow.AddHours(2), // 2 saat geçerli
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtString = tokenHandler.WriteToken(token);

        // HTML'in tam beklediği JSON formatında cevabı yolluyoruz
        return Ok(new { 
            Mesaj = "Başarıyla giriş yaptınız!", 
            Email = user.Email, 
            Rol = user.Role,
            Token = jwtString 
        });
    }
}