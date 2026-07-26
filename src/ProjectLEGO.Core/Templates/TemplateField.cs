namespace ProjectLEGO.Core.Templates;

public sealed class TemplateField
{
    public string FieldId { get; set; } = string.Empty;
    public string InternalName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public TemplateDataType DataType { get; set; }
    public string? Unit { get; set; }
    public bool Required { get; set; }
    public bool Searchable { get; set; }
    public int SortOrder { get; set; }
    public List<TemplateOption> Options { get; set; } = [];
}
