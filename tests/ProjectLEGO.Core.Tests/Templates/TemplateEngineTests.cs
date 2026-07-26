using System.Text;
using ProjectLEGO.Core.Templates;
using Xunit;

namespace ProjectLEGO.Core.Tests.Templates;

public sealed class TemplateEngineTests
{
    [Fact]
    public void SampleTemplates_AllThreeLoad()
    {
        var manager = new TemplateManager(GetSampleDirectory());
        var result = manager.LoadAll();

        Assert.True(result.IsSuccessful, JoinErrors(result));
        Assert.Equal(3, result.Templates.Count);
    }

    [Fact]
    public void AddingJson_LoadsWithoutSourceChange()
    {
        using var directory = new TemporaryDirectory();
        WriteTemplate(directory.Path, "first.json", CreateTemplate("first"));
        var manager = new TemplateManager(directory.Path);
        Assert.Single(manager.LoadAll().Templates);

        WriteTemplate(directory.Path, "second.json", CreateTemplate("second"));
        Assert.Equal(2, manager.Reload().Templates.Count);
    }

    [Fact]
    public void InvalidJson_DoesNotStopOtherFiles()
    {
        using var directory = new TemporaryDirectory();
        WriteTemplate(directory.Path, "valid.json", CreateTemplate("valid"));
        File.WriteAllText(Path.Combine(directory.Path, "invalid.json"), "{ invalid", Encoding.UTF8);

        var result = new TemplateLoader().LoadAll(directory.Path);

        Assert.Single(result.Templates);
        Assert.Contains(result.Errors, error => error.Code == "InvalidJson");
    }

    [Fact]
    public void NullCollection_DoesNotStopOtherFiles()
    {
        using var directory = new TemporaryDirectory();
        WriteTemplate(directory.Path, "valid.json", CreateTemplate("valid"));
        File.WriteAllText(
            Path.Combine(directory.Path, "null-fields.json"),
            """{"TemplateId":"bad","Name":"bad","SchemaVersion":"1.0","TemplateType":"Single","Fields":null}""",
            Encoding.UTF8);

        var result = new TemplateLoader().LoadAll(directory.Path);

        Assert.Single(result.Templates);
        Assert.Contains(result.Errors, error => error.Code == "ValidationFailed");
    }

    [Fact]
    public void DuplicateTemplateId_IsDetected()
    {
        using var directory = new TemporaryDirectory();
        WriteTemplate(directory.Path, "one.json", CreateTemplate("same"));
        WriteTemplate(directory.Path, "two.json", CreateTemplate("same"));

        var result = new TemplateLoader().LoadAll(directory.Path);

        Assert.Single(result.Templates);
        Assert.Contains(result.Errors, error => error.Code == "DuplicateTemplateId");
    }

    [Fact]
    public void DuplicateFieldId_IsDetected()
    {
        var template = CreateTemplate("duplicate-field");
        template.Fields = [CreateTextField("same", "first", 0), CreateTextField("same", "second", 1)];

        var result = new TemplateValidator().Validate(template);

        Assert.Contains(result.Errors, error => error.Path.EndsWith("FieldId", StringComparison.Ordinal));
    }

    [Fact]
    public void DuplicateInternalName_IsDetected()
    {
        var template = CreateTemplate("duplicate-name");
        template.Fields = [CreateTextField("first", "same", 0), CreateTextField("second", "same", 1)];

        var result = new TemplateValidator().Validate(template);

        Assert.Contains(result.Errors, error => error.Path.EndsWith("InternalName", StringComparison.Ordinal));
    }

    [Fact]
    public void SelectWithoutOptions_IsInvalid()
    {
        var template = CreateTemplate("select-empty");
        template.Fields = [new TemplateField { FieldId = "kind", InternalName = "kind", DisplayName = "종류", DataType = TemplateDataType.Select }];

        var result = new TemplateValidator().Validate(template);

        Assert.Contains(result.Errors, error => error.Path.EndsWith("Options", StringComparison.Ordinal));
    }

    [Fact]
    public void DuplicateSelectOptionValue_IsDetected()
    {
        var template = CreateTemplate("select-duplicate");
        template.Fields =
        [
            new TemplateField
            {
                FieldId = "kind", InternalName = "kind", DisplayName = "종류", DataType = TemplateDataType.Select,
                Options =
                [
                    new TemplateOption { Value = "same", DisplayName = "첫 번째" },
                    new TemplateOption { Value = "same", DisplayName = "두 번째", SortOrder = 1 }
                ]
            }
        ];

        var result = new TemplateValidator().Validate(template);

        Assert.Contains(result.Errors, error => error.Message.Contains("Duplicate option Value", StringComparison.Ordinal));
    }

    [Fact]
    public void SavedTemplate_ReloadsWithValuesPreserved()
    {
        using var directory = new TemporaryDirectory();
        var template = CreateTemplate("korean-roundtrip");
        template.Name = "한글 이름";
        template.Description = "한글 설명이 손상되지 않습니다.";
        var manager = new TemplateManager(directory.Path);

        var save = manager.SaveTemplate(template);
        var load = manager.Reload();

        Assert.True(save.Success);
        Assert.True(load.IsSuccessful, JoinErrors(load));
        var loaded = Assert.Single(load.Templates);
        Assert.Equal(template.Name, loaded.Name);
        Assert.Equal(template.Description, loaded.Description);
    }

    [Fact]
    public void MissingTemplate_ReturnsNull()
    {
        using var directory = new TemporaryDirectory();
        var manager = new TemplateManager(directory.Path);
        manager.LoadAll();

        Assert.Null(manager.GetTemplate("does-not-exist"));
    }

    [Fact]
    public void KoreanText_RoundTripsAsUtf8()
    {
        var serializer = new JsonTemplateSerializer();
        var template = CreateTemplate("utf8");
        template.Name = "옹벽";
        template.Description = "배면성토와 내진 고려";

        var json = serializer.Serialize(template);
        var loaded = serializer.Deserialize(json);

        Assert.Contains("옹벽", json, StringComparison.Ordinal);
        Assert.Equal(template.Description, loaded.Description);
    }

    [Fact]
    public void SingleTemplate_WithNoFields_IsValid()
    {
        var template = CreateTemplate("corrugated-pipe");
        template.TemplateType = TemplateType.Single;

        Assert.True(new TemplateValidator().Validate(template).IsValid);
    }

    [Fact]
    public void UnknownJsonProperty_IsIgnored()
    {
        const string json = """
            {
              "TemplateId": "future",
              "Name": "미래 형식",
              "Category": "테스트",
              "Description": "확장 속성 테스트",
              "TemplateType": "Single",
              "SchemaVersion": "1.0",
              "Fields": [],
              "FutureProperty": { "enabled": true }
            }
            """;

        var template = new JsonTemplateSerializer().Deserialize(json);

        Assert.Equal("future", template.TemplateId);
    }

    private static LegoTemplate CreateTemplate(string id) => new()
    {
        TemplateId = id,
        Name = id,
        Category = "test",
        Description = "test template",
        TemplateType = TemplateType.Single,
        SchemaVersion = "1.0"
    };

    private static TemplateField CreateTextField(string fieldId, string internalName, int sortOrder) => new()
    {
        FieldId = fieldId,
        InternalName = internalName,
        DisplayName = internalName,
        DataType = TemplateDataType.Text,
        SortOrder = sortOrder
    };

    private static void WriteTemplate(string directory, string fileName, LegoTemplate template)
    {
        new JsonTemplateSerializer().Save(Path.Combine(directory, fileName), template);
    }

    private static string GetSampleDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "ProjectLEGO.sln")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return Path.Combine(directory.FullName, "data", "templates");
    }

    private static string JoinErrors(TemplateLoadResult result) =>
        string.Join(Environment.NewLine, result.Errors.Select(error => $"{error.Code}: {error.Message}"));

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ProjectLEGO.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
    }
}
