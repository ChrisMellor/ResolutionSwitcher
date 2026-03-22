namespace ResolutionSwitcher.Domain.Display.Models;

/// <summary>
///     Represents the rectangular bounds of a display area, defined by its position and size.
/// </summary>
/// <remarks>
///     Use this type to describe the position and dimensions of a display or screen region in
///     device-independent pixels. The coordinates specify the upper-left corner of the rectangle, and the width and height
///     specify its size. This type is immutable.
/// </remarks>
public sealed record DisplayBounds
{
    /// <summary>
    ///     Initializes a new instance of the DisplayBounds class with the specified position and size.
    /// </summary>
    /// <param name="X">The horizontal position of the upper-left corner of the bounds, in pixels.</param>
    /// <param name="Y">The vertical position of the upper-left corner of the bounds, in pixels.</param>
    /// <param name="Width">The width of the bounds, in pixels. Must be a non-negative value.</param>
    /// <param name="Height">The height of the bounds, in pixels. Must be a non-negative value.</param>
    public DisplayBounds(int X,
        int Y,
        int Width,
        int Height)
    {
        this.X = X;
        this.Y = Y;
        this.Width = Width;
        this.Height = Height;
    }

    /// <summary>
    ///     Gets the value of X.
    /// </summary>
    public int X { get; init; }

    /// <summary>
    ///     Gets the Y-coordinate value.
    /// </summary>
    public int Y { get; init; }

    /// <summary>
    ///     Gets the width dimension.
    /// </summary>
    public int Width { get; init; }

    /// <summary>
    ///     Gets the height value.
    /// </summary>
    public int Height { get; init; }

    /// <summary>
    ///     Deconstructs the current instance into its X, Y, Width, and Height components.
    /// </summary>
    /// <remarks>
    ///     Use this method to enable deconstruction syntax, allowing the instance to be unpacked into
    ///     separate variables for X, Y, Width, and Height.
    /// </remarks>
    /// <param name="x">When this method returns, contains the X-coordinate value of the instance.</param>
    /// <param name="y">When this method returns, contains the Y-coordinate value of the instance.</param>
    /// <param name="width">When this method returns, contains the width value of the instance.</param>
    /// <param name="height">When this method returns, contains the height value of the instance.</param>
    public void Deconstruct(out int x, out int y, out int width, out int height)
    {
        x = X;
        y = Y;
        width = Width;
        height = Height;
    }
}