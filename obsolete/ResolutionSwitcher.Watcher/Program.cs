using ResolutionSwitcher.Application.Configuration.Queries;
using ResolutionSwitcher.Application.Watcher;
using ResolutionSwitcher.Infrastructure.Windows.Configuration;
using ResolutionSwitcher.Infrastructure.Windows.Display;
using ResolutionSwitcher.Infrastructure.Windows.Processes;
using System.Runtime.Versioning;

namespace ResolutionSwitcher.Watcher;

[SupportedOSPlatform("windows")]
internal static class Program
{
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

        var configurationStore = new JsonConfigurationStore(ResolutionSwitcherPaths.ConfigPath);
        var displayService = new WindowsDisplayService();
        var processMonitor = new WindowsProcessMonitor();
        var loadConfigurationQuery = new LoadConfigurationQuery(configurationStore);
        var statusStore = new WatcherStatusStore();
        using var watcherRuntime = new WatcherRuntime(loadConfigurationQuery, displayService, processMonitor, statusStore);
        using var context = new WatcherApplicationContext(configurationStore.ConfigPath, statusStore, watcherRuntime);
        System.Windows.Forms.Application.Run(context);
    }
}