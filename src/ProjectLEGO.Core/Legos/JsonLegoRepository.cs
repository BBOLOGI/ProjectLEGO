using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using ProjectLEGO.Core.Templates;

namespace ProjectLEGO.Core.Legos;

public sealed class JsonLegoRepository : ILegoRepository
{
    private readonly string _directoryPath;
    private readonly IReadOnlyDictionary<string, LegoTemplate> _templates;
    private readonly LegoRecordValidator _validator = new();
    private readonly JsonSerializerOptions _options;

    public JsonLegoRepository(string directoryPath, IEnumerable<LegoTemplate> templates)
    {
        _directoryPath = directoryPath;
        _templates = templates.ToDictionary(template => template.TemplateId, StringComparer.OrdinalIgnoreCase);
        _options = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        _options.Converters.Add(new JsonStringEnumConverter());
    }

    public LegoRecordLoadResult LoadAll()
    {
        var records = new List<LegoRecord>();
        var errors = new List<LegoRecordLoadError>();
        if (!Directory.Exists(_directoryPath))
        {
            return new(records, [new(_directoryPath, "DirectoryNotFound", "LEGO directory does not exist.")]);
        }

        var ids = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in Directory.GetFiles(_directoryPath, "*.json", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                var json = File.ReadAllText(file, Encoding.UTF8);
                var record = JsonSerializer.Deserialize<LegoRecord>(json, _options) ?? throw new JsonException("LEGO JSON resolved to null.");
                var validation = _validator.Validate(record, _templates);
                if (!validation.IsValid)
                {
                    errors.Add(new(file, "ValidationFailed", string.Join("; ", validation.Errors.Select(error => $"{error.Path}: {error.Message}"))));
                    continue;
                }

                if (ids.TryGetValue(record.LegoId, out var original))
                {
                    errors.Add(new(file, "DuplicateLegoId", $"LegoId '{record.LegoId}' is already defined by '{original}'."));
                    continue;
                }

                ids.Add(record.LegoId, file);
                records.Add(record);
            }
            catch (JsonException exception)
            {
                errors.Add(new(file, "InvalidJson", exception.Message));
            }
            catch (IOException exception)
            {
                errors.Add(new(file, "ReadFailed", exception.Message));
            }
            catch (UnauthorizedAccessException exception)
            {
                errors.Add(new(file, "ReadFailed", exception.Message));
            }
        }

        return new(records, errors);
    }
}
