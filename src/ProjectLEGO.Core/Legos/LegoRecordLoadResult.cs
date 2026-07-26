namespace ProjectLEGO.Core.Legos;

public sealed record LegoRecordLoadError(string FilePath, string Code, string Message);

public sealed class LegoRecordLoadResult
{
    public LegoRecordLoadResult(IReadOnlyList<LegoRecord> records, IReadOnlyList<LegoRecordLoadError> errors)
    {
        Records = records;
        Errors = errors;
    }

    public IReadOnlyList<LegoRecord> Records { get; }
    public IReadOnlyList<LegoRecordLoadError> Errors { get; }
    public bool IsSuccessful => Errors.Count == 0;
}
