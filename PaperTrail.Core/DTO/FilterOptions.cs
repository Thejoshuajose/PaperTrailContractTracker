using PaperTrail.Core.Models;

namespace PaperTrail.Core.DTO;

/// <summary>
/// Options used when filtering <see cref="Contract"/> queries.  These are
/// deliberately lightweight as they are used both by the repository and by
/// various view models.
/// </summary>
public class FilterOptions
{
    /// <summary>
    /// Free form search text that should be matched against the contract title,
    /// counter party name and tags.  The search is case insensitive.
    /// </summary>
    public string? SearchText { get; set; }

    /// <summary>
    /// Optional set of statuses to include.  When <c>null</c> or empty all
    /// statuses are returned.
    /// </summary>
    public ContractStatus[]? Statuses { get; set; }

    /// <summary>
    /// A collection of tags to filter by.  Tags are normalised using
    /// <see cref="NormalizeTags"/> before comparison.
    /// </summary>
    public string[]? Tags { get; set; }

    /// <summary>
    /// Optional renewal date range.
    /// </summary>
    public DateOnly? RenewalFrom { get; set; }
    public DateOnly? RenewalTo { get; set; }

    /// <summary>
    /// Returns the set of normalised tags.  Tags are split on comma or
    /// semicolon, trimmed and converted to lower case.
    /// </summary>
    public IEnumerable<string> NormalizeTags() =>
        Tags == null
            ? Enumerable.Empty<string>()
            : Tags
                .SelectMany(t => t.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(t => t.Trim().ToLowerInvariant())
                .Where(t => !string.IsNullOrWhiteSpace(t));
}
