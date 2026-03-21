using ResolutionSwitcher.Application.Abstractions.Display;
using ResolutionSwitcher.Application.Abstractions.Processes;
using ResolutionSwitcher.Domain.Configuration;
using ResolutionSwitcher.Domain.Display;
using System.Runtime.Versioning;

namespace ResolutionSwitcher.Application.Watcher;

/// <summary>
///     Monitors running processes and automatically applies or restores display resolutions based on active game profiles.
/// </summary>
/// <remarks>
///     ResolutionWatcher is intended for use on Windows platforms. It tracks the original display
///     resolutions and ensures that changes made for specific game profiles are reverted when those profiles are no longer
///     active.
/// </remarks>
[SupportedOSPlatform("windows")]
internal sealed class ResolutionWatcher
{
    private readonly IDisplayService _displayService;
    private readonly Dictionary<string, DisplayResolution> _originalModes = new(StringComparer.OrdinalIgnoreCase);
    private readonly IProcessMonitor _processMonitor;
    private string? _activeDeviceKey;
    private string? _activeProfileKey;

    /// <summary>
    ///     Initializes a new instance of the ResolutionWatcher class with the specified display service and process monitor.
    /// </summary>
    /// <param name="displayService">The display service used to retrieve and monitor display resolution changes.</param>
    /// <param name="processMonitor">The process monitor used to track the state of relevant processes.</param>
    public ResolutionWatcher(IDisplayService displayService, IProcessMonitor processMonitor)
    {
        _displayService = displayService;
        _processMonitor = processMonitor;
    }

    /// <summary>
    ///     Gets a value indicating whether there is an active profile currently set.
    /// </summary>
    public bool HasActiveProfile => _activeProfileKey is not null;

    /// <summary>
    ///     Evaluates the specified application configuration and applies the first running profile, or restores all
    ///     profiles if none are running.
    /// </summary>
    /// <remarks>
    ///     If no running profile is found and an active profile exists, all profiles are restored to
    ///     their original state. The method reports status updates through the provided delegate as profiles are applied or
    ///     restored.
    /// </remarks>
    /// <param name="configuration">The application configuration containing the set of profiles to evaluate. Cannot be null.</param>
    /// <param name="reportStatus">An action delegate used to report status messages during evaluation. Cannot be null.</param>
    public void Evaluate(AppConfiguration configuration, Action<string> reportStatus)
    {
        var matchingProfile = FindFirstRunningProfile(configuration.Profiles);

        if (matchingProfile is null)
        {
            if (_activeProfileKey is not null)
            {
                RestoreAll(reportStatus);
            }

            return;
        }

        ApplyProfile(matchingProfile, reportStatus);
    }

    /// <summary>
    ///     Restores all devices to their original display modes and resets the active profile and device state.
    /// </summary>
    /// <remarks>
    ///     Use this method to revert all managed devices to their initial display configurations. This
    ///     operation also clears any active profile or device selection, returning the system to its default state. The
    ///     provided callback can be used to display progress or log status updates during the restore process.
    /// </remarks>
    /// <param name="reportStatus">
    ///     A callback action that receives status messages for each device as it is restored. The string parameter provides
    ///     information about the current operation or device being processed.
    /// </param>
    public void RestoreAll(Action<string> reportStatus)
    {
        foreach (var deviceKey in _originalModes.Keys.ToArray())
        {
            RestoreDisplay(deviceKey, reportStatus);
        }

        _activeProfileKey = null;
        _activeDeviceKey = null;
    }

    /// <summary>
    ///     Applies the specified game profile's display settings to the target device and reports status updates.
    /// </summary>
    /// <remarks>
    ///     If a different device is currently active, its display settings are restored before applying
    ///     the new profile. The method ensures the requested resolution is set, and status updates are provided to the
    ///     caller. The method does not throw exceptions for invalid profiles; callers should ensure valid input.
    /// </remarks>
    /// <param name="profile">The game profile containing the display configuration to apply. Cannot be null.</param>
    /// <param name="reportStatus">
    ///     A callback action used to report status messages during the profile application process.
    ///     Cannot be null.
    /// </param>
    private void ApplyProfile(GameProfile profile, Action<string> reportStatus)
    {
        var targetDeviceKey = DisplayNames.ToDeviceKey(profile.DisplayName);

        if (_activeDeviceKey is not null
         && !string.Equals(_activeDeviceKey, targetDeviceKey, StringComparison.OrdinalIgnoreCase))
        {
            RestoreDisplay(_activeDeviceKey, reportStatus);
        }

        if (!_originalModes.ContainsKey(targetDeviceKey))
        {
            _originalModes[targetDeviceKey] = _displayService.CaptureCurrentResolution(profile.DisplayName);
        }

        var currentMode = _displayService.CaptureCurrentResolution(profile.DisplayName);
        var requestedMode = _displayService.ResolveRequestedResolution(profile);
        var modeAlreadyApplied = _displayService.IsSameResolution(currentMode, requestedMode);
        var profileChanged = !string.Equals(_activeProfileKey, profile.UniqueKey, StringComparison.Ordinal);

        if (!modeAlreadyApplied)
        {
            _displayService.ApplyResolution(requestedMode);
            reportStatus($"Detected {profile.ProfileLabel}. Switched {DisplayNames.Describe(profile.DisplayName)} to {_displayService.FormatResolution(requestedMode)}.");
        }
        else if (profileChanged)
        {
            reportStatus($"{profile.ProfileLabel} is running. {DisplayNames.Describe(profile.DisplayName)} is already set to {_displayService.FormatResolution(requestedMode)}.");
        }

        _activeProfileKey = profile.UniqueKey;
        _activeDeviceKey = targetDeviceKey;
    }

    /// <summary>
    ///     Searches the specified collection of game profiles and returns the first profile whose associated process is
    ///     currently running.
    /// </summary>
    /// <param name="profiles">A collection of game profiles to search for a running process. Cannot be null.</param>
    /// <returns>
    ///     The first game profile in the collection whose process is running; otherwise, null if no running profile is
    ///     found.
    /// </returns>
    private GameProfile? FindFirstRunningProfile(IEnumerable<GameProfile> profiles)
    {
        foreach (var profile in profiles)
        {
            if (_processMonitor.IsRunning(profile.NormalizedProcessName))
            {
                return profile;
            }
        }

        return null;
    }

    /// <summary>
    ///     Restores the display settings for the specified device to their original mode and reports the status of the
    ///     operation.
    /// </summary>
    /// <remarks>
    ///     If the original display mode for the specified device is not available, the method performs
    ///     no action. After restoration, the original mode is removed from the internal tracking collection.
    /// </remarks>
    /// <param name="deviceKey">The unique key identifying the display device whose settings are to be restored.</param>
    /// <param name="reportStatus">
    ///     An action delegate that receives a status message describing the result of the restore
    ///     operation.
    /// </param>
    private void RestoreDisplay(string deviceKey, Action<string> reportStatus)
    {
        if (!_originalModes.TryGetValue(deviceKey, out var originalMode))
        {
            return;
        }

        _displayService.ApplyResolution(originalMode);
        reportStatus($"Restored {DisplayNames.Describe(originalMode.DisplayName)} to {_displayService.FormatResolution(originalMode)}.");
        _originalModes.Remove(deviceKey);
    }
}