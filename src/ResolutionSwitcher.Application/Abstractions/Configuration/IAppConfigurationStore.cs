using ResolutionSwitcher.Domain.Configuration;

namespace ResolutionSwitcher.Application.Abstractions.Configuration;

/// <summary>
///     Represents a store for loading and saving application configuration data.
/// </summary>
/// <remarks>
///     Implementations of this interface provide mechanisms to persist and retrieve application
///     configuration settings. The store may support different storage backends, such as files or databases. Thread safety
///     and persistence guarantees depend on the specific implementation.
/// </remarks>
public interface IAppConfigurationStore
{
    /// <summary>
    ///     Loads the application configuration from persistent storage or creates a new configuration if none exists.
    /// </summary>
    /// <remarks>
    ///     If no existing configuration is found, a new default configuration is created and returned.
    ///     Subsequent calls may return the same configuration instance depending on the implementation.
    /// </remarks>
    /// <returns>An instance of <see cref="AppConfiguration" /> representing the loaded or newly created configuration.</returns>
    AppConfiguration LoadOrCreate();

    /// <summary>
    ///     Saves the specified application configuration settings.
    /// </summary>
    /// <param name="configuration">The configuration settings to be saved. Cannot be null.</param>
    void Save(AppConfiguration configuration);
}