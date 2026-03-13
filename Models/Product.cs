using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FabrikaBackend.Models;

public class Product
{
    [Key]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("urun_kodu")]
    public string UrunKodu { get; set; } = string.Empty;

    [JsonPropertyName("urun_adi")]
    public string UrunAdi { get; set; } = string.Empty;

    // Arkadaşının "—" gördüğü eksik alanlar:
    [JsonPropertyName("brut_agirlik")]
    public double BrutAgirlik { get; set; }

    [JsonPropertyName("net_agirlik")]
    public double NetAgirlik { get; set; }

    [JsonPropertyName("hurda_orani")]
    public double HurdaOrani { get; set; }

    // Grafik ve Kapasite için gereken alanlar:
    [JsonPropertyName("kapasite")]
    public int Kapasite { get; set; }

    [JsonPropertyName("kritik_seviye")]
    public int KritikSeviye { get; set; } = 100;

    [JsonPropertyName("miktar")] // Frontend'deki miktarSayi buna bağlanacak
    public int CurrentStock { get; set; }

    // Maliyet ve Fiyat alanları (Tabloda 0.00 görünmemesi için):
    [JsonPropertyName("maliyet")]
    public double Maliyet { get; set; }

    [JsonPropertyName("fiyat")]
    public double Fiyat { get; set; }

    // Hangi firmaya ait ürün
    [JsonPropertyName("companyId")]
    public int CompanyId { get; set; }
}