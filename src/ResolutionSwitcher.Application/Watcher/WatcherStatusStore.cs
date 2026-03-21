using ResolutionSwitcher.Application.Abstractions.Watcher;

namespace ResolutionSwitcher.Application.Watcher;

public sealed class WatcherStatusStore : IWatcherStatusFeed, IWatcherStatusPublisher
{
    public WatcherStatusSnapshot Current { get; private set; } = new("Watcher starting...", DateTimeOffset.Now, false);

    public event EventHandler<WatcherStatusSnapshot>? StatusChanged;

    public void Publish(string message, bool isError = false)
    {
        if (string.Equals(Current.Message, message, StringComparison.Ordinal) && (Current.IsError == isError))
        {
            return;
        }

        Current = new WatcherStatusSnapshot(message, DateTimeOffset.Now, isError);
        StatusChanged?.Invoke(this, Current);
    }
}