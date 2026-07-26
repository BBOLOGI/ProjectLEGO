namespace ProjectLEGO.Core.Templates;

public sealed class TemplateValidator
{
    public TemplateValidationResult Validate(LegoTemplate? template)
    {
        var errors = new List<TemplateValidationError>();
        if (template is null)
        {
            errors.Add(new("$", "Template is required."));
            return new(errors);
        }

        Required(template.TemplateId, "TemplateId", errors);
        Required(template.Name, "Name", errors);
        Required(template.SchemaVersion, "SchemaVersion", errors);
        if (!Enum.IsDefined(template.TemplateType))
        {
            errors.Add(new("TemplateType", "TemplateType is invalid."));
        }

        if (template.Fields is null)
        {
            errors.Add(new("Fields", "Fields collection is required."));
            return new(errors);
        }

        var fieldIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var internalNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < template.Fields.Count; index++)
        {
            var field = template.Fields[index];
            var path = $"Fields[{index}]";
            if (field is null)
            {
                errors.Add(new(path, "Field cannot be null."));
                continue;
            }

            Required(field.FieldId, $"{path}.FieldId", errors);
            Required(field.InternalName, $"{path}.InternalName", errors);
            Required(field.DisplayName, $"{path}.DisplayName", errors);

            if (!string.IsNullOrWhiteSpace(field.FieldId) && !fieldIds.Add(field.FieldId))
            {
                errors.Add(new($"{path}.FieldId", $"Duplicate FieldId '{field.FieldId}'."));
            }

            if (!string.IsNullOrWhiteSpace(field.InternalName) && !internalNames.Add(field.InternalName))
            {
                errors.Add(new($"{path}.InternalName", $"Duplicate InternalName '{field.InternalName}'."));
            }

            if (field.SortOrder < 0)
            {
                errors.Add(new($"{path}.SortOrder", "SortOrder cannot be negative."));
            }

            if (!Enum.IsDefined(field.DataType))
            {
                errors.Add(new($"{path}.DataType", "DataType is invalid."));
                continue;
            }

            if (field.Options is null)
            {
                errors.Add(new($"{path}.Options", "Options collection is required."));
                continue;
            }

            if (field.DataType == TemplateDataType.Select)
            {
                if (field.Options.Count == 0)
                {
                    errors.Add(new($"{path}.Options", "Select fields require at least one option."));
                }

                var values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                for (var optionIndex = 0; optionIndex < field.Options.Count; optionIndex++)
                {
                    var option = field.Options[optionIndex];
                    var optionPath = $"{path}.Options[{optionIndex}]";
                    if (option is null)
                    {
                        errors.Add(new(optionPath, "Option cannot be null."));
                        continue;
                    }

                    Required(option.Value, $"{optionPath}.Value", errors);
                    Required(option.DisplayName, $"{optionPath}.DisplayName", errors);
                    if (option.SortOrder < 0)
                    {
                        errors.Add(new($"{optionPath}.SortOrder", "SortOrder cannot be negative."));
                    }

                    if (!string.IsNullOrWhiteSpace(option.Value) && !values.Add(option.Value))
                    {
                        errors.Add(new($"{optionPath}.Value", $"Duplicate option Value '{option.Value}'."));
                    }
                }
            }
            else if (field.Options.Count != 0)
            {
                errors.Add(new($"{path}.Options", "Only Select fields can define options."));
            }

            if (field.DataType != TemplateDataType.Number && !string.IsNullOrWhiteSpace(field.Unit))
            {
                errors.Add(new($"{path}.Unit", "Only Number fields can define a unit."));
            }
        }

        return new(errors);
    }

    private static void Required(string? value, string path, ICollection<TemplateValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(new(path, $"{path} is required."));
        }
    }
}
