using Microsoft.Win32;
using System.Runtime.Versioning;

namespace ResolutionSwitcher.Infrastructure.Startup;

/// <summary>
///     Provides methods for registering the current application to start automatically when the user logs into Windows.
/// </summary>
/// <remarks>
///     This class is intended for use on Windows platforms only. It modifies the current user's registry
///     settings to enable or update startup registration for the application. Use these methods to ensure the application
///     launches in the background at user login.
/// </remarks>
[SupportedOSPlatform("windows")]
public static class WindowsStartupRegistration
{
    private const string BackgroundArgument = "--background";
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "ResolutionSwitcher";

    /// <summary>
    ///     Ensures that the current application is registered to start automatically for the current user when Windows
    ///     starts.
    /// </summary>
    /// <remarks>
    ///     This method creates or updates the registry entry under the current user's Run key to launch
    ///     the application at user logon. If the entry already exists, it will be overwritten.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if the current executable path cannot be determined.</exception>
    public static void EnsureCurrentUserStartupRegistration()
    {
        var executablePath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            throw new InvalidOperationException("Unable to determine the current executable path for startup registration.");
        }

        using var runKey = Registry.CurrentUser.CreateSubKey(RunKeyPath);
        runKey.SetValue(ValueName, BuildCommand(executablePath), RegistryValueKind.String);
    }

    /// <summary>
    ///     Builds a command-line string to execute the specified executable with background execution arguments.
    /// </summary>
    /// <param name="executablePath">The file system path to the executable. This can be a relative or absolute path.</param>
    /// <returns>A command-line string that includes the full path to the executable and the required background argument.</returns>
    internal static string BuildCommand(string executablePath)
    {
        return $"\"{Path.GetFullPath(executablePath)}\" {BackgroundArgument}";
    }
}