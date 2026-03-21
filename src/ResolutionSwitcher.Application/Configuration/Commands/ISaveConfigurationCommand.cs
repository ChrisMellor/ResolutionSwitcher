using ResolutionSwitcher.Domain.Configuration;

namespace ResolutionSwitcher.Application.Configuration.Commands;

/// <summary>
///     Defines a command that saves the specified application configuration.
/// </summary>
/// <remarks>
///     Implementations of this interface are responsible for persisting the provided configuration. The
///     specific storage mechanism and location are determined by the implementation.
/// </remarks>
public interface ISaveConfigurationCommand
{
    /// <summary>
    ///     Executes the operation using the specified application configuration.
    /// </summary>
    /// <param name="configuration">The configuration settings to use for execution. Cannot be null.</param>
    void Execute(AppConfiguration configuration);
}