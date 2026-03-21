using ResolutionSwitcher.Application.Abstractions.Display;
using ResolutionSwitcher.Application.Exceptions;
using ResolutionSwitcher.Domain.Configuration;
using ResolutionSwitcher.Domain.Display;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ResolutionSwitcher.Infrastructure.Display;

/// <summary>
///     Provides Windows-specific functionality for querying and modifying display devices and their supported resolutions.
/// </summary>
/// <remarks>
///     This class is intended for use on Windows platforms and enables applications to enumerate connected
///     displays, retrieve supported display modes, capture the current resolution, and apply new display settings. All
///     operations are performed using native Windows APIs. The class is sealed and not intended for inheritance. Thread
///     safety is not guaranteed; callers should ensure appropriate synchronization if accessing from multiple
///     threads.
/// </remarks>
[SupportedOSPlatform("windows")]
public sealed class WindowsDisplayService : IDisplayService
{
    private const int EnumCurrentSettings = -1;

    private const int DisplayChangeSuccessful = 0;
    private const int DisplayChangeRestart = 1;
    private const int DisplayChangeBadMode = -2;
    private const int DisplayChangeFailed = -1;
    private const int DisplayChangeBadFlags = -4;
    private const int DisplayChangeBadParam = -5;
    private const int DisplayChangeNotUpdated = -3;
    private const int DisplayChangeBadDualView = -6;

    private const uint CdsTest = 0x00000002;

    private const uint DmBitsPerPixel = 0x00040000;
    private const uint DmPixelsWidth = 0x00080000;
    private const uint DmPixelsHeight = 0x00100000;
    private const uint DmDisplayFrequency = 0x00400000;

    private const uint DisplayDeviceAttachedToDesktop = 0x00000001;
    private const uint DisplayDevicePrimaryDevice = 0x00000004;

    /// <summary>
    ///     Attempts to apply the specified display resolution to the target display device. Validates the resolution before
    ///     making changes to ensure compatibility.
    /// </summary>
    /// <remarks>
    ///     This method first tests the requested resolution for compatibility before applying it. If the
    ///     resolution is not supported or the operation fails, no changes are made to the display configuration.
    /// </remarks>
    /// <param name="resolution">
    ///     The display resolution settings to apply. Must specify a valid display device and supported
    ///     resolution.
    /// </param>
    /// <exception cref="DisplaySettingsException">
    ///     Thrown if the resolution is rejected during validation or if applying the
    ///     resolution fails.
    /// </exception>
    public void ApplyResolution(DisplayResolution resolution)
    {
        var nativeMode = ResolveNativeMode(resolution);
        var testResult = ChangeDisplaySettingsEx(resolution.DisplayName, ref nativeMode, nint.Zero, CdsTest, nint.Zero);
        if (testResult != DisplayChangeSuccessful)
        {
            throw new DisplaySettingsException(
                $"Windows rejected {DisplayNames.Describe(resolution.DisplayName)} mode {FormatResolution(resolution)} during validation ({DescribeChangeResult(testResult)}).");
        }

        var applyResult = ChangeDisplaySettingsEx(resolution.DisplayName, ref nativeMode, nint.Zero, 0, nint.Zero);
        if (applyResult != DisplayChangeSuccessful)
        {
            throw new DisplaySettingsException(
                $"Windows failed to apply {DisplayNames.Describe(resolution.DisplayName)} mode {FormatResolution(resolution)} ({DescribeChangeResult(applyResult)}).");
        }
    }

    /// <summary>
    ///     Retrieves the current display resolution settings for the specified display device.
    /// </summary>
    /// <param name="displayName">The name of the display device to query, or null to specify the primary display device.</param>
    /// <returns>A DisplayResolution object representing the current resolution settings of the specified display device.</returns>
    /// <exception cref="DisplaySettingsException">
    ///     Thrown if the current display mode cannot be read for the specified display
    ///     device.
    /// </exception>
    public DisplayResolution CaptureCurrentResolution(string? displayName)
    {
        var nativeMode = CreateEmptyDevMode();
        if (!EnumDisplaySettings(displayName, EnumCurrentSettings, ref nativeMode))
        {
            throw new DisplaySettingsException($"Unable to read the current mode for {DisplayNames.Describe(displayName)}.");
        }

        return ToDisplayResolution(displayName, nativeMode);
    }

    /// <summary>
    ///     Formats the specified display resolution as a human-readable string.
    /// </summary>
    /// <param name="resolution">The display resolution to format. Must not be null.</param>
    /// <returns>A string representing the resolution in the format "{width}x{height} @ {refreshRate}Hz".</returns>
    public string FormatResolution(DisplayResolution resolution)
    {
        return $"{resolution.Width}x{resolution.Height} @ {resolution.RefreshRate}Hz";
    }

    /// <summary>
    ///     Retrieves a read-only list of display devices currently attached to the desktop.
    /// </summary>
    /// <remarks>
    ///     The returned list includes only displays that are currently attached to the Windows desktop.
    ///     If no displays are detected, a default entry for the primary display is included to ensure the list is never
    ///     empty. The order of the displays in the list corresponds to their enumeration by the operating system.
    /// </remarks>
    /// <returns>
    ///     A read-only list of <see cref="DisplayDeviceInfo" /> objects representing each display device attached to the
    ///     desktop. The list contains at least one entry representing the primary display, even if no displays are
    ///     detected.
    /// </returns>
    public IReadOnlyList<DisplayDeviceInfo> GetDisplays()
    {
        var displays = new List<DisplayDeviceInfo>();

        for (uint displayIndex = 0; ; displayIndex++)
        {
            var displayDevice = CreateEmptyDisplayDevice();
            if (!EnumDisplayDevices(null, displayIndex, ref displayDevice, 0))
            {
                break;
            }

            if ((displayDevice.StateFlags & DisplayDeviceAttachedToDesktop) == 0)
            {
                continue;
            }

            var isPrimary = (displayDevice.StateFlags & DisplayDevicePrimaryDevice) != 0;
            var friendlyName = string.IsNullOrWhiteSpace(displayDevice.DeviceString)
                ? displayDevice.DeviceName
                : displayDevice.DeviceString;
            var bounds = GetDisplayBounds(displayDevice.DeviceName);

            displays.Add(new DisplayDeviceInfo(displayDevice.DeviceName,
                                               isPrimary
                                                   ? $"{friendlyName} (Primary)"
                                                   : friendlyName,
                                               isPrimary,
                                               bounds));
        }

        if (displays.Count == 0)
        {
            displays.Add(new DisplayDeviceInfo(null,
                                               "Primary display",
                                               true,
                                               GetDisplayBounds(null)));
        }

        return displays;
    }

    /// <summary>
    ///     Retrieves a list of supported display modes for the specified display device.
    /// </summary>
    /// <remarks>
    ///     Each display mode option includes the width, height, and refresh rate (if available)
    ///     supported by the display device. This method does not guarantee that the returned modes are currently active or
    ///     available for immediate use; it only reflects the modes reported by the system.
    /// </remarks>
    /// <param name="displayName">The name of the display device to query, or null to specify the primary display device.</param>
    /// <returns>
    ///     A read-only list of supported display mode options for the specified display. The list is ordered by width,
    ///     height, and refresh rate in descending order. The list is empty if no supported modes are found.
    /// </returns>
    public IReadOnlyList<DisplayModeOption> GetSupportedModes(string? displayName)
    {
        var modes = new HashSet<DisplayModeOption>();

        for (var modeIndex = 0; ; modeIndex++)
        {
            var nativeMode = CreateEmptyDevMode();
            if (!EnumDisplaySettings(displayName, modeIndex, ref nativeMode))
            {
                break;
            }

            if ((nativeMode.dmPixelsWidth == 0) || (nativeMode.dmPixelsHeight == 0))
            {
                continue;
            }

            modes.Add(new DisplayModeOption((int)nativeMode.dmPixelsWidth,
                                            (int)nativeMode.dmPixelsHeight,
                                            nativeMode.dmDisplayFrequency == 0
                                                ? null
                                                : (int)nativeMode.dmDisplayFrequency));
        }

        return modes
              .OrderByDescending(mode => mode.Width)
              .ThenByDescending(mode => mode.Height)
              .ThenByDescending(mode => mode.RefreshRate ?? 0)
              .ToList();
    }

    /// <summary>
    ///     Determines whether two display resolutions are equivalent based on device key, width, height, refresh rate, and
    ///     bits per pixel.
    /// </summary>
    /// <param name="left">The first display resolution to compare.</param>
    /// <param name="right">The second display resolution to compare.</param>
    /// <returns>
    ///     true if both display resolutions have the same device key, width, height, refresh rate, and bits per pixel;
    ///     otherwise, false.
    /// </returns>
    public bool IsSameResolution(DisplayResolution left, DisplayResolution right)
    {
        return string.Equals(left.DeviceKey, right.DeviceKey, StringComparison.OrdinalIgnoreCase)
            && (left.Width == right.Width)
            && (left.Height == right.Height)
            && (left.RefreshRate == right.RefreshRate)
            && (left.BitsPerPixel == right.BitsPerPixel);
    }

    /// <summary>
    ///     Finds and returns a supported display resolution that matches the specified game profile's width, height, and
    ///     optional refresh rate.
    /// </summary>
    /// <remarks>
    ///     If the profile does not specify a refresh rate, the method selects the mode with the highest
    ///     available refresh rate for the given resolution. This method is typically used to ensure that the display is set
    ///     to a mode compatible with the game's requirements.
    /// </remarks>
    /// <param name="profile">
    ///     The game profile specifying the desired display name, resolution width and height, and an optional refresh rate
    ///     to match.
    /// </param>
    /// <returns>
    ///     A DisplayResolution object representing the supported display mode that matches the requested profile. If no
    ///     refresh rate is specified, the mode with the highest available refresh rate is selected.
    /// </returns>
    /// <exception cref="DisplaySettingsException">
    ///     Thrown if no supported display mode matches the requested width, height, and optional refresh rate for the
    ///     specified display.
    /// </exception>
    public DisplayResolution ResolveRequestedResolution(GameProfile profile)
    {
        var matches = new List<Win32DeviceMode>();

        for (var modeIndex = 0; ; modeIndex++)
        {
            var nativeMode = CreateEmptyDevMode();
            if (!EnumDisplaySettings(profile.DisplayName, modeIndex, ref nativeMode))
            {
                break;
            }

            if ((nativeMode.dmPixelsWidth != profile.Width) || (nativeMode.dmPixelsHeight != profile.Height))
            {
                continue;
            }

            if (profile.RefreshRate.HasValue && (nativeMode.dmDisplayFrequency != profile.RefreshRate.Value))
            {
                continue;
            }

            nativeMode.dmFields = DmPixelsWidth | DmPixelsHeight | DmBitsPerPixel | DmDisplayFrequency;
            matches.Add(nativeMode);
        }

        if (matches.Count == 0)
        {
            throw new DisplaySettingsException(
                $"No supported display mode matched {profile.Width}x{profile.Height}{FormatRefreshRate(profile.RefreshRate)} for {profile.ProfileLabel} on {DisplayNames.Describe(profile.DisplayName)}.");
        }

        var selectedMode = profile.RefreshRate.HasValue
            ? matches[0]
            : matches.OrderByDescending(mode => mode.dmDisplayFrequency)
                     .First();

        return ToDisplayResolution(profile.DisplayName, selectedMode);
    }

    /// <summary>
    ///     Changes the display settings for the specified display device to the settings specified in a Win32 device mode structure.
    /// </summary>
    /// <remarks>
    ///     This method is a platform invoke (P/Invoke) signature for the native ChangeDisplaySettingsEx
    ///     function in user32.dll. It is intended for advanced scenarios involving direct manipulation of display settings.
    ///     Incorrect usage may result in system instability or display issues. Callers should ensure that the Win32 device mode
    ///     structure is properly initialized and that all parameters are valid before invoking this method.
    /// </remarks>
    /// <param name="deviceName">The name of the display device to be changed. Pass null to specify the current display device.</param>
    /// <param name="devMode">A reference to a Win32 device mode structure that describes the new display settings to apply.</param>
    /// <param name="hwnd">
    ///     A handle to the window that will own any user interface related to the display change. This parameter is
    ///     typically set to IntPtr.Zero.
    /// </param>
    /// <param name="flags">
    ///     A set of flags that determine how the display settings are changed. These flags control options such as whether
    ///     the change is temporary or permanent.
    /// </param>
    /// <param name="lParam">
    ///     A pointer to additional data used for private data or reserved for future use. Typically set to
    ///     IntPtr.Zero.
    /// </param>
    /// <returns>
    ///     An integer value indicating the result of the operation. Zero indicates success; a nonzero value indicates an
    ///     error. See the Windows API documentation for possible return codes.
    /// </returns>
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int ChangeDisplaySettingsEx(string? deviceName, ref Win32DeviceMode devMode, nint hwnd, uint flags, nint lParam);

    /// <summary>
    ///     Creates a new instance of the Win32 device mode structure with default values for device and form names.
    /// </summary>
    /// <remarks>
    ///     This method is useful when a valid, but uninitialized, Win32 device mode structure is required for
    ///     device configuration or enumeration operations.
    /// </remarks>
    /// <returns>A Win32 device mode structure initialized with empty device and form names, and the appropriate structure size.</returns>
    private static Win32DeviceMode CreateEmptyDevMode()
    {
        return new Win32DeviceMode
        {
            dmDeviceName = string.Empty,
            dmFormName = string.Empty,
            dmSize = (short)Marshal.SizeOf<Win32DeviceMode>()
        };
    }

    /// <summary>
    ///     Creates a new instance of the Win32 display device structure with all string fields initialized to empty values.
    /// </summary>
    /// <remarks>
    ///     This method is useful for obtaining a properly initialized Win32 display device structure before
    ///     passing it to native APIs that require the cb field to be set to the structure's size.
    /// </remarks>
    /// <returns>A Win32 display device structure with its string fields set to empty and the cb field set to the correct size.</returns>
    private static Win32DisplayDevice CreateEmptyDisplayDevice()
    {
        return new Win32DisplayDevice
        {
            cb = Marshal.SizeOf<Win32DisplayDevice>(),
            DeviceName = string.Empty,
            DeviceString = string.Empty,
            DeviceID = string.Empty,
            DeviceKey = string.Empty
        };
    }

    /// <summary>
    ///     Returns a human-readable description for a display change result code.
    /// </summary>
    /// <remarks>
    ///     This method is typically used to translate system-defined display change result codes into
    ///     user-friendly messages for logging or display purposes.
    /// </remarks>
    /// <param name="result">The integer result code representing the outcome of a display change operation.</param>
    /// <returns>
    ///     A string describing the meaning of the specified result code. Returns a generic message if the code is
    ///     unrecognized.
    /// </returns>
    private static string DescribeChangeResult(int result)
    {
        return result switch
        {
            DisplayChangeSuccessful => "success",
            DisplayChangeRestart => "restart required",
            DisplayChangeBadMode => "unsupported mode",
            DisplayChangeFailed => "driver failure",
            DisplayChangeBadFlags => "invalid flags",
            DisplayChangeBadParam => "invalid parameters",
            DisplayChangeNotUpdated => "registry update failed",
            DisplayChangeBadDualView => "dual-view conflict",
            _ => $"unknown result {result}"
        };
    }

    /// <summary>
    ///     Enumerates display devices attached to the desktop and returns information about each device.
    /// </summary>
    /// <remarks>
    ///     This method is a wrapper for the native EnumDisplayDevices function in user32.dll. It must be
    ///     called repeatedly with incrementing iDevNum values to enumerate all display devices. The method is not
    ///     thread-safe.
    /// </remarks>
    /// <param name="lpDevice">
    ///     The device name. If null, the function returns information for the display adapter(s) on the desktop. Otherwise,
    ///     specifies the device name of a display adapter or monitor to query.
    /// </param>
    /// <param name="iDevNum">
    ///     The index of the display device to query. Set to zero for the first device, one for the second,
    ///     and so on.
    /// </param>
    /// <param name="lpDisplayDevice">
    ///     A reference to a Win32 display device structure that receives information about the display device. The cb member of
    ///     the structure must be set to the size of the structure before calling this method.
    /// </param>
    /// <param name="dwFlags">Function-specific flags that control enumeration behavior. Typically set to zero.</param>
    /// <returns>true if the display device information was retrieved successfully; otherwise, false.</returns>
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumDisplayDevices(string? lpDevice, uint iDevNum, ref Win32DisplayDevice lpDisplayDevice, uint dwFlags);

    /// <summary>
    ///     Retrieves information about one of the graphics modes for a display device.
    /// </summary>
    /// <remarks>
    ///     This method is a P/Invoke signature for the native EnumDisplaySettings function in
    ///     user32.dll. The caller must initialize the Win32 device mode structure's size before calling. This method should be used
    ///     with care, as incorrect usage may cause unexpected results or system instability.
    /// </remarks>
    /// <param name="deviceName">The name of the display device. Use null to specify the current display device on the desktop.</param>
    /// <param name="modeNum">
    ///     The type of information to retrieve. Set to 0 to obtain the current settings, or use a positive integer to
    ///     enumerate supported display modes.
    /// </param>
    /// <param name="devMode">
    ///     A reference to a Win32 device mode structure that, on successful return, receives information about the display device's
    ///     settings.
    /// </param>
    /// <returns>true if the function succeeds and the Win32 device mode structure is filled with valid data; otherwise, false.</returns>
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumDisplaySettings(string? deviceName, int modeNum, ref Win32DeviceMode devMode);

    /// <summary>
    ///     Formats a refresh rate value as a string suitable for display, appending the unit if specified.
    /// </summary>
    /// <param name="refreshRate">The refresh rate in hertz to format. If null, no refresh rate is included in the output.</param>
    /// <returns>
    ///     A string containing the formatted refresh rate with the 'Hz' unit if a value is provided; otherwise, an empty
    ///     string.
    /// </returns>
    private static string FormatRefreshRate(int? refreshRate)
    {
        return refreshRate.HasValue
            ? $" @ {refreshRate.Value}Hz"
            : string.Empty;
    }

    /// <summary>
    ///     Retrieves the bounds of the display device identified by the specified name.
    /// </summary>
    /// <param name="displayName">The name of the display device. Specify null to retrieve the bounds of the primary display.</param>
    /// <returns>
    ///     A DisplayBounds structure representing the position and size of the display. Returns a DisplayBounds with all
    ///     values set to 0 if the display cannot be found.
    /// </returns>
    private static DisplayBounds GetDisplayBounds(string? displayName)
    {
        var currentMode = CreateEmptyDevMode();
        if (!EnumDisplaySettings(displayName, EnumCurrentSettings, ref currentMode))
        {
            return new DisplayBounds(0, 0, 0, 0);
        }

        return new DisplayBounds(currentMode.dmPosition.x,
                                 currentMode.dmPosition.y,
                                 (int)currentMode.dmPixelsWidth,
                                 (int)currentMode.dmPixelsHeight);
    }

    /// <summary>
    ///     Finds and returns the native Win32 device mode structure that matches the specified display resolution settings.
    /// </summary>
    /// <remarks>
    ///     This method searches all available display modes for the specified display and returns the
    ///     first mode that exactly matches the provided resolution parameters. Use this method to obtain a Win32 device mode suitable
    ///     for applying or validating display settings.
    /// </remarks>
    /// <param name="resolution">
    ///     The display resolution to match, including display name, width, height, refresh rate, and bits per pixel. Cannot
    ///     be null.
    /// </param>
    /// <returns>A Win32 device mode structure representing the native mode that matches the specified resolution settings.</returns>
    /// <exception cref="DisplaySettingsException">
    ///     Thrown if no native mode matching the specified resolution can be found for
    ///     the given display.
    /// </exception>
    private static Win32DeviceMode ResolveNativeMode(DisplayResolution resolution)
    {
        for (var modeIndex = 0; ; modeIndex++)
        {
            var nativeMode = CreateEmptyDevMode();
            if (!EnumDisplaySettings(resolution.DisplayName, modeIndex, ref nativeMode))
            {
                break;
            }

            if (((int)nativeMode.dmPixelsWidth != resolution.Width)
             || ((int)nativeMode.dmPixelsHeight != resolution.Height)
             || ((int)nativeMode.dmDisplayFrequency != resolution.RefreshRate)
             || ((int)nativeMode.dmBitsPerPel != resolution.BitsPerPixel))
            {
                continue;
            }

            nativeMode.dmFields = DmPixelsWidth | DmPixelsHeight | DmBitsPerPixel | DmDisplayFrequency;
            return nativeMode;
        }

        throw new DisplaySettingsException(
            $"Windows could not locate a native mode for {DisplayNames.Describe(resolution.DisplayName)} at {resolution.Width}x{resolution.Height} @ {resolution.RefreshRate}Hz.");
    }

    /// <summary>
    ///     Creates a new instance of the DisplayResolution class using the specified display name and native display mode
    ///     information.
    /// </summary>
    /// <param name="displayName">The name of the display device, or null if the display is unnamed.</param>
    /// <param name="nativeMode">A Win32 device mode structure containing the native display mode settings to use for the resolution.</param>
    /// <returns>
    ///     A DisplayResolution object representing the resolution and settings described by the provided display name and
    ///     native mode.
    /// </returns>
    private static DisplayResolution ToDisplayResolution(string? displayName, Win32DeviceMode nativeMode)
    {
        return new DisplayResolution(displayName,
                                     (int)nativeMode.dmPixelsWidth,
                                     (int)nativeMode.dmPixelsHeight,
                                     (int)nativeMode.dmDisplayFrequency,
                                     (int)nativeMode.dmBitsPerPel);
    }
}
