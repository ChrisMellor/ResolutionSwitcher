using ResolutionSwitcher.Application.Abstractions.Watcher;
using ResolutionSwitcher.Application.Watcher;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace ResolutionSwitcher.Watcher;

[SupportedOSPlatform("windows")]
internal sealed class WatcherApplicationContext : ApplicationContext
{
    private readonly string _configPath;
    private readonly ToolStripMenuItem _statusItem;
    private readonly IWatcherStatusFeed _statusFeed;
    private readonly NotifyIcon _trayIcon;
    private readonly ContextMenuStrip _trayMenu;
    private readonly SynchronizationContext _uiContext;
    private readonly WatcherRuntime _watcherRuntime;

    public WatcherApplicationContext(string configPath,
        IWatcherStatusFeed statusFeed,
        WatcherRuntime watcherRuntime)
    {
        _configPath = configPath;
        _statusFeed = statusFeed;
        _watcherRuntime = watcherRuntime;
        _uiContext = SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext();

        _statusItem = new ToolStripMenuItem("Starting watcher...") { Enabled = false };

        var scanNowItem = new ToolStripMenuItem("Scan now");
        scanNowItem.Click += (_, _) => _watcherRuntime.RequestRefresh();

        var openConfigItem = new ToolStripMenuItem("Open config folder");
        openConfigItem.Click += (_, _) => OpenConfigFolder();

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) => ExitThread();

        _trayMenu = new ContextMenuStrip();
        _trayMenu.Items.Add(_statusItem);
        _trayMenu.Items.Add(new ToolStripSeparator());
        _trayMenu.Items.Add(scanNowItem);
        _trayMenu.Items.Add(openConfigItem);
        _trayMenu.Items.Add(new ToolStripSeparator());
        _trayMenu.Items.Add(exitItem);

        _trayIcon = new NotifyIcon
        {
            Text = "Resolution Switcher Watcher",
            Icon = SystemIcons.Application,
            ContextMenuStrip = _trayMenu,
            Visible = true
        };
        _trayIcon.DoubleClick += (_, _) => OpenConfigFolder();

        _statusFeed.StatusChanged += OnStatusChanged;
        UpdateStatus(_statusFeed.Current);

        _watcherRuntime.Start();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _statusFeed.StatusChanged -= OnStatusChanged;
            _watcherRuntime.Dispose();
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _trayMenu.Dispose();
        }

        base.Dispose(disposing);
    }

    private void OnStatusChanged(object? sender, WatcherStatusSnapshot status)
    {
        _uiContext.Post(_ => UpdateStatus(status), null);
    }

    private void OpenConfigFolder()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);

        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"/select,\"{_configPath}\"",
            UseShellExecute = true
        });
    }

    private void UpdateStatus(WatcherStatusSnapshot status)
    {
        _statusItem.Text = $"{status.Timestamp:HH:mm:ss}  {status.Message}";

        if (status.IsError)
        {
            _trayIcon.ShowBalloonTip(3000,
                                     "Resolution Switcher Watcher",
                                     status.Message,
                                     ToolTipIcon.Warning);
        }
    }
}