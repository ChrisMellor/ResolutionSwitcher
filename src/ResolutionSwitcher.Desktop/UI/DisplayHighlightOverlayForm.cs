using ResolutionSwitcher.Domain.Display;
using System.Drawing.Drawing2D;
using System.Runtime.Versioning;

namespace ResolutionSwitcher.Desktop.UI;

/// <summary>
///     Represents a transparent, non-activating overlay form that highlights a display area with a colored border.
/// </summary>
/// <remarks>
///     This form is intended for use as a visual overlay to highlight a specific display region, such as
///     during screen selection or display identification. The overlay does not appear in the taskbar, does not receive
///     input focus, and is always rendered above other windows. The form uses a transparent color key to allow only the
///     border to be visible, and is double-buffered to reduce flicker. This class is supported only on Windows
///     platforms.
/// </remarks>
[SupportedOSPlatform("windows")]
internal sealed class DisplayHighlightOverlayForm : Form
{
    private const int BorderThickness = 12;
    private static readonly Color TransparentColorKey = Color.Magenta;
    private static readonly Color BorderColor = Color.DeepSkyBlue;

    /// <summary>
    ///     Initializes a new instance of the DisplayHighlightOverlayForm class with the specified display bounds.
    /// </summary>
    /// <remarks>
    ///     The overlay form is configured to be borderless, transparent, and always on top. It does not
    ///     appear in the taskbar and is positioned manually according to the specified bounds.
    /// </remarks>
    /// <param name="bounds">The bounds that define the position and size of the overlay form on the display.</param>
    public DisplayHighlightOverlayForm(DisplayBounds bounds)
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        ShowInTaskbar = false;
        TopMost = true;
        Bounds = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        BackColor = TransparentColorKey;
        TransparencyKey = TransparentColorKey;
        DoubleBuffered = true;
    }

    /// <summary>
    ///     Gets a value indicating whether the window is displayed without activating it.
    /// </summary>
    /// <remarks>
    ///     Override this property to allow the window to be shown without taking focus from the
    ///     currently active window. This is commonly used for tool windows or notifications that should not interrupt the
    ///     user's workflow.
    /// </remarks>
    protected override bool ShowWithoutActivation => true;

    /// <summary>
    ///     Gets the parameters required to create the control's handle, including extended window styles for transparency,
    ///     tool window appearance, and non-activation behavior.
    /// </summary>
    /// <remarks>
    ///     The returned parameters include the WS_EX_TRANSPARENT, WS_EX_TOOLWINDOW, and WS_EX_NOACTIVATE
    ///     extended window styles. This causes the control to be transparent to mouse events, appear as a tool window, and
    ///     not activate when shown. These styles are useful for creating overlay or non-interactive UI elements.
    /// </remarks>
    protected override CreateParams CreateParams
    {
        get
        {
            const int wsExTransparent = 0x00000020;
            const int wsExToolWindow = 0x00000080;
            const int wsExNoActivate = 0x08000000;

            var createParams = base.CreateParams;
            createParams.ExStyle |= wsExTransparent | wsExToolWindow | wsExNoActivate;
            return createParams;
        }
    }

    /// <summary>
    ///     Paints the control's border using the specified graphics context and current border settings.
    /// </summary>
    /// <remarks>
    ///     This method customizes the control's appearance by drawing a border with anti-aliased
    ///     rendering. It is typically called by the Windows Forms framework during the painting process and should not be
    ///     called directly.
    /// </remarks>
    /// <param name="e">
    ///     A <see cref="PaintEventArgs" /> that contains the event data, including the graphics surface on which
    ///     to paint.
    /// </param>
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using var pen = new Pen(BorderColor, BorderThickness) { Alignment = PenAlignment.Inset };

        var borderBounds = ClientRectangle;
        borderBounds.Inflate(-BorderThickness / 2, -BorderThickness / 2);
        e.Graphics.DrawRectangle(pen, borderBounds);
    }
}