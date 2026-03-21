using ResolutionSwitcher.Application.Abstractions.Configuration;
using ResolutionSwitcher.Domain.Configuration;

namespace ResolutionSwitcher.Application.Configuration.Queries;

/// <summary>
///     Represents a query that loads the application configuration from the specified configuration store.
/// </summary>
public sealed class LoadConfigurationQuery : ILoadConfigurationQuery
{
    private readonly IAppConfigurationStore _configurationStore;

    /// <summary>
    ///     Represents a query that loads the application configuration from the specified configuration store.
    /// </summary>
    /// <param name="configurationStore">
    ///     The configuration store used to load or create the application configuration. Cannot
    ///     be null.
    /// </param>
    public LoadConfigurationQuery(IAppConfigurationStore configurationStore)
    {
        _configurationStore = configurationStore;
    }

    /// <summary>
    ///     Retrieves the application configuration, creating a new configuration if none exists.
    /// </summary>
    /// <returns>
    ///     An instance of <see cref="AppConfiguration" /> representing the current application configuration. A new
    ///     configuration is created and returned if one does not already exist.
    /// </returns>
    public AppConfiguration Execute()
    {
        return _configurationStore.LoadOrCreate();
    }
}