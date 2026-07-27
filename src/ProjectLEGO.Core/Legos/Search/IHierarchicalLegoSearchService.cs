namespace ProjectLEGO.Core.Legos.Search;

public interface IHierarchicalLegoSearchService
{
    IReadOnlyList<HierarchicalSearchResult> Search(string? keyword);
    IReadOnlyList<LegoSpecificationOption> GetSpec1(string templateId);
    IReadOnlyList<LegoSpecificationOption> GetSpec2(string templateId, string spec1);
    IReadOnlyList<LegoSpecificationOption> GetSpec3(string templateId, string spec1, string spec2);
    IReadOnlyList<LegoSpecificationOption> GetSpec4(string templateId, string spec1, string spec2, string spec3);
    LegoRecord? FindFinalRecord(string templateId, string? spec1, string? spec2, string? spec3, string? spec4);
}
