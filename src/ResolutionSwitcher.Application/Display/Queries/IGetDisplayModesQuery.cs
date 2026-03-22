using ResolutionSwitcher.Domain.Display.Models;

namespace ResolutionSwitcher.Application.Display.Queries;

/// <summary>
///     Defines a query for retrieving the available display mode options for a specified display name.
/// </summary>
/// <remarks>
///     Implementations of this interface should return all supported display modes relevant to the provided
///     display name. If the display name is null, the implementation may return a default set of display modes or all
///     available modes, depending on the application's requirements.
/// </remarks>
public interface IGetDisplayModesQuery
{
    /// <summary>
    ///     Retrieves a read-only list of display mode options that match the specified display name.
    /// </summary>
    /// <param name="displayName">The display name to filter display mode options. If null, all available options are returned.</param>
    /// <returns>
    ///     A read-only list of <see cref="DisplayModeOption" /> objects that match the specified display name. Returns an
    ///     empty list if no matching options are found.
    /// </returns>
    IReadOnlyList<DisplayModeOption> Execute(string? displayName);
}
