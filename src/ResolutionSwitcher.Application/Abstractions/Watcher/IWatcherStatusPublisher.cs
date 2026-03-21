namespace ResolutionSwitcher.Application.Abstractions.Watcher;

/// <summary>
///     Defines a contract for publishing status messages from a watcher component.
/// </summary>
/// <remarks>
///     Implementations of this interface can be used to relay informational or error messages to external
///     consumers, such as logs, user interfaces, or monitoring systems. The interface does not specify the destination or
///     formatting of messages, allowing flexibility in how status updates are handled.
/// </remarks>
public interface IWatcherStatusPublisher
{
    /// <summary>
    ///     Publishes a message to the output stream, optionally marking it as an error.
    /// </summary>
    /// <param name="message">The message text to publish. Cannot be null.</param>
    /// <param name="isError">true to indicate the message represents an error; otherwise, false. The default is false.</param>
    void Publish(string message, bool isError = false);
}