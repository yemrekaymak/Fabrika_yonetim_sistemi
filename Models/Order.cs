using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FabrikaBackend.Models;

public class Order
{
    [Key]
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("product_id")]
    public string ProductId { get; set; }

    [JsonPropertyName("musteri_adi")]
    public string? MusteriAdi { get; set; }

    [JsonPropertyName("urun_kodu")]
    public string? UrunKodu { get; set; }

    [JsonPropertyName("urun_adi")]
    public string? UrunAdi { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("estimated_hour")]
    public double EstimatedDays { get; set; }

    [JsonPropertyName("total_cost")]    
    public double TotalCost { get; set; }

    [JsonPropertyName("sale_price")]
    public double SalePrice { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "pending";

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}