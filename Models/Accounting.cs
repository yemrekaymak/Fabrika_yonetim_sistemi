using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FabrikaBackend.Models;

// --- GİDERLER (Sabit ve Değişken Ayrımı Var) ---
public class Expense
{
    [Key]
    [JsonPropertyName("gider_adi")]
    public string GiderAdi { get; set; } = string.Empty; // Örn: "Elektrik", "Hammadde"

    [Required]
    [JsonPropertyName("tutar")]
    public double Tutar { get; set; }

    [Required]
    [JsonPropertyName("gider_tipi")]
    public string GiderTipi { get; set; } = "Sabit"; // "Sabit" veya "Değişken"

    [JsonPropertyName("tarih")]
    public DateTime Tarih { get; set; } = DateTime.Now;
}

// --- GELİRLER (Sadece Satış Odaklı) ---
public class Income
{
    [Key]
    [JsonPropertyName("gelir_adi")]
    public string GelirAdi { get; set; } = string.Empty; // Örn: "ABC Ltd. Şti Satışı"

    [Required]
    [JsonPropertyName("tutar")]
    public double Tutar { get; set; }

    [JsonPropertyName("tarih")]
    public DateTime Tarih { get; set; } = DateTime.Now;
}