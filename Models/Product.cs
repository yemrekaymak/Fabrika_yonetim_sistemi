using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FabrikaBackend.Models;

public class Product
{
    [Key]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("urun_kodu")]
    public string UrunKodu { get; set; } = string.Empty;

    [JsonPropertyName("ham_madde")]
    public string HamMadde { get; set; } = string.Empty;

    [JsonPropertyName("malzeme_tipi")]
    public string MalzemeTipi { get; set; } = string.Empty;

    [JsonPropertyName("pres_kategorisi")]
    public string PresKategorisi { get; set; } = string.Empty;

    [JsonPropertyName("gunluk_uretim")]
    public int GunlukUretim { get; set; }

    [NotMapped]
    [JsonPropertyName("net_daily_capacity")]
    public double NetDailyCapacity => GunlukUretim * 0.85; 

    [NotMapped]
    [JsonPropertyName("monthly_capacity")]
    public double MonthlyCapacity => NetDailyCapacity * 22;

    [JsonPropertyName("brut_agirlik_kg")]
    public double BrutAgirlikKg { get; set; }

    [JsonPropertyName("net_agirlik_kg")]
    public double NetAgirlikKg { get; set; }

    [JsonPropertyName("hurda_orani")]
    public double HurdaOrani { get; set; }

    [JsonPropertyName("malzeme_verimi")]
    public double MalzemeVerimi { get; set; }

    [JsonPropertyName("calisan_sayisi")]
    public int CalisanSayisi { get; set; }

    [JsonPropertyName("has_heat_treatment")]
    public bool HasHeatTreatment { get; set; }

    [JsonPropertyName("base_cost")]
    public double BaseCost { get; set; }

    [JsonPropertyName("machines")]
    public List<Machine> Machines { get; set; } = new List<Machine>();
    
    [JsonPropertyName("current_stock")]
    public int CurrentStock { get; set; } = 0; 
}
