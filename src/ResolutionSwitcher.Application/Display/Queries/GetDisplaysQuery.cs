using ResolutionSwitcher.Application.Abstractions.Display;
using ResolutionSwitcher.Domain.Display;

namespace ResolutionSwitcher.Application.Display.Queries;

/// <summary>
///     Represents a query for retrieving information about available display devices.
/// </summary>
public sealed class GetDisplaysQuery : IGetDisplaysQuery
{
    private readonly IDisplayService _displayService;

    /// <summary>
    ///     Represents a query for retrieving information about available display devices.
    /// </summary>
    /// <param name="displayService">The service used to obtain display device information. Cannot be null.</param>
    public GetDisplaysQuery(IDisplayService displayService)
    {
        _displayService = displayService;
    }

    /// <summary>
    ///     Retrieves a read-only list of available display devices.
    /// </summary>
    /// <returns>
    ///     A read-only list of <see cref="DisplayDeviceInfo" /> objects representing the currently available display
    ///     devices. The list will be empty if no displays are detected.
    /// </returns>
    public IReadOnlyList<DisplayDeviceInfo> Execute()
    {
        return _displayService.GetDisplays();
    }
}