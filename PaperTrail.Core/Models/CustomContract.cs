using System.ComponentModel.DataAnnotations;

namespace PaperTrail.Core.Models;

public class CustomContract
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;
}
