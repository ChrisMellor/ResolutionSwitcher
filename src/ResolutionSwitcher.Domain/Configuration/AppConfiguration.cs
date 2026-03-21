namespace ResolutionSwitcher.Domain.Configuration;

/// <summary>
///     Represents the application configuration settings, including watcher options, polling interval, and game profiles.
/// </summary>
/// <remarks>
///     This class encapsulates user-configurable settings for the application. It is typically used to load,
///     store, or modify runtime configuration values. The configuration includes options for enabling a watcher, setting
///     the polling interval in seconds, and managing a collection of game profiles.
/// </remarks>
public sealed class AppConfiguration
{
    /// <summary>
    ///     Gets or sets a value indicating whether the watcher is enabled.
    /// </summary>
    public bool WatcherEnabled { get; set; } = true;

    /// <summary>
    ///     Gets or sets the interval, in seconds, between polling operations.
    /// </summary>
    /// <remarks>
    ///     Set this property to control how frequently the polling process runs. Adjusting the interval
    ///     may affect responsiveness and resource usage.
    /// </remarks>
    public int PollIntervalSeconds { get; set; } = 3;

    /// <summary>
    ///     Gets or sets the collection of game profiles associated with the current instance.
    /// </summary>
    public List<GameProfile> Profiles { get; set; } = [];

    /// <summary>
    ///     Creates a new instance of the AppConfiguration class with default settings.
    /// </summary>
    /// <remarks>
    ///     The default configuration enables the watcher and sets the polling interval to 3 seconds. Use
    ///     this method to quickly obtain a baseline configuration suitable for most scenarios.
    /// </remarks>
    /// <returns>A new AppConfiguration instance initialized with default values for all properties.</returns>
    public static AppConfiguration CreateDefault()
    {
        return new AppConfiguration
        {
            WatcherEnabled = true,
            PollIntervalSeconds = 3
        };
    }
}