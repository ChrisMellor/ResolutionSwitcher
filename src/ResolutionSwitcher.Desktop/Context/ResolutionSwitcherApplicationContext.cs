using ResolutionSwitcher.Application.Abstractions.Watcher;
using ResolutionSwitcher.Application.Watcher.Runtime;
using ResolutionSwitcher.Application.Watcher.Status;
using ResolutionSwitcher.Desktop.UI.Forms;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace ResolutionSwitcher.Desktop.Context;

/// <summary>
///     Provides the application context for the Resolution Switcher system tray application, managing the main form, tray
///     icon, and background watcher lifecycle.
/// </summary>
/// <remarks>
///     This context is responsible for initializing and disposing of UI components, handling user
///     interactions from the system tray, and coordinating status updates from the background watcher. It ensures the
///     application remains accessible from the system tray when the main window is closed and manages application exit
///     procedures. This class is intended for use on Windows platforms only.
/// </remarks>
[SupportedOSPlatform("windows")]
internal sealed class ResolutionSwitcherApplicationContext : ApplicationContext
{
    private readonly string _configPath;
    private readonly MainForm _mainForm;
    private readonly IWatcherStatusFeed _statusFeed;
    private readonly ToolStripMenuItem _statusItem;
    private readonly NotifyIcon _trayIcon;
    private readonly ContextMenuStrip _trayMenu;
    private readonly SynchronizationContext _uiContext;
    private readonly WatcherRuntime _watcherRuntime;
    private bool _hasShownTrayHint;
    private bool _isExiting;

    /// <summary>
    ///     Initializes a new instance of the ResolutionSwitcherApplicationContext class, configuring the main application
    ///     context, system tray icon, and event handlers for the resolution switcher application.
    /// </summary>
    /// <remarks>
    ///     This constructor sets up the application's system tray icon, context menu, and event handlers
    ///     for user interactions and status updates. The watcher runtime is started automatically. If startHidden is false,
    ///     the main window is shown immediately after initialization.
    /// </remarks>
    /// <param name="configPath">
    ///     The file system path to the application's configuration file or directory. Cannot be null or
    ///     empty.
    /// </param>
    /// <param name="mainForm">The main form instance used as the primary user interface for the application. Cannot be null.</param>
    /// <param name="statusFeed">An object that provides status updates from the watcher component. Cannot be null.</param>
    /// <param name="watcherRuntime">The runtime controller responsible for managing the watcher process. Cannot be null.</param>
    /// <param name="startHidden">true to start the application with the main window hidden; otherwise, false.</param>
    public ResolutionSwitcherApplicationContext(string configPath,
        MainForm mainForm,
        IWatcherStatusFeed statusFeed,
        WatcherRuntime watcherRuntime,
        bool startHidden)
    {
        _configPath = configPath;
        _mainForm = mainForm;
        MainForm = mainForm;
        _statusFeed = statusFeed;
        _watcherRuntime = watcherRuntime;
        _uiContext = SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext();

        _statusItem = new ToolStripMenuItem("Starting watcher...") { Enabled = false };

        var openSettingsItem = new ToolStripMenuItem("Open settings");
        openSettingsItem.Click += (_, _) => ShowMainWindow();

        var scanNowItem = new ToolStripMenuItem("Scan now");
        scanNowItem.Click += (_, _) => _watcherRuntime.RequestRefresh();

        var openConfigItem = new ToolStripMenuItem("Open config folder");
        openConfigItem.Click += (_, _) => OpenConfigFolder();

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) => ExitApplication();

        _trayMenu = new ContextMenuStrip();
        _trayMenu.Items.Add(_statusItem);
        _trayMenu.Items.Add(new ToolStripSeparator());
        _trayMenu.Items.Add(openSettingsItem);
        _trayMenu.Items.Add(scanNowItem);
        _trayMenu.Items.Add(openConfigItem);
        _trayMenu.Items.Add(new ToolStripSeparator());
        _trayMenu.Items.Add(exitItem);

        _trayIcon = new NotifyIcon
        {
            Text = "Resolution Switcher",
            Icon = SystemIcons.Application,
            ContextMenuStrip = _trayMenu,
            Visible = true
        };
        _trayIcon.DoubleClick += (_, _) => ShowMainWindow();

        _mainForm.FormClosing += OnMainFormClosing;
        _mainForm.ConfigurationSaved += OnConfigurationSaved;

        _statusFeed.StatusChanged += OnStatusChanged;
        UpdateStatus(_statusFeed.Current);

        _watcherRuntime.Start();

        if (!startHidden)
        {
            ShowMainWindow();
        }
    }

    /// <summary>
    ///     Releases the unmanaged resources used by the component and optionally releases the managed resources.
    /// </summary>
    /// <remarks>
    ///     This method is called by both the public Dispose() method and the finalizer. When disposing
    ///     is true, this method releases all resources held by managed objects. When disposing is false, only unmanaged
    ///     resources are released. Override this method to release additional resources in derived classes.
    /// </remarks>
    /// <param name="disposing">
    ///     true to release both managed and unmanaged resources; false to release only unmanaged
    ///     resources.
    /// </param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _statusFeed.StatusChanged -= OnStatusChanged;
            _mainForm.FormClosing -= OnMainFormClosing;
            _mainForm.ConfigurationSaved -= OnConfigurationSaved;
            _watcherRuntime.Dispose();
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _trayMenu.Dispose();
            _mainForm.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    ///     Terminates the application by closing the main form and exiting the message loop.
    /// </summary>
    /// <remarks>
    ///     Call this method to initiate a clean shutdown of the application. This method sets the
    ///     application state to exiting, closes the main form, and ends the current thread's message loop. After calling
    ///     this method, the application will begin the shutdown process and no further user interaction will be
    ///     possible.
    /// </remarks>
    private void ExitApplication()
    {
        _isExiting = true;
        _mainForm.Close();
        ExitThread();
    }

    /// <summary>
    ///     Handles the event that occurs after the configuration has been saved.
    /// </summary>
    /// <param name="sender">The source of the event. This is typically the object that raised the event.</param>
    /// <param name="e">An object that contains the event data.</param>
    private void OnConfigurationSaved(object? sender, EventArgs e)
    {
        _watcherRuntime.RequestRefresh();
    }

    /// <summary>
    ///     Handles the main form's closing event to minimize the application to the system tray instead of exiting when
    ///     closed by the user.
    /// </summary>
    /// <remarks>
    ///     If the user attempts to close the main form, the application is minimized to the system tray
    ///     and continues running. A notification balloon is shown the first time this occurs to inform the user. The
    ///     application will only exit if explicitly requested or closed for reasons other than user action.
    /// </remarks>
    /// <param name="sender">The source of the event, typically the main form.</param>
    /// <param name="e">
    ///     A FormClosingEventArgs that contains the event data, including the reason for closing and the ability to cancel
    ///     the close operation.
    /// </param>
    private void OnMainFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_isExiting || (e.CloseReason != CloseReason.UserClosing))
        {
            return;
        }

        e.Cancel = true;
        _mainForm.Hide();
        _mainForm.ShowInTaskbar = false;

        if (_hasShownTrayHint)
        {
            return;
        }

        _hasShownTrayHint = true;
        _trayIcon.ShowBalloonTip(2500,
                                 "Resolution Switcher",
                                 "Still running in the system tray.",
                                 ToolTipIcon.Info);
    }

    /// <summary>
    ///     Handles changes in the watcher status and updates the user interface accordingly.
    /// </summary>
    /// <param name="sender">
    ///     The source of the status change event. This parameter is typically the watcher instance that
    ///     raised the event.
    /// </param>
    /// <param name="status">A snapshot of the current watcher status to be reflected in the user interface. Cannot be null.</param>
    private void OnStatusChanged(object? sender, WatcherStatusSnapshot status)
    {
        _uiContext.Post(_ => UpdateStatus(status), null);
    }

    /// <summary>
    ///     Opens the folder containing the configuration file in Windows Explorer and selects the configuration file.
    /// </summary>
    /// <remarks>
    ///     If the configuration file's directory does not exist, it is created before opening Explorer.
    ///     This method is intended for use on Windows systems where Explorer is available.
    /// </remarks>
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

    /// <summary>
    ///     Displays the main application window and brings it to the foreground if it is minimized or not currently
    ///     visible.
    /// </summary>
    /// <remarks>
    ///     This method ensures that the main window is visible, restored from a minimized state if
    ///     necessary, and activated so it appears in front of other windows. Use this method to programmatically focus the
    ///     main window in response to user actions or application events.
    /// </remarks>
    private void ShowMainWindow()
    {
        if (!_mainForm.Visible)
        {
            _mainForm.Show();
        }

        if (_mainForm.WindowState == FormWindowState.Minimized)
        {
            _mainForm.WindowState = FormWindowState.Normal;
        }

        _mainForm.ShowInTaskbar = true;
        _mainForm.Activate();
        _mainForm.BringToFront();
    }

    /// <summary>
    ///     Updates the status display and shows a notification if the provided status indicates an error.
    /// </summary>
    /// <remarks>
    ///     If the status represents an error, a warning notification is shown to the user. This method
    ///     updates both the status text and, when appropriate, the system tray notification.
    /// </remarks>
    /// <param name="status">The status snapshot containing the timestamp, message, and error state to display. Cannot be null.</param>
    private void UpdateStatus(WatcherStatusSnapshot status)
    {
        _statusItem.Text = $@"{status.Timestamp:HH:mm:ss}  {status.Message}";

        if (status.IsError)
        {
            _trayIcon.ShowBalloonTip(3000,
                                     "Resolution Switcher",
                                     status.Message,
                                     ToolTipIcon.Warning);
        }
    }
}