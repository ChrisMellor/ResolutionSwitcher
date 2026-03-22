namespace ResolutionSwitcher.Domain.Display.Support;

/// <summary>
///     Provides utility methods for working with display names, including formatting and describing display identifiers.
/// </summary>
/// <remarks>
///     This class is intended for scenarios where display names may be null, empty, or whitespace, and a
///     consistent representation or description is required. All members are static and thread safe.
/// </remarks>
public static class DisplayNames
{
    /// <summary>
    ///     Returns a user-friendly description for a display name, defaulting to a generic label if the input is null or
    ///     whitespace.
    /// </summary>
    /// <param name="displayName">
    ///     The display name to describe. If null, empty, or consists only of whitespace, a default
    ///     description is returned.
    /// </param>
    /// <returns>
    ///     A string containing the specified display name, or "the primary display" if the input is null, empty, or
    ///     whitespace.
    /// </returns>
    public static string Describe(string? displayName)
    {
        return string.IsNullOrWhiteSpace(displayName)
            ? "the primary display"
            : displayName;
    }

    /// <summary>
    ///     Converts a display name to a device key, returning a default key if the display name is null, empty, or consists
    ///     only of white-space characters.
    /// </summary>
    /// <param name="displayName">
    ///     The display name to convert to a device key. If null, empty, or white space, a default key is
    ///     returned.
    /// </param>
    /// <returns>
    ///     A device key string based on the specified display name, or "primary" if the display name is null, empty, or
    ///     white space.
    /// </returns>
    public static string ToDeviceKey(string? displayName)
    {
        return string.IsNullOrWhiteSpace(displayName)
            ? "<primary>"
            : displayName;
    }
}