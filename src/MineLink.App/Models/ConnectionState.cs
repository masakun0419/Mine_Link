namespace MineLink_App.Models;

public enum ConnectionState
{
    Idle,
    Publishing,
    Connected,
}

public enum LogSeverity
{
    Info,
    Success,
    Warning,
    Error,
}

public sealed class LogEntry
{
    public required DateTimeOffset Timestamp { get; init; }
    public required LogSeverity Severity { get; init; }
    public required string Message { get; init; }
}

public sealed class RoomMember
{
    public required string DisplayName { get; init; }
    public required int PingMs { get; init; }
    public required DateTimeOffset ConnectedAt { get; init; }
}
