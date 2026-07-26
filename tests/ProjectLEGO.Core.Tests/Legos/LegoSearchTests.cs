using System.Text;
using System.Text.Json;
using ProjectLEGO.Core.Legos;
using ProjectLEGO.Core.Legos.Insert;
using ProjectLEGO.Core.Legos.Search;
using ProjectLEGO.Core.Templates;
using Xunit;

namespace ProjectLEGO.Core.Tests.Legos;

public sealed class LegoSearchTests
{
    [Fact] public void Repository_LoadsValidRecords() => Assert.Equal(8, LoadSamples().Records.Count);

    [Fact]
    public void Repository_LoadsSubfolders()
    {
        using var dir = new TempDirectory();
        Directory.CreateDirectory(Path.Combine(dir.Path, "nested"));
        WriteRecord(Path.Combine(dir.Path, "nested", "record.json"), Record("one", "corrugated-pipe"));
        Assert.Single(new JsonLegoRepository(dir.Path, Templates()).LoadAll().Records);
    }

    [Fact]
    public void Repository_IsolatesInvalidJson()
    {
        using var dir = new TempDirectory();
        WriteRecord(Path.Combine(dir.Path, "valid.json"), Record("one", "corrugated-pipe"));
        File.WriteAllText(Path.Combine(dir.Path, "bad.json"), "{bad", Encoding.UTF8);
        var result = new JsonLegoRepository(dir.Path, Templates()).LoadAll();
        Assert.Single(result.Records);
        Assert.Contains(result.Errors, error => error.Code == "InvalidJson");
    }

    [Fact]
    public void Repository_DetectsDuplicateLegoId()
    {
        using var dir = new TempDirectory();
        WriteRecord(Path.Combine(dir.Path, "one.json"), Record("same", "corrugated-pipe"));
        WriteRecord(Path.Combine(dir.Path, "two.json"), Record("same", "corrugated-pipe"));
        Assert.Contains(new JsonLegoRepository(dir.Path, Templates()).LoadAll().Errors, error => error.Code == "DuplicateLegoId");
    }

    [Fact] public void Validator_RejectsMissingTemplate() => Assert.Contains(Validate(Record("x", "missing")).Errors, error => error.Path == "TemplateId");

    [Fact]
    public void Validator_RejectsUnknownProperty()
    {
        var record = Record("x", "corrugated-pipe");
        record.Properties["unknown"] = Json("1");
        Assert.Contains(Validate(record).Errors, error => error.Path.Contains("unknown", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("width", "\"wrong\"")]
    [InlineData("shape", "\"triangle\"")]
    [InlineData("material", "true")]
    public void Validator_RejectsWrongPropertyTypes(string property, string json)
    {
        var record = CatchBasin("x", 1000, "concrete", "rectangular");
        record.Properties[property] = Json(json);
        Assert.False(Validate(record).IsValid);
    }

    [Fact]
    public void Search_ExcludesInactive()
    {
        var records = SampleRecords().ToList();
        records[0].IsActive = false;
        Assert.Equal(7, Service(records).Search(new()).Count);
    }

    [Fact] public void Search_EmptyKeywordReturnsAll() => Assert.Equal(8, Search().Count);
    [Fact] public void Search_NamePartialMatch() => Assert.Equal(3, Search(keyword: "집수정").Count);
    [Fact] public void Search_TagMatch() => Assert.Single(Search(keyword: "프리캐스트"));
    [Fact] public void Search_KoreanMatch() => Assert.Equal(4, Search(keyword: "옹벽").Count);
    [Fact] public void Search_TemplateFilter() => Assert.Equal(3, Search(templateId: "catch-basin").Count);
    [Fact] public void Search_CategoryFilter() => Assert.Equal(4, Search(category: "토공 구조물").Count);

    [Fact]
    public void Search_NumberRange()
    {
        var filters = new[] { new LegoSearchFilter { InternalName = "wallHeight", Operator = LegoFilterOperator.NumberRange, Minimum = 4, Maximum = 5 } };
        Assert.Equal(2, Search(templateId: "retaining-wall", filters: filters).Count);
    }

    [Fact]
    public void Search_BooleanFilter()
    {
        var filters = new[] { new LegoSearchFilter { InternalName = "seismicDesign", Operator = LegoFilterOperator.BooleanEquals, BooleanValue = true } };
        Assert.Equal(2, Search(templateId: "retaining-wall", filters: filters).Count);
    }

    [Fact]
    public void Search_SelectMultipleValuesUseOr()
    {
        var filters = new[] { new LegoSearchFilter { InternalName = "shape", Operator = LegoFilterOperator.SelectAny, SelectedValues = ["rectangular", "circular"] } };
        Assert.Equal(3, Search(templateId: "catch-basin", filters: filters).Count);
    }

    [Fact]
    public void Search_DifferentFiltersUseAnd()
    {
        var filters = new[]
        {
            new LegoSearchFilter { InternalName = "material", Operator = LegoFilterOperator.SelectAny, SelectedValues = ["concrete"] },
            new LegoSearchFilter { InternalName = "shape", Operator = LegoFilterOperator.SelectAny, SelectedValues = ["rectangular"] }
        };
        Assert.Single(Search(templateId: "catch-basin", filters: filters));
    }

    [Fact]
    public void Search_RelevanceIsDeterministic()
    {
        var first = Search(keyword: "집수정").Select(result => result.Record.LegoId).ToArray();
        var second = Search(keyword: "집수정").Select(result => result.Record.LegoId).ToArray();
        Assert.Equal(first, second);
    }

    [Fact]
    public void Search_SortsByNameAndUpdate()
    {
        var ascending = Search(sort: LegoSortOption.NameAscending);
        var descending = Search(sort: LegoSortOption.NameDescending);
        var recent = Search(sort: LegoSortOption.RecentlyUpdated);
        Assert.NotEqual(ascending[0].Record.LegoId, descending[0].Record.LegoId);
        Assert.Equal("RW-C6", recent[0].Record.LegoId);
    }

    [Fact]
    public void Search_SummaryFollowsSortOrder()
    {
        var result = Assert.Single(Search(templateId: "catch-basin", keyword: "CB-1000"));
        Assert.True(result.SpecificationSummary.IndexOf("가로", StringComparison.Ordinal) < result.SpecificationSummary.IndexOf("세로", StringComparison.Ordinal));
        Assert.True(result.SpecificationSummary.IndexOf("세로", StringComparison.Ordinal) < result.SpecificationSummary.IndexOf("높이", StringComparison.Ordinal));
    }

    [Fact]
    public void Search_NewTemplateAndRecordNeedsNoStructureCode()
    {
        var template = new LegoTemplate { TemplateId = "new-kind", Name = "새 구조물", Category = "시험", Description = "", SchemaVersion = "1.0", TemplateType = TemplateType.Single };
        var record = Record("NEW-1", "new-kind");
        var result = new LegoSearchService([record], [template]).Search(new LegoSearchRequest { Keyword = "새 구조물" });
        Assert.Single(result);
    }

    [Fact]
    public void InsertRequest_ContainsSelectedRecordData()
    {
        var record = SampleRecords().First();
        var request = LegoInsertRequestFactory.Create(record);
        Assert.NotNull(request);
        Assert.Equal(record.LegoId, request.LegoId);
        Assert.Equal(record.Properties, request.Properties);
    }

    [Fact] public void InsertRequest_NoSelectionReturnsNull() => Assert.Null(LegoInsertRequestFactory.Create(null));

    [Fact]
    public void Repository_PreservesKoreanNameAndTags()
    {
        var record = LoadSamples().Records.First(item => item.LegoId == "CP-001");
        Assert.Equal("파형강관 기본형", record.Name);
        Assert.Contains("배수", record.Tags);
    }

    private static LegoRecordValidationResult Validate(LegoRecord record) => new LegoRecordValidator().Validate(record, Templates().ToDictionary(template => template.TemplateId));
    private static IReadOnlyList<LegoSearchResult> Search(string? keyword = null, string? templateId = null, string? category = null, IReadOnlyCollection<LegoSearchFilter>? filters = null, LegoSortOption sort = LegoSortOption.Relevance) =>
        Service(SampleRecords()).Search(new LegoSearchRequest { Keyword = keyword, TemplateId = templateId, Category = category, Filters = filters ?? [], Sort = sort });
    private static LegoSearchService Service(IEnumerable<LegoRecord> records) => new(records, Templates());
    private static LegoRecordLoadResult LoadSamples() => new JsonLegoRepository(Path.Combine(RepoRoot(), "data", "legos"), Templates()).LoadAll();
    private static IReadOnlyList<LegoRecord> SampleRecords() => LoadSamples().Records;
    private static IReadOnlyList<LegoTemplate> Templates() => new TemplateLoader().LoadAll(Path.Combine(RepoRoot(), "data", "templates")).Templates;

    private static LegoRecord Record(string id, string templateId) => new()
    {
        LegoId = id, TemplateId = templateId, Name = id, Category = "시험", AssetPath = $"assets/{id}.dwg",
        CreatedAt = DateTimeOffset.Parse("2026-07-26T09:00:00+09:00"), UpdatedAt = DateTimeOffset.Parse("2026-07-26T09:00:00+09:00")
    };

    private static LegoRecord CatchBasin(string id, decimal width, string material, string shape)
    {
        var record = Record(id, "catch-basin");
        record.Properties = new()
        {
            ["width"] = Json(width.ToString(System.Globalization.CultureInfo.InvariantCulture)), ["length"] = Json("1000"), ["height"] = Json("1200"),
            ["material"] = Json($"\"{material}\""), ["shape"] = Json($"\"{shape}\"")
        };
        return record;
    }

    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();
    private static void WriteRecord(string path, LegoRecord record) => File.WriteAllText(path, JsonSerializer.Serialize(record), Encoding.UTF8);
    private static string RepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "ProjectLEGO.sln"))) directory = directory.Parent;
        Assert.NotNull(directory);
        return directory.FullName;
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory() { Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ProjectLEGO.Tests", Guid.NewGuid().ToString("N")); Directory.CreateDirectory(Path); }
        public string Path { get; }
        public void Dispose() { if (Directory.Exists(Path)) Directory.Delete(Path, true); }
    }
}
