using System.Text.Json;

namespace ProjectLEGO.Core.Legos.Insert;

public sealed record LegoInsertRequest(string LegoId, string TemplateId, string AssetPath, IReadOnlyDictionary<string, JsonElement> Properties);

public static class LegoInsertRequestFactory
{
    public static LegoInsertRequest? Create(LegoRecord? record) => record is null
        ? null
        : new(record.LegoId, record.TemplateId, record.AssetPath, record.Properties);
}
