using PaperTrail.Core.Models;

namespace PaperTrail.Core.DTO;

public class FilterOptions
{
    public string? Search { get; set; }
    public ContractStatus? Status { get; set; }
    public string? Tags { get; set; }
}
