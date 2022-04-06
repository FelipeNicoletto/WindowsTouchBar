namespace WindowsTouchBar.Events;

public class FnKeyEventArgs : EventArgs
{
    public readonly bool Pressed;

    public FnKeyEventArgs(bool pressed)
    {
        Pressed = pressed;
    }
}
