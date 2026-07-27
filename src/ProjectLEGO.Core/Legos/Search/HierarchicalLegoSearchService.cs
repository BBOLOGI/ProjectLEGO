using System.Globalization;
using System.Text;
using System.Text.Json;
using ProjectLEGO.Core.Templates;

namespace ProjectLEGO.Core.Legos.Search;

public sealed class HierarchicalLegoSearchService : IHierarchicalLegoSearchService
{
    private readonly IReadOnlyList<LegoRecord> _records;
    private readonly IReadOnlyDictionary<string, LegoTemplate> _templates;

    public HierarchicalLegoSearchService(ILegoRepository repository, IEnumerable<LegoTemplate> templates)
    {
        _templates = templates.ToDictionary(template => template.TemplateId, StringComparer.OrdinalIgnoreCase);
        _records = repository.LoadAll().Records.Where(record => record.IsActive && _templates.ContainsKey(record.TemplateId)).ToArray();
    }

    public IReadOnlyList<HierarchicalSearchResult> Search(string? keyword)
    {
        var normalized = Normalize(keyword);
        return _records
            .Where(record => normalized.Length == 0 || SearchValues(record, _templates[record.TemplateId]).Any(value => Normalize(value).Contains(normalized, StringComparison.Ordinal)))
            .Select(record => new HierarchicalSearchResult(record, _templates[record.TemplateId], GetFields(_templates[record.TemplateId]).Select(field => DisplayProperty(record, field)).ToArray()))
            .OrderBy(result => result.Template.Name, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(result => result.Record.Name, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(result => result.Record.LegoId, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public IReadOnlyList<LegoSpecificationOption> GetSpec1(string templateId) => GetOptions(templateId, 0, []);
    public IReadOnlyList<LegoSpecificationOption> GetSpec2(string templateId, string spec1) => GetOptions(templateId, 1, [spec1]);
    public IReadOnlyList<LegoSpecificationOption> GetSpec3(string templateId, string spec1, string spec2) => GetOptions(templateId, 2, [spec1, spec2]);
    public IReadOnlyList<LegoSpecificationOption> GetSpec4(string templateId, string spec1, string spec2, string spec3) => GetOptions(templateId, 3, [spec1, spec2, spec3]);

    public LegoRecord? FindFinalRecord(string templateId, string? spec1, string? spec2, string? spec3, string? spec4)
    {
        if (!_templates.TryGetValue(templateId, out var template)) return null;
        var fields = GetFields(template);
        var selections = new[] { spec1, spec2, spec3, spec4 };
        if (fields.Count > selections.Length || selections.Take(fields.Count).Any(string.IsNullOrWhiteSpace)) return null;
        var candidates = Filter(templateId, selections.Take(fields.Count).Select(value => value!).ToArray()).ToArray();
        return candidates.Length == 1 ? candidates[0] : null;
    }

    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var normalized = value.Normalize(NormalizationForm.FormKC).ToLower(CultureInfo.InvariantCulture);
        var builder = new StringBuilder(normalized.Length);
        foreach (var character in normalized)
        {
            if (char.IsWhiteSpace(character) || character is '=' or '-' or '_') continue;
            builder.Append(character is '×' or 'ｘ' ? 'x' : character);
        }

        return builder.ToString();
    }

    private IReadOnlyList<LegoSpecificationOption> GetOptions(string templateId, int level, IReadOnlyList<string> previous)
    {
        if (!_templates.TryGetValue(templateId, out var template)) return [];
        var fields = GetFields(template);
        if (level >= fields.Count) return [];
        var field = fields[level];
        return Filter(templateId, previous)
            .Where(record => record.Properties.ContainsKey(field.InternalName))
            .Select(record => new LegoSpecificationOption(RawProperty(record, field), DisplayProperty(record, field)))
            .DistinctBy(option => option.Value, StringComparer.OrdinalIgnoreCase)
            .OrderBy(option => SortValue(option.Value), StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    private IEnumerable<LegoRecord> Filter(string templateId, IReadOnlyList<string> selections)
    {
        var template = _templates[templateId];
        var fields = GetFields(template);
        return _records.Where(record => string.Equals(record.TemplateId, templateId, StringComparison.OrdinalIgnoreCase))
            .Where(record => selections.Select((selection, index) => (selection, index)).All(item => item.index < fields.Count && record.Properties.TryGetValue(fields[item.index].InternalName, out var value) && string.Equals(RawValue(value), item.selection, StringComparison.OrdinalIgnoreCase)));
    }

    private static IReadOnlyList<TemplateField> GetFields(LegoTemplate template) => template.Fields.Where(field => field.Searchable).OrderBy(field => field.SortOrder).Take(4).ToArray();

    private static IEnumerable<string> SearchValues(LegoRecord record, LegoTemplate template)
    {
        yield return record.LegoId;
        yield return record.Name;
        yield return record.Description ?? string.Empty;
        yield return record.Category;
        yield return template.Name;
        foreach (var tag in record.Tags) yield return tag;
        foreach (var field in template.Fields.Where(field => field.Searchable))
        {
            if (!record.Properties.TryGetValue(field.InternalName, out var value)) continue;
            var displayed = DisplayValue(value, field);
            yield return displayed;
            yield return field.DisplayName;
            yield return $"{field.DisplayName}={displayed}";
        }
    }

    private static string RawProperty(LegoRecord record, TemplateField field) => record.Properties.TryGetValue(field.InternalName, out var value) ? RawValue(value) : string.Empty;
    private static string DisplayProperty(LegoRecord record, TemplateField field) => record.Properties.TryGetValue(field.InternalName, out var value) ? DisplayValue(value, field) : string.Empty;
    private static string RawValue(JsonElement value) => value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : value.GetRawText();
    private static string DisplayValue(JsonElement value, TemplateField field)
    {
        var raw = RawValue(value);
        if (field.DataType == TemplateDataType.Select) raw = field.Options.FirstOrDefault(option => string.Equals(option.Value, raw, StringComparison.OrdinalIgnoreCase))?.DisplayName ?? raw;
        if (field.DataType == TemplateDataType.Boolean) raw = value.GetBoolean() ? "예" : "아니오";
        return string.IsNullOrWhiteSpace(field.Unit) ? raw : $"{raw} {field.Unit}";
    }

    private static string SortValue(string value) => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var number) ? number.ToString("000000000000.########", CultureInfo.InvariantCulture) : value;
}
