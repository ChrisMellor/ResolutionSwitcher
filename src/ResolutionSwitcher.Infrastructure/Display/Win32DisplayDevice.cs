using System.Runtime.InteropServices;

namespace ResolutionSwitcher.Infrastructure.Display;

/// <summary>
///     Contains information about a display device retrieved from the system, including device name, description,
///     state, and identifiers.
/// </summary>
/// <remarks>
///     This structure is typically used with native Windows API calls to enumerate display devices
///     and their properties. Field values correspond to the information returned by the operating system for each
///     display adapter or monitor. The structure layout and field sizes must match the expectations of the Windows API
///     for correct interoperation.
/// </remarks>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct Win32DisplayDevice
{
    private const int CchDeviceName = 32;
    private const int CchDeviceString = 128;
    private const int CchDeviceId = 128;
    private const int CchDeviceKey = 128;

    public int cb;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CchDeviceName)]
    public string DeviceName;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CchDeviceString)]
    public string DeviceString;

    public uint StateFlags;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CchDeviceId)]
    public string DeviceID;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CchDeviceKey)]
    public string DeviceKey;
}
