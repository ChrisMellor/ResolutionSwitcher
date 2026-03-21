namespace ResolutionSwitcher.Domain.Display;

/// <summary>
///     Represents a display mode option, including resolution and optional refresh rate information.
/// </summary>
/// <remarks>
///     Use this type to describe available display configurations, such as those supported by a monitor or
///     graphics device. The record is immutable and provides value-based equality.
/// </remarks>
public sealed record DisplayModeOption
{
    /// <summary>
    ///     Initializes a new instance of the DisplayModeOption class with the specified width, height, and optional refresh
    ///     rate.
    /// </summary>
    /// <param name="Width">The width of the display mode, in pixels. Must be a positive integer.</param>
    /// <param name="Height">The height of the display mode, in pixels. Must be a positive integer.</param>
    /// <param name="RefreshRate">The refresh rate of the display mode, in hertz. Specify null to use the default refresh rate.</param>
    public DisplayModeOption(int Width,
        int Height,
        int? RefreshRate)
    {
        this.Width = Width;
        this.Height = Height;
        this.RefreshRate = RefreshRate;
    }

    /// <summary>
    ///     Gets a display label that includes the resolution and, if available, the refresh rate.
    /// </summary>
    /// <remarks>
    ///     The label is formatted as "Width x Height @ RefreshRateHz" when a refresh rate is specified,
    ///     or as "Width x Height" otherwise. This property is useful for presenting display mode information in user
    ///     interfaces.
    /// </remarks>
    public string Label => RefreshRate.HasValue
        ? $"{Width} x {Height} @ {RefreshRate.Value}Hz"
        : $"{Width} x {Height}";

    /// <summary>
    ///     Gets the width value.
    /// </summary>
    public int Width { get; init; }

    /// <summary>
    ///     Gets the height value.
    /// </summary>
    public int Height { get; init; }

    /// <summary>
    ///     Gets the refresh rate, in hertz, for the associated operation or device.
    /// </summary>
    public int? RefreshRate { get; init; }

    /// <summary>
    ///     Deconstructs the current instance into its width, height, and optional refresh rate components.
    /// </summary>
    /// <remarks>
    ///     This method enables deconstruction syntax, allowing the instance to be unpacked into separate
    ///     variables for width, height, and refresh rate.
    /// </remarks>
    /// <param name="width">When this method returns, contains the width value of the current instance.</param>
    /// <param name="height">When this method returns, contains the height value of the current instance.</param>
    /// <param name="refreshRate">
    ///     When this method returns, contains the refresh rate value of the current instance, or null if no refresh rate is
    ///     specified.
    /// </param>
    public void Deconstruct(out int width, out int height, out int? refreshRate)
    {
        width = Width;
        height = Height;
        refreshRate = RefreshRate;
    }
}