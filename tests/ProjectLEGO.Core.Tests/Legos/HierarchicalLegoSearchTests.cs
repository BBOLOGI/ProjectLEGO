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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Search_EmptyReturnsNoResults(string? keyword) => Assert.Empty(Service().Search(keyword));

    [Fact]
    public void Search_NameGroupsMatchingRecordsByLegoType()
    {
        var result = Assert.Single(Service().Search("집수"));
        Assert.Equal("집수정", result.Template.Name);
        Assert.Equal(3, result.MatchingRecordCount);
    }

    [Fact] public void Search_NumberFindsMatchingLegoType() => Assert.Contains(Service().Search("1200"), result => result.Template.TemplateId == "catch-basin");
    [Fact] public void Search_FormatIgnoresMultiplicationStyle() => Assert.Equal("catch-basin", Assert.Single(Service().Search("1000 x 1000")).Template.TemplateId);
    [Fact] public void Search_DescriptionAndTagsAreIncluded() => Assert.Equal("catch-basin", Assert.Single(Service().Search("프리캐스트")).Template.TemplateId);

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
        Assert.Empty(service.Search(null));
        Assert.Single(service.Search("옹벽"));
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
