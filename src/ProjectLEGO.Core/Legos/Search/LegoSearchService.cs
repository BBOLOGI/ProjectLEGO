using System.Globalization;
using System.Text.Json;
using ProjectLEGO.Core.Templates;

namespace ProjectLEGO.Core.Legos.Search;

public sealed class LegoSearchService
{
    private readonly IReadOnlyList<LegoRecord> _records;
    private readonly IReadOnlyDictionary<string, LegoTemplate> _templates;

    public LegoSearchService(IEnumerable<LegoRecord> records, IEnumerable<LegoTemplate> templates)
    {
        _records = records.ToArray();
        _templates = templates.ToDictionary(template => template.TemplateId, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<LegoSearchResult> Search(LegoSearchRequest request)
    {
        var keyword = request.Keyword?.Trim() ?? string.Empty;
        var results = _records
            .Where(record => record.IsActive)
            .Where(record => _templates.ContainsKey(record.TemplateId))
            .Where(record => string.IsNullOrWhiteSpace(request.TemplateId) || string.Equals(record.TemplateId, request.TemplateId, StringComparison.OrdinalIgnoreCase))
            .Where(record => string.IsNullOrWhiteSpace(request.Category) || string.Equals(record.Category, request.Category, StringComparison.OrdinalIgnoreCase))
            .Select(record => new LegoSearchResult(record, _templates[record.TemplateId], BuildSummary(record, _templates[record.TemplateId]), Score(record, _templates[record.TemplateId], keyword)))
            .Where(result => string.IsNullOrEmpty(keyword) || result.RelevanceScore > 0)
            .Where(result => request.Filters.All(filter => MatchesFilter(result.Record, filter)))
            .ToArray();

        return request.Sort switch
        {
            LegoSortOption.NameAscending => results.OrderBy(result => result.Record.Name, StringComparer.CurrentCultureIgnoreCase).ThenBy(result => result.Record.LegoId, StringComparer.OrdinalIgnoreCase).ToArray(),
            LegoSortOption.NameDescending => results.OrderByDescending(result => result.Record.Name, StringComparer.CurrentCultureIgnoreCase).ThenBy(result => result.Record.LegoId, StringComparer.OrdinalIgnoreCase).ToArray(),
            LegoSortOption.RecentlyUpdated => results.OrderByDescending(result => result.Record.UpdatedAt).ThenBy(result => result.Record.LegoId, StringComparer.OrdinalIgnoreCase).ToArray(),
            _ => results.OrderByDescending(result => result.RelevanceScore).ThenBy(result => result.Record.Name, StringComparer.CurrentCultureIgnoreCase).ThenBy(result => result.Record.LegoId, StringComparer.OrdinalIgnoreCase).ToArray()
        };
    }

    private static int Score(LegoRecord record, LegoTemplate template, string keyword)
    {
        if (string.IsNullOrEmpty(keyword)) return 0;
        if (EqualsText(record.LegoId, keyword)) return 1000;
        if (EqualsText(record.Name, keyword)) return 900;
        if (record.Name.StartsWith(keyword, StringComparison.CurrentCultureIgnoreCase)) return 800;
        if (Contains(record.Name, keyword)) return 700;
        if (record.Tags.Any(tag => EqualsText(tag, keyword))) return 600;
        if (record.Tags.Any(tag => Contains(tag, keyword))) return 550;

        var secondary = new List<string?> { record.Description, record.Category, template.Name };
        secondary.AddRange(record.Properties.Values.Select(DisplayValue));
        secondary.AddRange(template.Fields.Select(field => field.DisplayName));
        return secondary.Any(value => Contains(value, keyword)) ? 400 : 0;
    }

    private static bool MatchesFilter(LegoRecord record, LegoSearchFilter filter)
    {
        if (!record.Properties.TryGetValue(filter.InternalName, out var value)) return false;
        return filter.Operator switch
        {
            LegoFilterOperator.TextEquals => value.ValueKind == JsonValueKind.String && EqualsText(value.GetString(), filter.TextValue),
            LegoFilterOperator.TextContains => value.ValueKind == JsonValueKind.String && Contains(value.GetString(), filter.TextValue),
            LegoFilterOperator.NumberEquals => TryDecimal(value, out var number) && filter.Minimum.HasValue && number == filter.Minimum.Value,
            LegoFilterOperator.NumberRange => TryDecimal(value, out var ranged) && (!filter.Minimum.HasValue || ranged >= filter.Minimum.Value) && (!filter.Maximum.HasValue || ranged <= filter.Maximum.Value),
            LegoFilterOperator.BooleanEquals => !filter.BooleanValue.HasValue || value.ValueKind is JsonValueKind.True or JsonValueKind.False && value.GetBoolean() == filter.BooleanValue.Value,
            LegoFilterOperator.SelectAny => filter.SelectedValues.Count == 0 || value.ValueKind == JsonValueKind.String && filter.SelectedValues.Contains(value.GetString() ?? string.Empty, StringComparer.OrdinalIgnoreCase),
            _ => false
        };
    }

    private static string BuildSummary(LegoRecord record, LegoTemplate template)
    {
        return string.Join(" · ", template.Fields.Where(field => field.Searchable).OrderBy(field => field.SortOrder).Select(field =>
        {
            if (!record.Properties.TryGetValue(field.InternalName, out var value)) return null;
            var displayed = DisplayValue(value);
            if (field.DataType == TemplateDataType.Select)
            {
                displayed = field.Options.FirstOrDefault(option => string.Equals(option.Value, displayed, StringComparison.OrdinalIgnoreCase))?.DisplayName ?? displayed;
            }

            return $"{field.DisplayName}: {displayed}{(string.IsNullOrWhiteSpace(field.Unit) ? string.Empty : " " + field.Unit)}";
        }).Where(text => text is not null));
    }

    private static string DisplayValue(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString() ?? string.Empty,
        JsonValueKind.Number => value.GetRawText(),
        JsonValueKind.True => "예",
        JsonValueKind.False => "아니오",
        _ => value.GetRawText()
    };

    private static bool TryDecimal(JsonElement value, out decimal number)
    {
        number = default;
        return value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out number);
    }
    private static bool EqualsText(string? left, string? right) => string.Equals(left, right, StringComparison.CurrentCultureIgnoreCase);
    private static bool Contains(string? value, string? keyword) => !string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(keyword) && value.Contains(keyword, StringComparison.CurrentCultureIgnoreCase);
}
