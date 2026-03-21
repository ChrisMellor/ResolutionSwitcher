using ResolutionSwitcher.Domain.Configuration;
using ResolutionSwitcher.Domain.Display;

namespace ResolutionSwitcher.Desktop.UI;

/// <summary>
///     Represents an item in a list of game profiles, including display and resolution information for presentation in a
///     user interface.
/// </summary>
/// <remarks>
///     This type encapsulates a game profile along with display-related metadata, such as a user-friendly
///     display label and resolution details. It is intended for use in scenarios where game profiles are presented in a
///     selectable list, such as in a launcher or configuration tool. Instances are typically created using the static
///     FromProfile method, which resolves display labels based on available display devices.
/// </remarks>
internal sealed class GameProfileListItem
{
    /// <summary>
    ///     Initializes a new instance of the GameProfileListItem class with the specified profile, display label, and
    ///     priority.
    /// </summary>
    /// <param name="profile">The game profile associated with this list item. Cannot be null.</param>
    /// <param name="displayLabel">The display label to use for this item in the user interface. Cannot be null or empty.</param>
    /// <param name="priority">
    ///     The priority value that determines the ordering of this item in the list. Higher values indicate higher
    ///     priority.
    /// </param>
    private GameProfileListItem(GameProfile profile, string displayLabel, int priority)
    {
        Profile = profile;
        DisplayLabel = displayLabel;
        Priority = priority;
    }

    /// <summary>
    ///     Gets the game profile associated with the current instance.
    /// </summary>
    public GameProfile Profile { get; }

    /// <summary>
    ///     Gets the priority level associated with the current instance.
    /// </summary>
    public int Priority { get; }

    /// <summary>
    ///     Gets the display name of the profile, or a default value if the profile name is not set.
    /// </summary>
    /// <remarks>
    ///     If the underlying profile name is null, empty, or consists only of white-space characters,
    ///     this property returns the string "(Unnamed profile)". Otherwise, it returns the profile's name as
    ///     specified.
    /// </remarks>
    public string Name => string.IsNullOrWhiteSpace(Profile.Name)
        ? "(Unnamed profile)"
        : Profile.Name;

    /// <summary>
    ///     Gets the name of the process associated with the current profile.
    /// </summary>
    public string ProcessName => Profile.ProcessName;

    /// <summary>
    ///     Gets the display label associated with the current instance.
    /// </summary>
    public string DisplayLabel { get; }

    /// <summary>
    ///     Gets a display resolution label that includes the width, height, and, if available, the refresh rate.
    /// </summary>
    /// <remarks>
    ///     The label is formatted as "{width} x {height} @ {refreshRate}Hz" when a refresh rate is
    ///     specified, or as "{width} x {height}" otherwise. This property is useful for presenting display settings in a
    ///     user-friendly format.
    /// </remarks>
    public string ResolutionLabel => Profile.RefreshRate.HasValue
        ? $"{Profile.Width} x {Profile.Height} @ {Profile.RefreshRate.Value}Hz"
        : $"{Profile.Width} x {Profile.Height}";

    /// <summary>
    ///     Creates a new GameProfileListItem from the specified game profile, display device list, and priority.
    /// </summary>
    /// <remarks>
    ///     If the profile specifies a display name, the method attempts to match it to a device in the
    ///     displays list to determine the display label. If no match is found, the display name itself is used. If the
    ///     profile does not specify a display name, the label of the primary display is used if available; otherwise, a
    ///     default label is assigned.
    /// </remarks>
    /// <param name="profile">The game profile to use as the source for the list item. Cannot be null.</param>
    /// <param name="displays">A read-only list of available display devices used to resolve the display label. Cannot be null.</param>
    /// <param name="priority">The priority value to assign to the list item. Higher values indicate higher priority.</param>
    /// <returns>
    ///     A GameProfileListItem representing the specified game profile, with a display label determined from the provided
    ///     display devices and the given priority.
    /// </returns>
    public static GameProfileListItem FromProfile(GameProfile profile, IReadOnlyList<DisplayDeviceInfo> displays, int priority)
    {
        var displayLabel = "Primary display";

        if (!string.IsNullOrWhiteSpace(profile.DisplayName))
        {
            displayLabel = displays.FirstOrDefault(display => string.Equals(display.DeviceName, profile.DisplayName, StringComparison.OrdinalIgnoreCase))
                                  ?.Label
                        ?? profile.DisplayName;
        }
        else
        {
            displayLabel = displays.FirstOrDefault(display => display.IsPrimary)
                                  ?.Label ?? displayLabel;
        }

        return new GameProfileListItem(profile, displayLabel, priority);
    }
}