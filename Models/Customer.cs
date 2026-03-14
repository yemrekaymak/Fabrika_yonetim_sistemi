using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FabrikaBackend.Models;

public class Customer
{
    [Key]
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty; // Controller arka planda otomatik atayacak

    [JsonPropertyName("isimSoyisim")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("mail")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("tel")]
    public string Phone { get; set; } = string.Empty;

    // Soru işareti (?) bu alanın opsiyonel (boş geçilebilir) olduğunu gösterir!
    [JsonPropertyName("firmaIsmi")]
    public string? CompanyName { get; set; } 
}