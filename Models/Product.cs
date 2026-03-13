using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FabrikaBackend.Models;

public class Product
{
    [Key]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [Required]
    [JsonPropertyName("urun_kodu")]
    public string UrunKodu { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("urun_adi")]
    public string UrunAdi { get; set; } = string.Empty;

    [JsonPropertyName("ham_madde_turu")]
    public string HamMaddeTuru { get; set; } = string.Empty;

    [JsonPropertyName("birim_uretim_suresi")]
    public int BirimUretimSuresi { get; set; }

    // Frontend Dropdown: "Dakika", "Saat" değerlerini alacak
    [JsonPropertyName("sure_birimi")] 
    public string SureBirimi { get; set; } = "Dakika";

    [JsonPropertyName("brut_agirlik_kg")]
    public int BrutAgirlikKg { get; set; }

    [JsonPropertyName("net_agirlik_kg")]
    public int NetAgirlikKg { get; set; }

    [JsonPropertyName("hurda_orani_yuzde")]
    public int HurdaOraniYuzde { get; set; }

    [JsonPropertyName("gunluk_uretim_kapasitesi")]
    public int GunlukUretimKapasitesi { get; set; }

    // Frontend Checkbox/Multi-select: ["1000 ton", "Eksantrik 80"] gibi bir liste tutar
    [JsonPropertyName("secili_makineler")]
    public List<string> SeciliMakineler { get; set; } = new List<string>();

    [JsonPropertyName("current_stock")]
    public int CurrentStock { get; set; } = 0;
}