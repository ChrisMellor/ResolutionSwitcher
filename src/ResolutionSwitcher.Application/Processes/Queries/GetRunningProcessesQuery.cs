using ResolutionSwitcher.Application.Abstractions.Processes;

namespace ResolutionSwitcher.Application.Processes.Queries;

/// <summary>
///     Represents a query for retrieving the names of all currently running processes.
/// </summary>
public sealed class GetRunningProcessesQuery : IGetRunningProcessesQuery
{
    private readonly IProcessMonitor _processMonitor;

    /// <summary>
    ///     Represents a query for retrieving the names of all currently running processes.
    /// </summary>
    /// <param name="processMonitor">The process monitor used to obtain the list of running process names. Cannot be null.</param>
    public GetRunningProcessesQuery(IProcessMonitor processMonitor)
    {
        _processMonitor = processMonitor;
    }

    /// <summary>
    ///     Retrieves the names of all currently running processes.
    /// </summary>
    /// <returns>
    ///     A read-only list of strings containing the names of all running processes. The list is empty if no processes are
    ///     running.
    /// </returns>
    public IReadOnlyList<string> Execute()
    {
        return _processMonitor.GetRunningProcessNames();
    }
}