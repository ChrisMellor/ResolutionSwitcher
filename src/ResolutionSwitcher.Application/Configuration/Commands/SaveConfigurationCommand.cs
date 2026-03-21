using ResolutionSwitcher.Application.Abstractions.Configuration;
using ResolutionSwitcher.Domain.Configuration;

namespace ResolutionSwitcher.Application.Configuration.Commands;

/// <summary>
///     Represents a command that saves application configuration using the specified configuration store.
/// </summary>
public sealed class SaveConfigurationCommand : ISaveConfigurationCommand
{
    private readonly IAppConfigurationStore _configurationStore;

    /// <summary>
    ///     Initializes a new instance of the SaveConfigurationCommand class using the specified configuration store.
    /// </summary>
    /// <param name="configurationStore">
    ///     The configuration store used to persist application configuration settings. Cannot be
    ///     null.
    /// </param>
    public SaveConfigurationCommand(IAppConfigurationStore configurationStore)
    {
        _configurationStore = configurationStore;
    }

    /// <summary>
    ///     Saves the specified application configuration to the configuration store.
    /// </summary>
    /// <param name="configuration">The application configuration to be saved. Cannot be null.</param>
    public void Execute(AppConfiguration configuration)
    {
        _configurationStore.Save(configuration);
    }
}