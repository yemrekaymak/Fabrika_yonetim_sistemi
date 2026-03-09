using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization; // JSON çevirmenleri için alet çantamız geldi!

namespace FabrikaBackend.Models
{
    public class Personnel
    {
        // Temel Bilgiler
        [Key]
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        // MULTI-TENANT: Bu personel hangi fabrikada çalışıyor?
        [JsonPropertyName("companyId")]
        public int CompanyId { get; set; } 

        [Required(ErrorMessage = "Ad zorunludur.")]
        [JsonPropertyName("ad")]
        public string FirstName { get; set; } = string.Empty; // Sarı uyarıları önlemek için boş atandı

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [JsonPropertyName("soyad")]
        public string LastName { get; set; } = string.Empty;

        // TC KİMLİK KISITLAMASI (Tam 11 hane ve sadece rakam)
        [Required(ErrorMessage = "TC Kimlik No zorunludur.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No tam 11 haneli olmalıdır.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "TC Kimlik No sadece rakamlardan oluşmalıdır.")]
        [JsonPropertyName("tcNo")]
        public string TcNo { get; set; } = string.Empty;

        // TELEFON KISITLAMASI (0 ile başlar, tam 11 hane)
        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Telefon numarası 11 haneli olmalıdır (Örn: 05551234567).")]
        [RegularExpression("^0[0-9]{10}$", ErrorMessage = "Telefon numarası 0 ile başlamalı ve sadece rakam içermelidir.")]
        [JsonPropertyName("telefon")]
        public string PhoneNumber { get; set; } = string.Empty;

        // İş Bilgileri
        [JsonPropertyName("departman")]
        public string Department { get; set; } = string.Empty;

        [JsonPropertyName("pozisyon")]
        public string Position { get; set; } = string.Empty;

        [JsonPropertyName("maas")]
        public decimal Salary { get; set; }

        [JsonPropertyName("ise_giris_tarihi")]
        public DateTime HireDate { get; set; }

        // İzin Bilgileri
        [JsonPropertyName("yillik_izin_hakki")]
        public int TotalAnnualLeave { get; set; }

        [JsonPropertyName("kullanilan_izin")]
        public int UsedLeave { get; set; }

        [JsonPropertyName("kalan_izin")]
        public int RemainingLeave { get; set; }

        // Performans ve Üretim
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

        // Acil Durum Bilgileri
        [JsonPropertyName("acil_durum_kisi")]
        public string EmergencyContactName { get; set; } = string.Empty;

        [StringLength(11, MinimumLength = 11, ErrorMessage = "Acil durum telefonu 11 haneli olmalıdır.")]
        [RegularExpression("^0[0-9]{10}$", ErrorMessage = "Acil durum telefonu geçerli bir formatta olmalıdır.")]
        [JsonPropertyName("acil_durum_tel")]
        public string EmergencyContactPhone { get; set; } = string.Empty;

        // Durum
        [JsonPropertyName("aktif")]
        public bool IsActive { get; set; } = true;
    }
}