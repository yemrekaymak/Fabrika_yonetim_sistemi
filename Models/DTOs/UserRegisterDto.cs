using System.Text.Json.Serialization;

namespace FabrikaBackend.DTOs;

public class UserRegisterDto
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    // Şifre tekrar alanı (frontend'de "şifre tekrarı")
    [JsonPropertyName("confirmPassword")]
    public string ConfirmPassword { get; set; } = string.Empty;

    // Firma adı (A Firması, B Firması gibi)
    [JsonPropertyName("companyName")]
    public string CompanyName { get; set; } = string.Empty;
}