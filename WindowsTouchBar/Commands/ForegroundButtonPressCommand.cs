using WindowsTouchBar.Helpers;

namespace WindowsTouchBar.Commands;

internal class ForegroundButtonPressCommand : ITouchCommand
{
    public IntPtr Handle { get; set; }

    public ForegroundButtonPressCommand(IntPtr handle)
    {
        Handle = handle;
    }

    public void TouchDown()
    {
        ForegroundTracker.PressForegroundButton(Handle);
    }

    public void TouchUp()
    {
    }
}
