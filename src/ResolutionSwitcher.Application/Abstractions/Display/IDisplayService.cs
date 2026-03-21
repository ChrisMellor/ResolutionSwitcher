using ResolutionSwitcher.Domain.Configuration;
using ResolutionSwitcher.Domain.Display;

namespace ResolutionSwitcher.Application.Abstractions.Display;

/// <summary>
///     Provides methods for querying and managing display devices, display modes, and screen resolutions.
/// </summary>
/// <remarks>
///     Implementations of this interface enable applications to interact with connected display hardware,
///     retrieve available display configurations, and apply or compare screen resolutions. This interface is typically
///     used
///     in scenarios where display settings need to be queried, modified, or presented to users, such as in graphics
///     settings panels or system utilities.
/// </remarks>
public interface IDisplayService
{
    /// <summary>
    ///     Applies the specified display resolution to the current display device.
    /// </summary>
    /// <remarks>
    ///     Applying a new resolution may cause the display to flicker or temporarily go blank while the
    ///     change is in progress. Ensure that the specified resolution is supported by the display device to avoid
    ///     unexpected behavior.
    /// </remarks>
    /// <param name="resolution">The display resolution settings to apply. Cannot be null.</param>
    void ApplyResolution(DisplayResolution resolution);

    /// <summary>
    ///     Retrieves the current display resolution for the specified display device.
    /// </summary>
    /// <remarks>
    ///     If displayName is null, the method captures the resolution of the system's primary display.
    ///     The behavior for unrecognized or disconnected display names may vary depending on the platform
    ///     implementation.
    /// </remarks>
    /// <param name="displayName">
    ///     The name of the display device for which to capture the resolution, or null to use the primary display. If
    ///     multiple displays share the same name, the first match is used.
    /// </param>
    /// <returns>
    ///     A DisplayResolution object representing the current resolution of the specified display. If the display is not
    ///     found, the returned object may indicate an invalid or default resolution.
    /// </returns>
    DisplayResolution CaptureCurrentResolution(string? displayName);

    /// <summary>
    ///     Formats the specified display resolution as a human-readable string.
    /// </summary>
    /// <param name="resolution">The display resolution to format.</param>
    /// <returns>A string representation of the display resolution, suitable for display to users.</returns>
    string FormatResolution(DisplayResolution resolution);

    /// <summary>
    ///     Retrieves a read-only list of display devices currently available on the system.
    /// </summary>
    /// <returns>
    ///     A read-only list of <see cref="DisplayDeviceInfo" /> objects representing the available display devices. The list
    ///     is empty if no displays are detected.
    /// </returns>
    IReadOnlyList<DisplayDeviceInfo> GetDisplays();

    /// <summary>
    ///     Retrieves a read-only list of supported display mode options for the specified display device.
    /// </summary>
    /// <param name="displayName">
    ///     The name of the display device for which to retrieve supported modes. If null, the default
    ///     display is used.
    /// </param>
    /// <returns>
    ///     A read-only list of supported display mode options for the specified display. The list is empty if no modes are
    ///     available or the display is not found.
    /// </returns>
    IReadOnlyList<DisplayModeOption> GetSupportedModes(string? displayName);

    /// <summary>
    ///     Determines whether two display resolutions are equal.
    /// </summary>
    /// <param name="left">The first display resolution to compare.</param>
    /// <param name="right">The second display resolution to compare.</param>
    /// <returns>true if the specified display resolutions are equal; otherwise, false.</returns>
    bool IsSameResolution(DisplayResolution left, DisplayResolution right);

    /// <summary>
    ///     Determines the appropriate display resolution based on the specified game profile settings.
    /// </summary>
    /// <remarks>
    ///     The returned resolution reflects the preferences and constraints defined in the game profile,
    ///     such as aspect ratio or performance requirements. If the profile does not specify a valid resolution, a default
    ///     or fallback resolution may be returned.
    /// </remarks>
    /// <param name="profile">
    ///     The game profile containing user or system preferences that influence resolution selection.
    ///     Cannot be null.
    /// </param>
    /// <returns>A DisplayResolution instance representing the selected resolution for the provided profile.</returns>
    DisplayResolution ResolveRequestedResolution(GameProfile profile);
}