using System.Text.Json.Serialization;

namespace FabrikaBackend.DTOs;

public class UserLoginDto
{
    // Front-end'den gelecek olan "email" verisini yakalar
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    // Front-end'den gelecek olan "password" verisini yakalar
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}