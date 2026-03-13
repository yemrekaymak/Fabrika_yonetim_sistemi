using System.ComponentModel.DataAnnotations;

namespace FabrikaBackend.Models;

public class Company
{
    [Key]
    public int Id { get; set; }

    // Firma adı (ör: A Firması, B Firması)
    public string Name { get; set; } = string.Empty;
}

