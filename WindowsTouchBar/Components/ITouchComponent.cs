using SkiaSharp;

namespace WindowsTouchBar.Components;

internal interface ITouchComponent : IComponent
{
    event EventHandler OnTouchDown;
    event EventHandler OnTouchUp;
    void TouchDown(SKPoint point);
    void TouchMove(SKPoint point);
    void TouchUp(SKPoint point);
}
