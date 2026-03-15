using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FabrikaBackend.Models;

public class Machine
{
    [Key]
    public int Id { get; set; } // Makine ID (int)

    [JsonPropertyName("machine_name")]
    public string MachineName { get; set; } = string.Empty; // Makine Adı (string)

    [JsonPropertyName("details")]
    public string? Details { get; set; } // Opsiyonel Detay (string?)
}