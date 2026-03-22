using ResolutionSwitcher.Application.Configuration.Commands;
using ResolutionSwitcher.Application.Configuration.Queries;
using ResolutionSwitcher.Application.Display.Queries;
using ResolutionSwitcher.Application.Processes.Queries;
using ResolutionSwitcher.Application.Watcher.Runtime;
using ResolutionSwitcher.Application.Watcher.Status;
using ResolutionSwitcher.Desktop.Context;
using ResolutionSwitcher.Desktop.UI.Forms;
using ResolutionSwitcher.Infrastructure.Configuration.Paths;
using ResolutionSwitcher.Infrastructure.Configuration.Persistence;
using ResolutionSwitcher.Infrastructure.Display.Services;
using ResolutionSwitcher.Infrastructure.Processes.Monitoring;
using ResolutionSwitcher.Infrastructure.Startup.Registration;
using System.Runtime.Versioning;

namespace ResolutionSwitcher.Desktop;

/// <summary>
///     Provides the main entry point and startup logic for the ResolutionSwitcher application on Windows platforms.
/// </summary>
/// <remarks>
///     This class is intended for internal use and is not designed to be instantiated or accessed directly
///     by consumers. It ensures the application runs only on supported Windows operating systems and initializes core
///     services, configuration, and the main application context. The class also manages startup registration and handles
///     command-line arguments for background operation.
/// </remarks>
[SupportedOSPlatform("windows")]
internal static class Program
{
    /// <summary>
    ///     Ensures that the application is registered to start automatically with Windows for the current user.
    /// </summary>
    /// <remarks>
    ///     If startup registration fails and <paramref name="startHidden" /> is <see langword="false" />,
    ///     an error dialog is shown to inform the user. If <paramref name="startHidden" /> is <see langword="true" />, errors
    ///     are silently ignored.
    /// </remarks>
    /// <param name="startHidden">
    ///     Specifies whether to suppress error messages if startup registration fails. Set to <see langword="true" /> to
    ///     hide error dialogs; otherwise, an error message is displayed to the user.
    /// </param>
    private static void EnsureStartupRegistration(bool startHidden)
    {
        try
        {
            WindowsStartupRegistration.EnsureCurrentUserStartupRegistration();
        }
        catch (Exception exception)
        {
            if (startHidden)
            {
                return;
            }

            MessageBox.Show($"ResolutionSwitcher couldn't register itself to start with Windows.{Environment.NewLine}{Environment.NewLine}{exception.Message}",
                            "Startup Registration Failed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
        }
    }

    /// <summary>
    ///     Serves as the main entry point for the ResolutionSwitcher application.
    /// </summary>
    /// <remarks>
    ///     Initializes application configuration, checks for platform compatibility, processes
    ///     command-line arguments, and starts the application's main message loop. This method should not be called
    ///     directly; it is intended to be invoked by the operating system when the application starts.
    /// </remarks>
    [STAThread]
    private static void Main()
    {
        if (!OperatingSystem.IsWindows())
        {
            MessageBox.Show("ResolutionSwitcher only supports Windows.",
                            "Unsupported Platform",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
            return;
        }

        ApplicationConfiguration.Initialize();
        var startHidden = Environment.GetCommandLineArgs()
                                     .Skip(1)
                                     .Any(argument => string.Equals(argument, "--background", StringComparison.OrdinalIgnoreCase));
        EnsureStartupRegistration(startHidden);

        var configurationStore = new JsonConfigurationStore(ResolutionSwitcherPaths.ConfigPath);
        var displayService = new WindowsDisplayService();
        var processMonitor = new WindowsProcessMonitor();
        var loadConfigurationQuery = new LoadConfigurationQuery(configurationStore);
        var saveConfigurationCommand = new SaveConfigurationCommand(configurationStore);
        var getDisplaysQuery = new GetDisplaysQuery(displayService);
        var getDisplayModesQuery = new GetDisplayModesQuery(displayService);
        var getRunningProcessesQuery = new GetRunningProcessesQuery(processMonitor);
        var statusStore = new WatcherStatusStore();
        var mainForm = new MainForm(loadConfigurationQuery,
                                    saveConfigurationCommand,
                                    getDisplaysQuery,
                                    getDisplayModesQuery,
                                    getRunningProcessesQuery,
                                    configurationStore.ConfigPath);
        var watcherRuntime = new WatcherRuntime(loadConfigurationQuery, displayService, processMonitor, statusStore);
        using var context = new ResolutionSwitcherApplicationContext(configurationStore.ConfigPath,
                                                                     mainForm,
                                                                     statusStore,
                                                                     watcherRuntime,
                                                                     startHidden);
        System.Windows.Forms.Application.Run(context);
    }
}
