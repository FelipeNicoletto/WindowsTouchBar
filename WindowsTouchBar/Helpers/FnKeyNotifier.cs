using System.Runtime.InteropServices;
using WindowsTouchBar.Device;
using WindowsTouchBar.Events;
using WindowsTouchBar.Interop;

namespace WindowsTouchBar.Helpers;

public class FnKeyNotifier
{
    private IntPtr _fd;

    public event Action<FnKeyEventArgs>? Event;

    public FnKeyNotifier(string instancePath)
    {
        _fd = NativeMethods.CreateFile(
            instancePath, FileAccess.ReadWrite, FileShare.None,
            IntPtr.Zero, FileMode.Open, FileOptions.None,
            IntPtr.Zero
        );

        if (_fd == IntPtr.Zero)
        {
            throw new Exception("Error: " + Marshal.GetLastWin32Error());
        }

        ThreadPool.UnsafeQueueUserWorkItem(_ => PollNextFnStatus(), null);
    }

    public void PollNextFnStatus()
    {
        while (true)
        {
            var ioCtlResult = DfrHostIo.GetNextFnKeyStatus(_fd, out bool pressed);
            Task.Run(() => ProcessEvent(pressed));

            if (!ioCtlResult)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }
    }

    private void ProcessEvent(bool pressed)
    {
        Event?.Invoke(new FnKeyEventArgs(pressed));
    }
}
