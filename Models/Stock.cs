using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FabrikaBackend.Models;

public class Stock
{
    [Key] // Veritabanı için kimlik (Primary Key)
    [JsonPropertyName("kod")]
    public string Code { get; set; } = string.Empty; // Örn: STK-001

    [JsonPropertyName("ad")]
    public string Name { get; set; } = string.Empty;

    // Front-end'in istediği o ince detay: Biri ekranda göstermek, diğeri hesaplamak için!
    [JsonPropertyName("miktar")]
    public string QuantityText { get; set; } = string.Empty; // Örn: "450 Adet"

    [JsonPropertyName("miktarSayi")]
    public double Quantity { get; set; } // Örn: 450

    [JsonPropertyName("kapasite")]
    public double Capacity { get; set; }

    [JsonPropertyName("kritik")]
    public double CriticalLevel { get; set; }

    [JsonPropertyName("birimMaliyet")]
    public decimal UnitCost { get; set; }

    [JsonPropertyName("birimFiyat")]
    public decimal UnitPrice { get; set; }

    [JsonPropertyName("durum")]
    public string Status { get; set; } = string.Empty; // "kritik" veya "yeterli"
}