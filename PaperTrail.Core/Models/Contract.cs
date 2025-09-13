using System.ComponentModel.DataAnnotations;

namespace PaperTrail.Core.Models;

public class Contract
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;
    public Guid? CounterpartyId { get; set; }
    public Party? Counterparty { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Active;
    public DateOnly? EffectiveDate { get; set; }
    public DateOnly? RenewalDate { get; set; }
    [Range(1, 600)]
    public int? RenewalTermMonths { get; set; }
    public DateOnly? TerminationDate { get; set; }
    [Range(0, 3650)]
    public int? NoticePeriodDays { get; set; }
    public string? Tags { get; set; }
    [Range(0, 999999999)]
    public decimal? ValueAmount { get; set; }
    public string? Notes { get; set; }
    public bool SensitiveFieldsEncrypted { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
}
