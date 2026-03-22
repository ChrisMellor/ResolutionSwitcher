using ResolutionSwitcher.Application.Watcher.Status;

namespace ResolutionSwitcher.Application.Abstractions.Watcher;

/// <summary>
///     Represents a feed that provides the current status of a watcher and notifies subscribers when the status changes.
/// </summary>
/// <remarks>
///     Implementations of this interface allow consumers to monitor the real-time status of a watcher
///     component. Subscribers can listen to the <see cref="StatusChanged" /> event to receive updates when the status
///     changes.
/// </remarks>
public interface IWatcherStatusFeed
{
    /// <summary>
    ///     Gets the most recent snapshot of the watcher's status.
    /// </summary>
    WatcherStatusSnapshot Current { get; }

    /// <summary>
    ///     Occurs when the status of the watcher changes.
    /// </summary>
    /// <remarks>
    ///     Subscribers are notified with a snapshot of the current watcher status whenever a status
    ///     change occurs. Event handlers can use the provided snapshot to determine the new state and respond
    ///     accordingly.
    /// </remarks>
    event EventHandler<WatcherStatusSnapshot>? StatusChanged;
}
