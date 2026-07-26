namespace ProjectLEGO.Core.Templates;

public sealed record TemplateLoadError(string FilePath, string Code, string Message);

public sealed class TemplateLoadResult
{
    public TemplateLoadResult(
        IReadOnlyList<LegoTemplate> templates,
        IReadOnlyList<TemplateLoadError> errors)
    {
        Templates = templates;
        Errors = errors;
    }

    public IReadOnlyList<LegoTemplate> Templates { get; }
    public IReadOnlyList<TemplateLoadError> Errors { get; }
    public bool IsSuccessful => Errors.Count == 0;
}
