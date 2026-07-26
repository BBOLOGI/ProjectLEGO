namespace ProjectLEGO.Core.Legos.Search;

public enum LegoFilterOperator
{
    TextEquals,
    TextContains,
    NumberEquals,
    NumberRange,
    BooleanEquals,
    SelectAny
}

public sealed class LegoSearchFilter
{
    public string InternalName { get; set; } = string.Empty;
    public LegoFilterOperator Operator { get; set; }
    public string? TextValue { get; set; }
    public decimal? Minimum { get; set; }
    public decimal? Maximum { get; set; }
    public bool? BooleanValue { get; set; }
    public IReadOnlyCollection<string> SelectedValues { get; set; } = [];
}
