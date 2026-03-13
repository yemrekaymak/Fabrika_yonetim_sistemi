using System;
using System.ComponentModel.DataAnnotations; // [Key] ve [Required] için gerekli
using System.Text.Json.Serialization;      // [JsonPropertyName] için gerekli

namespace FabrikaBackend.Models;

public class Orders
{
    [Key]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [Required]
    [JsonPropertyName("musteri_adi")]
    public string MusteriAdi { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("urun_adi")]
    public string UrunAdi { get; set; } = string.Empty;

    [JsonPropertyName("miktar")]
    public int Miktar { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "pending";

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}