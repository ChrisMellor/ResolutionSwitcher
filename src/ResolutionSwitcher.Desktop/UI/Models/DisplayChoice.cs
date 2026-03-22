using ResolutionSwitcher.Domain.Display.Models;

namespace ResolutionSwitcher.Desktop.UI.Models;

/// <summary>
///     Represents a selectable item with a display label, an optional value, and associated display bounds.
/// </summary>
/// <remarks>
///     Use this type to describe a choice that can be presented in a user interface, such as a dropdown or
///     selection list. Each instance includes a label for display, an optional value for identification or storage, and
///     bounds information for layout or rendering purposes.
/// </remarks>
internal sealed record DisplayChoice
{
    /// <summary>
    ///     Initializes a new instance of the DisplayChoice class with the specified value, label, and display bounds.
    /// </summary>
    /// <param name="Value">
    ///     The value associated with the display choice. Can be null if the choice does not have an associated
    ///     value.
    /// </param>
    /// <param name="Label">The text label to display for this choice. Cannot be null.</param>
    /// <param name="Bounds">The display bounds that define the position and size of the choice on the screen.</param>
    public DisplayChoice(string? Value,
        string Label,
        DisplayBounds Bounds)
    {
        this.Value = Value;
        this.Label = Label;
        this.Bounds = Bounds;
    }

    /// <summary>
    ///     Gets the value represented by this instance.
    /// </summary>
    public string? Value { get; init; }

    /// <summary>
    ///     Gets the display label associated with this instance.
    /// </summary>
    public string Label { get; init; }

    /// <summary>
    ///     Gets the bounding rectangle that defines the size and position of the display area.
    /// </summary>
    public DisplayBounds Bounds { get; init; }

    /// <summary>
    ///     Deconstructs the current instance into its value, label, and display bounds components.
    /// </summary>
    /// <remarks>
    ///     This method enables deconstruction syntax, allowing the instance to be unpacked into separate
    ///     variables using tuple deconstruction.
    /// </remarks>
    /// <param name="value">When this method returns, contains the value component of the instance, or null if no value is set.</param>
    /// <param name="label">When this method returns, contains the label associated with the instance.</param>
    /// <param name="bounds">When this method returns, contains the display bounds of the instance.</param>
    public void Deconstruct(out string? value, out string label, out DisplayBounds bounds)
    {
        value = Value;
        label = Label;
        bounds = Bounds;
    }

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the value of the Label property for this instance.</returns>
    public override string ToString()
    {
        return Label;
    }
}