namespace ProjectLEGO.Core.Legos.Search;

public sealed class LegoSearchRequest
{
    public string? Keyword { get; set; }
    public string? TemplateId { get; set; }
    public string? Category { get; set; }
    public IReadOnlyCollection<LegoSearchFilter> Filters { get; set; } = [];
    public LegoSortOption Sort { get; set; } = LegoSortOption.Relevance;
}
