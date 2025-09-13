using System.ComponentModel.DataAnnotations;

namespace PaperTrail.Core.Models;

public class Party
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
