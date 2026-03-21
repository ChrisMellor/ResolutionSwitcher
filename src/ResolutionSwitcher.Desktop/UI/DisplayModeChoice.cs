using ResolutionSwitcher.Domain.Display;

namespace ResolutionSwitcher.Desktop.UI;

/// <summary>
///     Represents a selectable display mode option with an associated label for user interfaces.
/// </summary>
/// <remarks>
///     This type encapsulates a display mode choice, typically used to present available display options to
///     users in UI components such as dropdowns or menus. The label provides a user-friendly description of the
///     mode.
/// </remarks>
internal sealed record DisplayModeChoice
{
    /// <summary>
    ///     Initializes a new instance of the DisplayModeChoice class with the specified display mode option.
    /// </summary>
    /// <param name="Mode">The display mode option to assign to this instance.</param>
    public DisplayModeChoice(DisplayModeOption Mode)
    {
        this.Mode = Mode;
    }

    /// <summary>
    ///     Gets the display label associated with the current mode.
    /// </summary>
    public string Label => Mode.Label;

    /// <summary>
    ///     Gets the display mode option to use for rendering content.
    /// </summary>
    public DisplayModeOption Mode { get; init; }

    /// <summary>
    ///     Deconstructs the current instance into its display mode option.
    /// </summary>
    /// <param name="mode">When this method returns, contains the display mode option of the current instance.</param>
    public void Deconstruct(out DisplayModeOption mode)
    {
        mode = Mode;
    }

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string containing the value of the Label property.</returns>
    public override string ToString()
    {
        return Label;
    }
}