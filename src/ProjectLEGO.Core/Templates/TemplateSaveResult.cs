namespace ProjectLEGO.Core.Templates;

public sealed class TemplateSaveResult
{
    private TemplateSaveResult(bool success, string? filePath, IReadOnlyList<TemplateValidationError> errors)
    {
        Success = success;
        FilePath = filePath;
        Errors = errors;
    }

    public bool Success { get; }
    public string? FilePath { get; }
    public IReadOnlyList<TemplateValidationError> Errors { get; }

    public static TemplateSaveResult Saved(string filePath) => new(true, filePath, []);
    public static TemplateSaveResult Invalid(IReadOnlyList<TemplateValidationError> errors) => new(false, null, errors);
}
