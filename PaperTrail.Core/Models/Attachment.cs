using System.ComponentModel.DataAnnotations;

namespace PaperTrail.Core.Models;

public class Attachment
{
    [Key]
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public Contract Contract { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? Hash { get; set; }
    public DateTime CreatedUtc { get; set; }
    public bool? MissingFile { get; set; }
}
