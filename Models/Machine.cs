using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FabrikaBackend.Models;

public class Machine
{
    [Key]
    public int Id { get; set; } // Veritabanı için gizli kimlik

    public int ProductId { get; set; } // Bu makine hangi ürüne ait?

    [JsonPropertyName("machine_name")]
    public string MachineName { get; set; } = string.Empty;

    [JsonPropertyName("is_used")]
    public bool IsUsed { get; set; }
}