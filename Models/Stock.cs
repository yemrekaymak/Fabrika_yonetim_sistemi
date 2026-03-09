using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FabrikaBackend.Models;

public class Stock
{
    [Key]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("stokKodu")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("ad")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("miktarSayi")]
    public double Quantity { get; set; }

    [JsonPropertyName("kapasite")]
    public double Capacity { get; set; }

    [JsonPropertyName("kritik")]
    public double CriticalLevel { get; set; }

    [JsonPropertyName("birimMaliyet")]
    public decimal UnitCost { get; set; }

    [JsonPropertyName("birimFiyat")]
    public decimal UnitPrice { get; set; }

    [JsonPropertyName("durum")]
    public string Status { get; set; } = string.Empty;
}