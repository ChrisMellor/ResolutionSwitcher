namespace ResolutionSwitcher.Application.Abstractions.Processes;

/// <summary>
///     Defines functionality for monitoring and querying the state of running processes on the system.
/// </summary>
public interface IProcessMonitor
{
    /// <summary>
    ///     Retrieves the names of all currently running processes on the system.
    /// </summary>
    /// <returns>
    ///     A read-only list of strings containing the names of all running processes. The list is empty if no processes are
    ///     running.
    /// </returns>
    IReadOnlyList<string> GetRunningProcessNames();

    /// <summary>
    ///     Determines whether a process with the specified name is currently running on the system.
    /// </summary>
    /// <param name="processName">
    ///     The name of the process to check. This value is case-insensitive and should not include the file extension.
    ///     Cannot be null or empty.
    /// </param>
    /// <returns>true if a process with the specified name is running; otherwise, false.</returns>
    bool IsRunning(string processName);
}