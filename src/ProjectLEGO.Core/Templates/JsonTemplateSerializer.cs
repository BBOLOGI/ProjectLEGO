using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProjectLEGO.Core.Templates;

public sealed class JsonTemplateSerializer
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private readonly JsonSerializerOptions _options;

    public JsonTemplateSerializer()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };
        _options.Converters.Add(new JsonStringEnumConverter());
    }

    public LegoTemplate Deserialize(string json)
    {
        return JsonSerializer.Deserialize<LegoTemplate>(json, _options)
            ?? throw new JsonException("Template JSON resolved to null.");
    }

    public string Serialize(LegoTemplate template)
    {
        return JsonSerializer.Serialize(template, _options) + Environment.NewLine;
    }

    public LegoTemplate Load(string filePath)
    {
        return Deserialize(File.ReadAllText(filePath, Encoding.UTF8));
    }

    public void Save(string filePath, LegoTemplate template)
    {
        File.WriteAllText(filePath, Serialize(template), Utf8WithoutBom);
    }
}
