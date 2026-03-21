namespace ResolutionSwitcher.Application.Exceptions;

/// <summary>
///     Represents errors that occur when application configuration is invalid or missing required information.
/// </summary>
/// <remarks>
///     Use this exception to indicate problems related to configuration files, settings, or environment
///     variables that prevent the application from starting or operating correctly.
/// </remarks>
public sealed class ConfigurationException : Exception
{
    /// <summary>
    ///     Represents errors that occur when application configuration is invalid or missing required information.
    /// </summary>
    /// <remarks>
    ///     Use this exception to indicate problems related to configuration files, settings, or environment
    ///     variables that prevent the application from starting or operating correctly.
    /// </remarks>
    /// <param name="message">The error message that describes the reason for the configuration failure.</param>
    public ConfigurationException(string message) : base(message) { }
}