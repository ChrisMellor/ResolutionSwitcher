using ResolutionSwitcher.Domain.Configuration;

namespace ResolutionSwitcher.Application.Configuration.Queries;

/// <summary>
///     Defines a query for retrieving the application configuration.
/// </summary>
/// <remarks>
///     Implementations of this interface provide a mechanism to load configuration settings for the
///     application. The specific source or format of the configuration may vary depending on the implementation.
/// </remarks>
public interface ILoadConfigurationQuery
{
    /// <summary>
    ///     Executes the configuration process and returns the resulting application configuration.
    /// </summary>
    /// <returns>An <see cref="AppConfiguration" /> instance containing the configured application settings.</returns>
    AppConfiguration Execute();
}