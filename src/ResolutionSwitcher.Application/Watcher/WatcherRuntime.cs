using ResolutionSwitcher.Application.Abstractions.Display;
using ResolutionSwitcher.Application.Abstractions.Processes;
using ResolutionSwitcher.Application.Abstractions.Watcher;
using ResolutionSwitcher.Application.Configuration.Queries;
using ResolutionSwitcher.Application.Exceptions;
using System.Runtime.Versioning;

namespace ResolutionSwitcher.Application.Watcher;

/// <summary>
///     Provides runtime management for the display resolution watcher, including starting, stopping, and refreshing the
///     watcher process. This class is intended for use on Windows platforms only.
/// </summary>
/// <remarks>
///     WatcherRuntime coordinates background monitoring of display and process state, periodically applying
///     configuration profiles and publishing status updates. It is not thread-safe for concurrent Start or Dispose calls.
///     Dispose must be called to release resources when the watcher is no longer needed.
/// </remarks>
[SupportedOSPlatform("windows")]
public sealed class WatcherRuntime : IDisposable
{
    private readonly ILoadConfigurationQuery _loadConfigurationQuery;
    private readonly SemaphoreSlim _refreshSignal = new(0, 1);
    private readonly ResolutionWatcher _resolutionWatcher;
    private readonly IWatcherStatusPublisher _statusPublisher;
    private Task? _backgroundTask;
    private CancellationTokenSource? _cancellationTokenSource;

    public WatcherRuntime(ILoadConfigurationQuery loadConfigurationQuery,
        IDisplayService displayService,
        IProcessMonitor processMonitor,
        IWatcherStatusPublisher statusPublisher)
    {
        _loadConfigurationQuery = loadConfigurationQuery;
        _resolutionWatcher = new ResolutionWatcher(displayService, processMonitor);
        _statusPublisher = statusPublisher;
    }

    public void Dispose()
    {
        if (_cancellationTokenSource is null || _backgroundTask is null)
        {
            return;
        }

        _cancellationTokenSource.Cancel();

        try
        {
            _backgroundTask.GetAwaiter()
                           .GetResult();
        }
        catch (OperationCanceledException) { }
        finally
        {
            _cancellationTokenSource.Dispose();
            _refreshSignal.Dispose();
            _backgroundTask = null;
            _cancellationTokenSource = null;
        }
    }

    public void RequestRefresh()
    {
        if (_refreshSignal.CurrentCount == 0)
        {
            _refreshSignal.Release();
        }
    }

    public void Start()
    {
        if (_backgroundTask is not null)
        {
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _backgroundTask = Task.Run(() => RunAsync(_cancellationTokenSource.Token));
    }

    private void ReportError(string message)
    {
        _statusPublisher.Publish(message, true);
    }

    private void ReportStatus(string message)
    {
        _statusPublisher.Publish(message);
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var delay = TimeSpan.FromSeconds(3);

                try
                {
                    var configuration = _loadConfigurationQuery.Execute();
                    delay = TimeSpan.FromSeconds(configuration.PollIntervalSeconds);

                    if (!configuration.WatcherEnabled)
                    {
                        TryRestoreAll();
                        ReportStatus("Watcher paused.");
                    }
                    else if (configuration.Profiles.Count == 0)
                    {
                        TryRestoreAll();
                        ReportStatus("No game profiles configured. Watching is idle.");
                    }
                    else
                    {
                        _resolutionWatcher.Evaluate(configuration, ReportStatus);

                        if (!_resolutionWatcher.HasActiveProfile)
                        {
                            ReportStatus($"Watching {configuration.Profiles.Count} game profile(s).");
                        }
                    }
                }
                catch (ConfigurationException exception)
                {
                    TryRestoreAll();
                    ReportError($"Configuration error: {exception.Message}");
                    delay = TimeSpan.FromSeconds(5);
                }
                catch (DisplaySettingsException exception)
                {
                    ReportError($"Display error: {exception.Message}");
                    delay = TimeSpan.FromSeconds(5);
                }
                catch (Exception exception)
                {
                    ReportError($"Watcher error: {exception.Message}");
                    delay = TimeSpan.FromSeconds(5);
                }

                await WaitForNextCycleAsync(delay, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        finally
        {
            TryRestoreAll();
            _statusPublisher.Publish("Watcher stopped.");
        }
    }

    private void TryRestoreAll()
    {
        try
        {
            _resolutionWatcher.RestoreAll(ReportStatus);
        }
        catch (DisplaySettingsException exception)
        {
            ReportError($"Display error: {exception.Message}");
        }
    }

    private async Task WaitForNextCycleAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        var refreshTask = _refreshSignal.WaitAsync(cancellationToken);
        var delayTask = Task.Delay(delay, cancellationToken);
        var completedTask = await Task.WhenAny(refreshTask, delayTask);

        if (completedTask == refreshTask)
        {
            await refreshTask;
        }
    }
}