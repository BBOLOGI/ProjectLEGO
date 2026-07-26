using ProjectLEGO.Core.Templates;

namespace ProjectLEGO.Core.Legos.Search;

public sealed record LegoSearchResult(LegoRecord Record, LegoTemplate Template, string SpecificationSummary, int RelevanceScore);
