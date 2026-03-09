using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FabrikaBackend.Models;

public class Customer
{
    [Key]
    [JsonPropertyName("id")]
    public int Id { get; set; } // int yaparsak DB otomatik 1, 2, 3 diye atar

    [Required] // Boş geçilemez
    [JsonPropertyName("isimSoyisim")]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress] // Email formatı kontrolü
    [JsonPropertyName("mail")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("tel")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("firmaIsmi")]
    public string? CompanyName { get; set; }

    [JsonPropertyName("kayitTarihi")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}