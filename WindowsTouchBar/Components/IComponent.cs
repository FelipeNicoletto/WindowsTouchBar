using SkiaSharp;

namespace WindowsTouchBar.Components;

internal interface IComponent
{
    float X { get; set; }
    float Y { get; set; }
    float Width { get; }
    float Height { get; }
    void Draw(SKCanvas canvas);
}
