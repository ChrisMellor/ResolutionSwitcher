using System.Runtime.InteropServices;

namespace ResolutionSwitcher.Infrastructure.Display;

/// <summary>
///     Represents a point in a two-dimensional coordinate system using integer values for the x and y coordinates.
/// </summary>
/// <remarks>
///     This structure is typically used for interoperability with native Windows APIs that require a
///     point defined by integer coordinates. The coordinates specify the horizontal (x) and vertical (y) position,
///     respectively.
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
internal struct Win32Point
{
    public int x;
    public int y;
}
