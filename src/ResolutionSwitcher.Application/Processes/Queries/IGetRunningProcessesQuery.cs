namespace ResolutionSwitcher.Application.Processes.Queries;

/// <summary>
///     Represents a query for retrieving the names of all currently running processes on the system.
/// </summary>
/// <remarks>
///     Implementations of this interface should provide a mechanism to enumerate process names. The results
///     may vary depending on system permissions and platform-specific behavior.
/// </remarks>
public interface IGetRunningProcessesQuery
{
    /// <summary>
    ///     Executes the operation and retrieves a read-only list of result strings.
    /// </summary>
    /// <returns>
    ///     A read-only list of strings containing the results of the operation. The list is empty if there are no
    ///     results.
    /// </returns>
    IReadOnlyList<string> Execute();
}