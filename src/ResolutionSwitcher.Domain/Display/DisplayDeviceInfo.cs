namespace ResolutionSwitcher.Domain.Display;

/// <summary>
///     Represents information about a display device, including its name, label, primary status, and display bounds.
/// </summary>
/// <remarks>
///     Use this type to access metadata and layout information for a connected display device, such as when
///     enumerating available screens or configuring display settings. Instances of this record are immutable.
/// </remarks>
public sealed record DisplayDeviceInfo
{
    /// <summary>
    ///     Initializes a new instance of the DisplayDeviceInfo class with the specified device name, label, primary status,
    ///     and display bounds.
    /// </summary>
    /// <param name="DeviceName">The unique name of the display device, or null if not specified.</param>
    /// <param name="Label">The user-friendly label for the display device.</param>
    /// <param name="IsPrimary">true if the display device is the primary display; otherwise, false.</param>
    /// <param name="Bounds">The bounds of the display device, specifying its position and size.</param>
    public DisplayDeviceInfo(string? DeviceName,
        string Label,
        bool IsPrimary,
        DisplayBounds Bounds)
    {
        this.DeviceName = DeviceName;
        this.Label = Label;
        this.IsPrimary = IsPrimary;
        this.Bounds = Bounds;
    }

    /// <summary>
    ///     Gets the name of the device associated with this instance.
    /// </summary>
    public string? DeviceName { get; init; }

    /// <summary>
    ///     Gets the display label associated with this instance.
    /// </summary>
    public string Label { get; init; }

    /// <summary>
    ///     Gets a value indicating whether this instance is designated as the primary entity.
    /// </summary>
    public bool IsPrimary { get; init; }

    /// <summary>
    ///     Gets the bounding rectangle that defines the display area.
    /// </summary>
    public DisplayBounds Bounds { get; init; }

    /// <summary>
    ///     Deconstructs the display information into its component properties.
    /// </summary>
    /// <remarks>
    ///     Use this method to deconstruct the display object into individual variables for easier access
    ///     to its properties, typically in a deconstruction assignment.
    /// </remarks>
    /// <param name="deviceName">
    ///     When this method returns, contains the device name associated with the display, or null if not
    ///     set.
    /// </param>
    /// <param name="label">When this method returns, contains the user-friendly label for the display.</param>
    /// <param name="isPrimary">
    ///     When this method returns, contains a value indicating whether the display is the primary
    ///     display.
    /// </param>
    /// <param name="bounds">When this method returns, contains the bounds of the display as a DisplayBounds structure.</param>
    public void Deconstruct(out string? deviceName, out string label, out bool isPrimary, out DisplayBounds bounds)
    {
        deviceName = DeviceName;
        label = Label;
        isPrimary = IsPrimary;
        bounds = Bounds;
    }
}