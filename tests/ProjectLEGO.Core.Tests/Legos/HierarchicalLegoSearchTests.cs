using ProjectLEGO.Core.Legos;
using ProjectLEGO.Core.Legos.Search;
using ProjectLEGO.Core.Templates;
using Xunit;

namespace ProjectLEGO.Core.Tests.Legos;

public sealed class HierarchicalLegoSearchTests
{
    [Theory]
    [InlineData("500×500", "500x500")]
    [InlineData("500 X 500", "500x500")]
    [InlineData("H=3.0", "h3.0")]
    public void Normalize_EquivalentFormatsMatch(string left, string right)
    {
        Assert.Equal(HierarchicalLegoSearchService.Normalize(left), HierarchicalLegoSearchService.Normalize(right));
    }

    [Fact] public void Search_EmptyReturnsAllRecords() => Assert.Equal(8, Service().Search(string.Empty).Count);
    [Fact] public void Search_NameUsesLikeMatching() => Assert.Equal(3, Service().Search("집수").Count);
    [Fact] public void Search_NumberFindsSpecifications() => Assert.Contains(Service().Search("1200"), result => result.Record.LegoId == "CB-1200");
    [Fact] public void Search_FormatIgnoresMultiplicationStyle() => Assert.Contains(Service().Search("1000 x 1000"), result => result.Record.LegoId == "CB-1000");
    [Fact] public void Search_DescriptionAndTagsAreIncluded() => Assert.Single(Service().Search("프리캐스트"));

    [Fact]
    public void Spec1_ReturnsDistinctOrderedOptions()
    {
        var options = Service().GetSpec1("retaining-wall");
        Assert.Equal(["3.0", "4.0", "5.0", "6.0"], options.Select(option => option.Value));
    }

    [Fact]
    public void Spec2_IsFilteredBySpec1()
    {
        var options = Service().GetSpec2("retaining-wall", "3.0");
        Assert.Single(options);
        Assert.Equal("gravity", options[0].Value);
    }

    [Fact]
    public void ChangingParentProducesDifferentChildOptions()
    {
        var service = Service();
        Assert.Equal("gravity", Assert.Single(service.GetSpec2("retaining-wall", "3.0")).Value);
        Assert.Equal("cantilever", Assert.Single(service.GetSpec2("retaining-wall", "4.0")).Value);
    }

    [Fact]
    public void FindFinalRecord_ReturnsOnlyCompleteUniqueSelection()
    {
        var service = Service();
        Assert.Null(service.FindFinalRecord("retaining-wall", "3.0", "gravity", null, null));
        Assert.Equal("RW-G3", service.FindFinalRecord("retaining-wall", "3.0", "gravity", "true", "false")?.LegoId);
    }

    [Fact]
    public void FindFinalRecord_SingleTemplateNeedsNoSpecifications()
    {
        Assert.Equal("CP-001", Service().FindFinalRecord("corrugated-pipe", null, null, null, null)?.LegoId);
    }

    [Fact]
    public void UiServiceDependsOnRepositoryContract()
    {
        ILegoRepository repository = new StubRepository(SampleRecords());
        var service = new HierarchicalLegoSearchService(repository, Templates());
        Assert.Equal(8, service.Search(null).Count);
    }

    private static HierarchicalLegoSearchService Service()
    {
        var templates = Templates();
        ILegoRepository repository = new JsonLegoRepository(Path.Combine(RepoRoot(), "data", "legos"), templates);
        return new(repository, templates);
    }

    private static IReadOnlyList<LegoTemplate> Templates() => new TemplateLoader().LoadAll(Path.Combine(RepoRoot(), "data", "templates")).Templates;
    private static IReadOnlyList<LegoRecord> SampleRecords()
    {
        var templates = Templates();
        return new JsonLegoRepository(Path.Combine(RepoRoot(), "data", "legos"), templates).LoadAll().Records;
    }

    private static string RepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "ProjectLEGO.sln"))) directory = directory.Parent;
        Assert.NotNull(directory);
        return directory.FullName;
    }

    private sealed class StubRepository(IReadOnlyList<LegoRecord> records) : ILegoRepository
    {
        public LegoRecordLoadResult LoadAll() => new(records, []);
    }
}
