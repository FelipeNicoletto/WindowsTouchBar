namespace WindowsTouchBar.Events;

internal class TouchEventArgs : EventArgs
{
    public int X { get; }
    public int Y { get; }

    public TouchEventType TouchEventType { get; }

    public TouchEventArgs(int x, int y, TouchEventType touchEventType)
    {
        X = x;
        Y = y;
        TouchEventType = touchEventType;
    }
}

public enum TouchEventType
{
    TouchDown,
    TouchUp,
    TouchMove
}
