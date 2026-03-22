using System.Runtime.InteropServices;

namespace ResolutionSwitcher.Infrastructure.Display.Interop;

/// <summary>
///     Defines the device mode structure used to specify device settings for display devices and printers in
///     Windows API calls.
/// </summary>
/// <remarks>
///     This structure is primarily used when interacting with native Windows functions that require
///     device configuration information, such as changing display settings or querying printer capabilities. The fields
///     correspond to various device parameters, including device name, display orientation, resolution, color settings,
///     and more. This structure must be initialized and populated according to the requirements of the specific Windows
///     API being called. The layout and field usage are defined by the Windows SDK documentation for the native DEVMODE
///     structure.
/// </remarks>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct Win32DeviceMode
{
    private const int CchDeviceName = 32;
    private const int CchFormName = 32;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CchDeviceName)]
    public string dmDeviceName;

    public short dmSpecVersion;
    public short dmDriverVersion;
    public short dmSize;
    public short dmDriverExtra;
    public uint dmFields;
    public Win32Point dmPosition;
    public uint dmDisplayOrientation;
    public uint dmDisplayFixedOutput;
    public short dmColor;
    public short dmDuplex;
    public short dmYResolution;
    public short dmTTOption;
    public short dmCollate;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CchFormName)]
    public string dmFormName;

    public short dmLogPixels;
    public uint dmBitsPerPel;
    public uint dmPixelsWidth;
    public uint dmPixelsHeight;
    public uint dmDisplayFlags;
    public uint dmDisplayFrequency;
    public uint dmICMMethod;
    public uint dmICMIntent;
    public uint dmMediaType;
    public uint dmDitherType;
    public uint dmReserved1;
    public uint dmReserved2;
    public uint dmPanningWidth;
    public uint dmPanningHeight;
}