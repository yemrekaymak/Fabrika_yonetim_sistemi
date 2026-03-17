using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FabrikaBackend.Models;

public class Product
{
    [Key]
    [JsonPropertyName("urun_kodu")]
    public string UrunKodu { get; set; } = string.Empty;

    [JsonPropertyName("urun_adi")]
    public string UrunAdi { get; set; } = string.Empty; // Yeni eklendi

    [JsonPropertyName("ham_madde")]
    public string HamMadde { get; set; } = string.Empty;

    [JsonPropertyName("malzeme_tipi")]
    public string MalzemeTipi { get; set; } = string.Empty;

    [JsonPropertyName("pres_kategorisi")]
    public string PresKategorisi { get; set; } = string.Empty;

    // Günlük üretim, Aylık kapasite, Malzeme verimi, Çalışan sayısı, Heat Treatment silindi.

    [JsonPropertyName("brut_agirlik_kg")]
    public double BrutAgirlikKg { get; set; }

    [JsonPropertyName("net_agirlik_kg")]
    public double NetAgirlikKg { get; set; }

    [JsonPropertyName("hurda_orani")]
    public double HurdaOrani { get; set; }

    [JsonPropertyName("base_cost")]
    public double BaseCost { get; set; }

    /// <summary>Birim satış fiyatı (₺). Yoksa maliyet ile aynı kabul edilir.</summary>
    [JsonPropertyName("sale_price")]
    public double? SalePrice { get; set; }

    /// <summary>Birim başına üretim süresi (saat). Tahmini bitiş süresi = miktar * BirimUretimSuresiSaat.</summary>
    [JsonPropertyName("birim_uretim_suresi_saat")]
    public double? BirimUretimSuresiSaat { get; set; }

    [JsonPropertyName("current_stock")]
    public int CurrentStock { get; set; } = 0; 

    [JsonPropertyName("machines")]
    public List<Machine> Machines { get; set; } = new List<Machine>();
}