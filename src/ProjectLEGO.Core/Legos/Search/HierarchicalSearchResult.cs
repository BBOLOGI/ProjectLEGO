using ProjectLEGO.Core.Templates;

namespace ProjectLEGO.Core.Legos.Search;

public sealed record HierarchicalSearchResult(
    LegoTemplate Template,
    int MatchingRecordCount);

public sealed record LegoSpecificationOption(string Value, string DisplayName)
{
    public override string ToString() => DisplayName;
}
