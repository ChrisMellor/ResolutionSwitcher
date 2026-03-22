using System.Diagnostics;
using ResolutionSwitcher.Application.Abstractions.Processes;

namespace ResolutionSwitcher.Infrastructure.Processes.Monitoring;

/// <summary>
///     Provides functionality to monitor and query running processes on a Windows system.
/// </summary>
/// <remarks>
///     This class offers methods to retrieve the names of currently running processes and to check if a
///     specific process is active. It is intended for use on Windows platforms and may not function correctly on other
///     operating systems. Instances of this class are thread-safe for concurrent use.
/// </remarks>
public sealed class WindowsProcessMonitor : IProcessMonitor
{
    /// <summary>
    ///     Retrieves the names of all currently running processes on the local machine.
    /// </summary>
    /// <remarks>
    ///     Process names are returned without duplicates and are compared using case-insensitive ordinal
    ///     comparison. The method may not include processes that terminate during enumeration or for which access is
    ///     denied.
    /// </remarks>
    /// <returns>
    ///     A read-only list of unique process names representing all running processes. The list is sorted in a
    ///     case-insensitive manner and will be empty if no processes are found.
    /// </returns>
    public IReadOnlyList<string> GetRunningProcessNames()
    {
        var processNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var processes = Process.GetProcesses();

        foreach (var process in processes)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(process.ProcessName))
                {
                    processNames.Add(process.ProcessName);
                }
            }
            catch { }
            finally
            {
                process.Dispose();
            }
        }

        return processNames
              .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
              .ToArray();
    }

    /// <summary>
    ///     Determines whether a process with the specified name is currently running on the local machine.
    /// </summary>
    /// <remarks>
    ///     Process names are case-insensitive and should not include the ".exe" extension. This method
    ///     checks all processes running on the local computer.
    /// </remarks>
    /// <param name="processName">
    ///     The name of the process to check for. This should not include the file extension. Cannot be
    ///     null or empty.
    /// </param>
    /// <returns>true if at least one process with the specified name is running; otherwise, false.</returns>
    public bool IsRunning(string processName)
    {
        var processes = Process.GetProcessesByName(processName);

        try
        {
            return processes.Length > 0;
        }
        finally
        {
            foreach (var process in processes)
            {
                process.Dispose();
            }
    
   }
    }
}