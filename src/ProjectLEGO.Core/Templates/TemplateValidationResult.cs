namespace ProjectLEGO.Core.Templates;

public sealed record TemplateValidationError(string Path, string Message);

public sealed class TemplateValidationResult
{
    public TemplateValidationResult(IReadOnlyList<TemplateValidationError> errors)
    {
        Errors = errors;
    }

    public IReadOnlyList<TemplateValidationError> Errors { get; }
    public bool IsValid => Errors.Count == 0;
}
