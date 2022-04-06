using WindowsTouchBar.Device;
using WindowsTouchBar.Interop;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

namespace WindowsTouchBar;

unsafe class LockedFrameBuffer : IDisposable
{
    private readonly IntPtr _deviceHandle;
    private int _bufferSize;
    private readonly int _width;
    private readonly int _height;

    public LockedFrameBuffer(IntPtr instance, int width, int height)
    {
        if (!Sse2.IsSupported || !Ssse3.IsSupported || !Avx2.IsSupported)
        {
            throw new Exception("Outdated CPU detected");
        }

        _deviceHandle = instance;
        _width = width;
        _height = height;

        _bufferSize = RowBytes * _height;
        Address = Marshal.AllocHGlobal(_bufferSize);
    }

    public IntPtr Address { get; private set; }

    public int RowBytes => _width * 4;

    public unsafe void VSync()
    {
        var requestSize = _width * _height * 3 + Marshal.SizeOf(typeof(DFR_HOSTIO_UPDATE_FRAMEBUFFER_HEADER));
        var RequestMemory = Marshal.AllocHGlobal(requestSize);
        if (RequestMemory == IntPtr.Zero)
        {
            throw new Exception("Failed to allocate memory for FrameBuffer");
        }

        byte* pRequest = (byte*)RequestMemory.ToPointer();
        byte* pFbContent = (byte*)Address.ToPointer();

        NativeMethods.ZeroMemory(pRequest, requestSize);
        UnmanagedMemoryStream requestStream = new UnmanagedMemoryStream(pRequest, requestSize, requestSize, FileAccess.Write);
        using (var binaryWriter = new BinaryWriter(requestStream))
        {
            binaryWriter.Write((ushort)0);
            binaryWriter.Write((ushort)0);
            binaryWriter.Write((ushort)_width);
            binaryWriter.Write((ushort)_height);
            binaryWriter.Write(DfrHostIo.DFR_FRAMEBUFFER_FORMAT);
            binaryWriter.Write((uint)0);
            binaryWriter.Flush();

            for (var w = 0; w < _width; w++)
            {
                for (var h = _height - 1; h >= 0; h--)
                {
                    byte* p = pFbContent + (_width * h + w) * 4;
                    binaryWriter.Write(*p);
                    binaryWriter.Write(*(p + 1));
                    binaryWriter.Write(*(p + 2));
                }
            }

            binaryWriter.Flush();
            NativeMethods.DeviceIoControl(
                _deviceHandle,
                DfrHostIo.IOCTL_DFR_UPDATE_FRAMEBUFFER,
                RequestMemory,
                requestSize,
                IntPtr.Zero,
                0,
                IntPtr.Zero,
                IntPtr.Zero
            );
        }

        Marshal.FreeHGlobal(RequestMemory);
    }

    public void Dispose()
    {
        VSync();

        Marshal.FreeHGlobal(Address);
        Address = IntPtr.Zero;
    }
}
