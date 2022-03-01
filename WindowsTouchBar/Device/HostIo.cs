using System.Runtime.InteropServices;
using WindowsTouchBar.Interop;

namespace WindowsTouchBar.Device;

static class DfrHostIo
{
    private static uint ControlCode(uint deviceType, uint function, uint method, uint access) =>
        ((deviceType) << 16) | ((access) << 14) | ((function) << 2) | (method);

    private const uint FILE_DEVICE_DFR = 0x8086;

    private const uint FUNCTION_UPDATE_FRAMEBUFFER = 0x801;
    private const uint FUNCTION_CLEAR_FRAMEBUFFER = 0x802;
    private const uint FUNCTION_QUERY_DEVICE = 0x803;
    private const uint FUNCTION_QUERY_FN_KEY = 0x804;

    private const uint FILE_READ_DATA = 0x0001;         // file & pipe
    private const uint FILE_LIST_DIRECTORY = 0x0001;    // directory

    private const uint FILE_WRITE_DATA = 0x0002;        // file & pipe
    private const uint FILE_ADD_FILE = 0x0002;          // directory

    private const uint METHOD_BUFFERED = 0;
    private const uint METHOD_IN_DIRECT = 1;
    private const uint METHOD_OUT_DIRECT = 2;
    private const uint METHOD_NEITHER = 3;

    public static uint IOCTL_DFR_UPDATE_FRAMEBUFFER => 
        ControlCode(FILE_DEVICE_DFR, FUNCTION_UPDATE_FRAMEBUFFER, METHOD_BUFFERED, FILE_WRITE_DATA);
    public static uint IOCTL_DFR_CLEAR_FRAMEBUFFER =>
        ControlCode(FILE_DEVICE_DFR, FUNCTION_CLEAR_FRAMEBUFFER, METHOD_BUFFERED, FILE_WRITE_DATA);
    public static uint IOCTL_DFR_GET_FN_STATUS =>
        ControlCode(FILE_DEVICE_DFR, FUNCTION_QUERY_FN_KEY, METHOD_BUFFERED, FILE_READ_DATA);

    public const uint DFR_FRAMEBUFFER_FORMAT = 0x52474241;

    public static bool ClearDfrFrameBuffer(IntPtr deviceHandle)
    {
        return NativeMethods.DeviceIoControl(
            deviceHandle,
            IOCTL_DFR_CLEAR_FRAMEBUFFER,
            IntPtr.Zero,
            0,
            IntPtr.Zero,
            0,
            IntPtr.Zero,
            IntPtr.Zero
        );
    }

    public static bool GetNextFnKeyStatus(IntPtr deviceHandle, out bool pressed)
    {
        pressed = false;

        DFR_HOSTIO_FN_KEY_STATUS status;
        status.FnKeyPressed = 0;

        int size = Marshal.SizeOf(status);
        IntPtr statusPtr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(status, statusPtr, true);

        var ioctlStatus = NativeMethods.DeviceIoControl(
            deviceHandle,
            IOCTL_DFR_GET_FN_STATUS,
            IntPtr.Zero,
            0,
            statusPtr,
            size,
            IntPtr.Zero,
            IntPtr.Zero
        );

        if (ioctlStatus)
        {
            status = Marshal.PtrToStructure<DFR_HOSTIO_FN_KEY_STATUS>(statusPtr);
            pressed = status.FnKeyPressed != 0;
        }

        Marshal.FreeHGlobal(statusPtr);
        return ioctlStatus;
    }
}

[StructLayout(LayoutKind.Sequential)]
struct DFR_HOSTIO_UPDATE_FRAMEBUFFER_HEADER
{
    public ushort BeginX;
    public ushort BeginY;
    public ushort Width;
    public ushort Height;
    public uint FrameBufferPixelFormat;
    public uint RequireVertFlip;
}

[StructLayout(LayoutKind.Sequential)]
struct DFR_HOSTIO_FN_KEY_STATUS
{
    public byte FnKeyPressed;
}
