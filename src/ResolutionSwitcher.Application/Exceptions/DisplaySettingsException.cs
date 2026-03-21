namespace ResolutionSwitcher.Application.Exceptions;

/// <summary>
///     Represents errors that occur when configuring or applying display settings.
/// </summary>
public sealed class DisplaySettingsException : Exception
{
    /// <summary>
    ///     Represents errors that occur when configuring or applying display settings.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public DisplaySettingsException(string message) : base(message) { }
}