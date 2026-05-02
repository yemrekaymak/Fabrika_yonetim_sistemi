using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FabrikaBackend.Models
{
    public class Personnel
    {
        [Required(ErrorMessage = "Ad zorunludur.")]
        [JsonPropertyName("ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [JsonPropertyName("soyad")]
        public string LastName { get; set; } = string.Empty;

        [Key]
        [Required(ErrorMessage = "TC Kimlik No zorunludur.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No tam 11 haneli olmalıdır.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "TC Kimlik No sadece rakamlardan oluşmalıdır.")]
        [JsonPropertyName("tcNo")]
        public string TcNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Telefon numarası 11 haneli olmalıdır.")]
        [RegularExpression("^0[0-9]{10}$", ErrorMessage = "Telefon numarası 0 ile başlamalıdır.")]
        [JsonPropertyName("telefon")]
        public string PhoneNumber { get; set; } = string.Empty;

        [JsonPropertyName("personel_id")]
        public string PersonelCode { get; set; } = string.Empty;

        [JsonPropertyName("departman")]
        public string Department { get; set; } = string.Empty;

        [JsonPropertyName("pozisyon")]
        public string Position { get; set; } = string.Empty;

        [JsonPropertyName("maas")]
        public decimal Salary { get; set; }

        [JsonPropertyName("yol_ucreti")]
        public decimal TransportAllowance { get; set; }

        [JsonPropertyName("yemek_ucreti")]
        public decimal MealAllowance { get; set; }

        [JsonPropertyName("ise_giris_tarihi")]
        public DateTime HireDate { get; set; }

        [JsonPropertyName("yillik_izin_hakki")]
        public int TotalAnnualLeave { get; set; }

        [JsonPropertyName("kullanilan_izin")]
        public int UsedLeave { get; set; }

        [JsonPropertyName("kalan_izin")]
        public int RemainingLeave { get; set; }

        [JsonPropertyName("performans_puani")]
        public double PerformanceScore { get; set; }

        [JsonPropertyName("ortalama_gunluk_uretim")]
        public double AverageDailyProduction { get; set; }

        [JsonPropertyName("devamsizlik_gun")]
        public int AbsenteeismDays { get; set; }

        [JsonPropertyName("fazla_mesai_saat")]
        public double OvertimeHours { get; set; }

        [JsonPropertyName("egitim_sertifikalari")]
        public string Certifications { get; set; } = string.Empty;

        [JsonPropertyName("acil_durum_kisi")]
        public string EmergencyContactName { get; set; } = string.Empty;

        [JsonPropertyName("acil_durum_tel")]
        public string EmergencyContactPhone { get; set; } = string.Empty;

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; } = true;

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("absenteeism_rate")]
        public double AbsenteeismRate => TotalAnnualLeave > 0
            ? Math.Round((double)AbsenteeismDays / TotalAnnualLeave, 4)
            : 0;
    }
}
