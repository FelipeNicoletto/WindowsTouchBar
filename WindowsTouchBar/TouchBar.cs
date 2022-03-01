using WindowsTouchBar.Device;
using WindowsTouchBar.Interop;

namespace WindowsTouchBar;

internal class TouchBar : IDisposable
{
    private IntPtr _fd;
    public readonly LockedFrameBuffer FrameBuffer;
    public readonly TouchReceiver TouchReceiver;

    public const int Width = 2008;
    public const int Height = 60;

    public TouchBar()
    {
        var instancePath = Locator.FindDfrDevice();
        if (instancePath == null)
        {
            throw new Exception("DFR Display Device not found");
        }

        _fd = NativeMethods.CreateFile(
            instancePath, FileAccess.Write, FileShare.None,
            IntPtr.Zero, FileMode.Open, FileOptions.None,
            IntPtr.Zero
        );

        FrameBuffer = new LockedFrameBuffer(_fd, Width, Height);
        TouchReceiver = new TouchReceiver(Width, Height);


        var timer = new System.Timers.Timer(1000 / 30);
        timer.AutoReset = true;
        timer.Elapsed += Timer_Elapsed;
        timer.Enabled = true;

    }

    private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        FrameBuffer.VSync();
    }

    private bool disposedValue = false;
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                
            }
            
            NativeMethods.CloseHandle(_fd);

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        // TODO: uncomment the following line if the finalizer is overridden above.
        GC.SuppressFinalize(this);
    }
}
