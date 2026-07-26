namespace ProjectLEGO.Core.Templates;

public sealed class LegoTemplate
{
    public string TemplateId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TemplateType TemplateType { get; set; }
    public string SchemaVersion { get; set; } = string.Empty;
    public List<TemplateField> Fields { get; set; } = [];
}
