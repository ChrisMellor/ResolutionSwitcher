using ResolutionSwitcher.Domain.Display.Support;

namespace ResolutionSwitcher.Domain.Display.Models;

/// <summary>
///     Represents the display resolution and related settings for a display device, including width, height, refresh rate,
///     color depth, and an optional display name.
/// </summary>
/// <remarks>
///     Use this record to encapsulate all relevant parameters for a display mode when querying or
///     configuring display devices. All properties are immutable after initialization. This type is typically used in
///     scenarios involving display enumeration, configuration, or comparison.
/// </remarks>
public sealed record DisplayResolution
{
    /// <summary>
    ///     Initializes a new instance of the DisplayResolution class with the specified display name, width, height,
    ///     refresh rate, and color depth.
    /// </summary>
    /// <param name="DisplayName">The name of the display or monitor. Can be null if the display does not have a specific name.</param>
    /// <param name="Width">The width of the display resolution, in pixels. Must be a positive integer.</param>
    /// <param name="Height">The height of the display resolution, in pixels. Must be a positive integer.</param>
    /// <param name="RefreshRate">The refresh rate of the display, in hertz (Hz). Must be a positive integer.</param>
    /// <param name="BitsPerPixel">The number of bits per pixel, representing the color depth. Must be a positive integer.</param>
    public DisplayResolution(string? DisplayName,
        int Width,
        int Height,
        int RefreshRate,
        int BitsPerPixel)
    {
        this.DisplayName = DisplayName;
        this.Width = Width;
        this.Height = Height;
        this.RefreshRate = RefreshRate;
        this.BitsPerPixel = BitsPerPixel;
    }

    /// <summary>
    ///     Gets the unique device key associated with the current display name.
    /// </summary>
    public string DeviceKey => DisplayNames.ToDeviceKey(DisplayName);

    /// <summary>
    ///     Gets the display name associated with the object.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    ///     Gets the width dimension.
    /// </summary>
    public int Width { get; init; }

    /// <summary>
    ///     Gets the height value.
    /// </summary>
    public int Height { get; init; }

    /// <summary>
    ///     Gets the refresh rate, in hertz, for the associated display or operation.
    /// </summary>
    public int RefreshRate { get; init; }

    /// <summary>
    ///     Gets the number of bits used to represent each pixel in the image format.
    /// </summary>
    public int BitsPerPixel { get; init; }

    /// <summary>
    ///     Deconstructs the display mode into its component properties.
    /// </summary>
    /// <remarks>
    ///     Use this method to enable deconstruction syntax for the display mode, allowing assignment of
    ///     its properties to individual variables in a single statement.
    /// </remarks>
    /// <param name="displayName">
    ///     When this method returns, contains the display name associated with the display mode, or null
    ///     if not set.
    /// </param>
    /// <param name="width">When this method returns, contains the width of the display mode, in pixels.</param>
    /// <param name="height">When this method returns, contains the height of the display mode, in pixels.</param>
    /// <param name="refreshRate">When this method returns, contains the refresh rate of the display mode, in hertz.</param>
    /// <param name="bitsPerPixel">When this method returns, contains the number of bits per pixel for the display mode.</param>
    public void Deconstruct(out string? displayName, out int width, out int height, out int refreshRate, out int bitsPerPixel)
    {
        displayName = DisplayName;
        width = Width;
        height = Height;
        refreshRate = RefreshRate;
        bitsPerPixel = BitsPerPixel;
    }
}
