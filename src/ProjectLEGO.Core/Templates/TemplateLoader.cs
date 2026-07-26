using System.Text.Json;

namespace ProjectLEGO.Core.Templates;

public sealed class TemplateLoader
{
    private readonly JsonTemplateSerializer _serializer;
    private readonly TemplateValidator _validator;

    public TemplateLoader(JsonTemplateSerializer? serializer = null, TemplateValidator? validator = null)
    {
        _serializer = serializer ?? new();
        _validator = validator ?? new();
    }

    public TemplateLoadResult LoadAll(string directoryPath, bool includeSubdirectories = true)
    {
        var templates = new List<LegoTemplate>();
        var errors = new List<TemplateLoadError>();
        if (!Directory.Exists(directoryPath))
        {
            errors.Add(new(directoryPath, "DirectoryNotFound", "Template directory does not exist."));
            return new(templates, errors);
        }

        var searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var files = Directory.GetFiles(directoryPath, "*.json", searchOption)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase);
        var ids = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            try
            {
                var template = _serializer.Load(file);
                var validation = _validator.Validate(template);
                if (!validation.IsValid)
                {
                    errors.Add(new(file, "ValidationFailed", string.Join("; ", validation.Errors.Select(error => $"{error.Path}: {error.Message}"))));
                    continue;
                }

                if (ids.TryGetValue(template.TemplateId, out var originalFile))
                {
                    errors.Add(new(file, "DuplicateTemplateId", $"TemplateId '{template.TemplateId}' is already defined by '{originalFile}'."));
                    continue;
                }

                ids.Add(template.TemplateId, file);
                templates.Add(template);
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

        return new(templates, errors);
    }
}
