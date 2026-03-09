using System.Text.Json.Serialization;

namespace FabrikaBackend.DTOs;

public class UserRegisterDto
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}