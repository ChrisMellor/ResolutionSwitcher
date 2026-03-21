namespace ResolutionSwitcher.Domain.Configuration;

/// <summary>
///     Represents a configuration profile for a game, including display settings and identification information.
/// </summary>
/// <remarks>
///     A GameProfile encapsulates the key properties needed to identify and configure a game instance, such
///     as its process name, display name, and preferred display settings. This type is intended to be used for managing
///     and
///     distinguishing between different game configurations, for example in a launcher or settings manager.
/// </remarks>
public sealed class GameProfile
{
    /// <summary>
    ///     Gets or sets the name associated with the current instance.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the name of the process associated with this instance.
    /// </summary>
    public string ProcessName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the display name associated with the object.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    ///     Gets or sets the width dimension.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    ///     Gets or sets the height value.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    ///     Gets or sets the refresh rate, in hertz, for the associated operation or device.
    /// </summary>
    public int? RefreshRate { get; set; }

    /// <summary>
    ///     Gets the display label for the profile, using the profile name if available or the process name otherwise.
    /// </summary>
    public string ProfileLabel => string.IsNullOrWhiteSpace(Name)
        ? ProcessName
        : Name;

    /// <summary>
    ///     Gets the process name with any file extension and leading or trailing whitespace removed.
    /// </summary>
    public string NormalizedProcessName => Path.GetFileNameWithoutExtension(ProcessName.Trim());

    /// <summary>
    ///     Gets a unique string key that identifies the display configuration based on process name, display name,
    ///     dimensions, refresh rate, and display name.
    /// </summary>
    /// <remarks>
    ///     The unique key is constructed by combining several display-related properties. This key can
    ///     be used for comparison, caching, or lookup scenarios where a distinct identifier for a display configuration is
    ///     required.
    /// </remarks>
    public string UniqueKey => $"{NormalizedProcessName}|{DisplayName ?? "<primary>"}|{Width}|{Height}|{RefreshRate?.ToString() ?? "*"}|{Name ?? string.Empty}";
}