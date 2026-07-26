using System.Text;
using System.Security.Cryptography;

namespace ProjectLEGO.Core.Templates;

public sealed class TemplateManager
{
    private readonly string _directoryPath;
    private readonly TemplateLoader _loader;
    private readonly TemplateValidator _validator;
    private readonly JsonTemplateSerializer _serializer;
    private Dictionary<string, LegoTemplate> _templates = new(StringComparer.OrdinalIgnoreCase);

    public TemplateManager(string directoryPath)
    {
        _directoryPath = directoryPath;
        _serializer = new();
        _validator = new();
        _loader = new(_serializer, _validator);
    }

    public TemplateLoadResult LoadAll()
    {
        var result = _loader.LoadAll(_directoryPath);
        _templates = result.Templates.ToDictionary(template => template.TemplateId, StringComparer.OrdinalIgnoreCase);
        return result;
    }

    public TemplateLoadResult Reload() => LoadAll();

    public IReadOnlyList<LegoTemplate> GetAllTemplates()
    {
        return _templates.Values.OrderBy(template => template.Name, StringComparer.CurrentCulture).ToArray();
    }

    public LegoTemplate? GetTemplate(string templateId)
    {
        return _templates.GetValueOrDefault(templateId);
    }

    public TemplateSaveResult SaveTemplate(LegoTemplate template)
    {
        var validation = _validator.Validate(template);
        if (!validation.IsValid)
        {
            return TemplateSaveResult.Invalid(validation.Errors);
        }

        Directory.CreateDirectory(_directoryPath);
        var fileName = CreateSafeFileName(template.TemplateId);
        var destination = Path.Combine(_directoryPath, fileName);
        var temporary = Path.Combine(_directoryPath, $".{fileName}.{Guid.NewGuid():N}.tmp");
        try
        {
            _serializer.Save(temporary, template);
            File.Move(temporary, destination, true);
            _templates[template.TemplateId] = template;
            return TemplateSaveResult.Saved(destination);
        }
        finally
        {
            if (File.Exists(temporary))
            {
                File.Delete(temporary);
            }
        }
    }

    private static string CreateSafeFileName(string templateId)
    {
        var builder = new StringBuilder(templateId.Length);
        var previousWasDash = false;
        foreach (var character in templateId.ToLowerInvariant())
        {
            var safe = char.IsAsciiLetterOrDigit(character) || character is '-' or '_' or '.';
            var output = safe ? character : '-';
            if (output == '-' && previousWasDash)
            {
                continue;
            }

            builder.Append(output);
            previousWasDash = output == '-';
        }

        var name = builder.ToString().Trim('-', '.', '_');
        if (string.IsNullOrEmpty(name))
        {
            throw new InvalidOperationException("TemplateId cannot produce a safe file name.");
        }

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(templateId)))[..12].ToLowerInvariant();
        return $"{name}-{hash}.json";
    }
}
