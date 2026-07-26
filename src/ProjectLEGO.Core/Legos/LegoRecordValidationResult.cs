namespace ProjectLEGO.Core.Legos;

public sealed record LegoRecordValidationError(string Path, string Message);

public sealed class LegoRecordValidationResult
{
    public LegoRecordValidationResult(IReadOnlyList<LegoRecordValidationError> errors) => Errors = errors;
    public IReadOnlyList<LegoRecordValidationError> Errors { get; }
    public bool IsValid => Errors.Count == 0;
}
