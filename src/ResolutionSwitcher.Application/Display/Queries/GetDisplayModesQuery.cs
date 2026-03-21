using ResolutionSwitcher.Application.Abstractions.Display;
using ResolutionSwitcher.Domain.Display;

namespace ResolutionSwitcher.Application.Display.Queries;

/// <summary>
///     Represents a query for retrieving the supported display modes for a specified display.
/// </summary>
public sealed class GetDisplayModesQuery : IGetDisplayModesQuery
{
    private readonly IDisplayService _displayService;

    /// <summary>
    ///     Represents a query for retrieving the supported display modes for a specified display.
    /// </summary>
    /// <param name="displayService">The service used to obtain supported display modes. Cannot be null.</param>
    public GetDisplayModesQuery(IDisplayService displayService)
    {
        _displayService = displayService;
    }

    /// <summary>
    ///     Retrieves the list of supported display mode options for the specified display name.
    /// </summary>
    /// <param name="displayName">
    ///     The name of the display for which to retrieve supported display modes. If null, the default
    ///     display is used.
    /// </param>
    /// <returns>
    ///     A read-only list of supported display mode options for the specified display. The list is empty if no modes are
    ///     available.
    /// </returns>
    public IReadOnlyList<DisplayModeOption> Execute(string? displayName)
    {
        return _displayService.GetSupportedModes(displayName);
    }
}