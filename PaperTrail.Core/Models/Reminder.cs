using System.ComponentModel.DataAnnotations;

namespace PaperTrail.Core.Models;

public class Reminder
{
    [Key]
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public Contract Contract { get; set; } = null!;
    public ReminderType Type { get; set; }
    public DateTime DueUtc { get; set; }
    public DateTime? CompletedUtc { get; set; }
    public DateTime CreatedUtc { get; set; }
}
