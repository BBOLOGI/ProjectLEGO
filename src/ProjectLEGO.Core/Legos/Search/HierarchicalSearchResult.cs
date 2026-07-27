using ProjectLEGO.Core.Templates;

namespace ProjectLEGO.Core.Legos.Search;

public sealed record HierarchicalSearchResult(
    LegoRecord Record,
    LegoTemplate Template,
    IReadOnlyList<string> Specifications);

public sealed record LegoSpecificationOption(string Value, string DisplayName)
{
    public override string ToString() => DisplayName;
}
