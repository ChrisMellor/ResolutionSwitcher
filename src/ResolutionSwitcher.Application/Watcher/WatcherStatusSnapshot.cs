namespace ResolutionSwitcher.Application.Watcher;

public sealed record WatcherStatusSnapshot
{
    public WatcherStatusSnapshot(string Message,
        DateTimeOffset Timestamp,
        bool IsError)
    {
        this.Message = Message;
        this.Timestamp = Timestamp;
        this.IsError = IsError;
    }

    public string Message { get; init; }

    public DateTimeOffset Timestamp { get; init; }

    public bool IsError { get; init; }

    public void Deconstruct(out string message, out DateTimeOffset timestamp, out bool isError)
    {
        message = Message;
        timestamp = Timestamp;
        isError = IsError;
    }
}