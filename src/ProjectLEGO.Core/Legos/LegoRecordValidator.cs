using System.Text.Json;
using ProjectLEGO.Core.Templates;

namespace ProjectLEGO.Core.Legos;

public sealed class LegoRecordValidator
{
    public LegoRecordValidationResult Validate(LegoRecord? record, IReadOnlyDictionary<string, LegoTemplate> templates)
    {
        var errors = new List<LegoRecordValidationError>();
        if (record is null)
        {
            return new([new("$", "LEGO Record is required.")]);
        }

        Required(record.LegoId, "LegoId", errors);
        Required(record.TemplateId, "TemplateId", errors);
        Required(record.Name, "Name", errors);
        Required(record.Category, "Category", errors);
        Required(record.AssetPath, "AssetPath", errors);

        if (!templates.TryGetValue(record.TemplateId, out var template))
        {
            errors.Add(new("TemplateId", $"TemplateId '{record.TemplateId}' does not exist."));
            return new(errors);
        }

        if (record.Properties is null)
        {
            errors.Add(new("Properties", "Properties collection is required."));
            return new(errors);
        }

        var fields = template.Fields.ToDictionary(field => field.InternalName, StringComparer.OrdinalIgnoreCase);
        foreach (var property in record.Properties)
        {
            if (!fields.TryGetValue(property.Key, out var field))
            {
                errors.Add(new($"Properties.{property.Key}", "Property is not defined by the Template."));
                continue;
            }

            ValidateValue(property.Key, property.Value, field, errors);
        }

        foreach (var field in template.Fields.Where(field => field.Required))
        {
            if (!record.Properties.ContainsKey(field.InternalName))
            {
                errors.Add(new($"Properties.{field.InternalName}", "Required property is missing."));
            }
        }

        return new(errors);
    }

    private static void ValidateValue(string key, JsonElement value, TemplateField field, ICollection<LegoRecordValidationError> errors)
    {
        var valid = field.DataType switch
        {
            TemplateDataType.Number => value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out _),
            TemplateDataType.Text => value.ValueKind == JsonValueKind.String,
            TemplateDataType.Boolean => value.ValueKind is JsonValueKind.True or JsonValueKind.False,
            TemplateDataType.Select => value.ValueKind == JsonValueKind.String &&
                field.Options.Any(option => string.Equals(option.Value, value.GetString(), StringComparison.OrdinalIgnoreCase)),
            _ => false
        };

        if (!valid)
        {
            errors.Add(new($"Properties.{key}", $"Value does not match {field.DataType}."));
        }
    }

    private static void Required(string? value, string path, ICollection<LegoRecordValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(value)) errors.Add(new(path, $"{path} is required."));
    }
}
