using ProjectLEGO.Core.Legos.Insert;

namespace ProjectLEGO.Desktop.Services;

public sealed class LegoInsertRequestedEventArgs(LegoInsertRequest request) : EventArgs
{
    public LegoInsertRequest Request { get; } = request;
}
