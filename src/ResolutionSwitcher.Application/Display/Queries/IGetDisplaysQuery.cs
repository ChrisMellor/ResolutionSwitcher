using ResolutionSwitcher.Domain.Display.Models;

namespace ResolutionSwitcher.Application.Display.Queries;

/// <summary>
///     Represents a query for retrieving information about available display devices.
/// </summary>
/// <remarks>
///     Implementations of this interface provide a mechanism to enumerate display devices connected to the
///     system. The returned list may vary depending on the current hardware configuration and system state.
/// </remarks>
public interface IGetDisplaysQuery
{
    /// <summary>
    ///     Retrieves a read-only list of display device information available on the system.
    /// </summary>
    /// <returns>
    ///     A read-only list of <see cref="DisplayDeviceInfo" /> objects representing the available display devices. The list
    ///     is empty if no display devices are found.
    /// </returns>
    IReadOnlyList<DisplayDeviceInfo> Execute();
}
