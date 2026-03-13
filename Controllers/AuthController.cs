using Microsoft.AspNetCore.Mvc;
using FabrikaBackend.Data;
using FabrikaBackend.Models;
using FabrikaBackend.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

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

    // --- 1. KAYIT OL (REGISTER) ---
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(UserRegisterDto request)
    {
        if (request.Password != request.ConfirmPassword)
        {
            return BadRequest("Şifre ve şifre tekrarı aynı olmalıdır.");
        }

        // Email daha önce alınmış mı kontrolü (varsa sende aynen kalsın)
        if (_context.Users.Any(u => u.Email == request.Email))
        {
            return BadRequest("Bu email zaten kullanılıyor.");
        }

        if (string.IsNullOrWhiteSpace(request.CompanyName))
        {
            return BadRequest("Firma adı boş olamaz.");
        }

        // Firma adı üzerinden şirketi bul veya oluştur
        var existingCompany = await _context.Companies
            .FirstOrDefaultAsync(c => c.Name == request.CompanyName);

        if (existingCompany == null)
        {
            existingCompany = new Company { Name = request.CompanyName };
            _context.Companies.Add(existingCompany);
            await _context.SaveChangesAsync();
        }

        var newUser = new User
        {
            Email = request.Email,
            Password = request.Password, // Gerçek hayatta şifrelenir
            // Bu endpointi sadece yönetici kullanacağı için varsayılan rol Admin
            Role = "Admin",
            CompanyId = existingCompany.Id
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return Ok(new { Mesaj = "Kayıt başarılı!", Kullanici = newUser });
    }

    // --- 2. GİRİŞ YAP (LOGIN) VE YAKA KARTI VER ---
    [HttpPost("login")]
    [AllowAnonymous]
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

        // --- JWT ÜRETİM MERKEZİ (Program.cs'deki ayarlarla uyumlu) ---
        var tokenHandler = new JwtSecurityTokenHandler();
        var keyString = _configuration["Jwt:Key"] ?? "SeninCokGizliVeUzunJwtAnahtarin123!";
        var key = Encoding.UTF8.GetBytes(keyString);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role), // Rolü yaka kartına basıyoruz
                new Claim("company_id", user.CompanyId.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(2), // 2 saat geçerli
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
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