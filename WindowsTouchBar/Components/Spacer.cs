using SkiaSharp;

namespace WindowsTouchBar.Components;

internal class Spacer : IComponent
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height => 0;
    public readonly bool AutoSize;

    public Spacer(int width)
    {
        Width = width;
    }

    public Spacer(bool autoSize)
    {
        AutoSize = autoSize;
    }

    public void Draw(SKCanvas canvas)
    {

    }
}
