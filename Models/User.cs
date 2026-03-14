using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FabrikaBackend.Models;

public class User
{
    [Key]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty; // Gerçek hayatta şifrelenir ama şimdilik düz tutalım

    [JsonPropertyName("role")]
    public string Role { get; set; } = "Personel"; // "Admin" veya "Personel" olacak
}