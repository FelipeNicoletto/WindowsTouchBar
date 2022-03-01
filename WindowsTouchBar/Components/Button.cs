using SkiaSharp;
using SkiaSharp.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsTouchBar.Components;

internal class Button : Rectangle
{
    private Text _text = new Text("Textop teste");

    public Button(SKRect rect): base(rect)
    {
        CornerRadius = new SKPoint(10, 10);
        FillColor = SKColor.Parse("#FF292827");

        _text.ForeColor = SKColors.White;
        _text.FontSize = 20;
        Width = _text.Bounds.Width + 20;

        _text.Center = new SKPoint(Width / 2, Height / 2);

    }

    public void TouchDown()
    {
        FillColor = SKColors.Red;
    }

    public void TouchUp()
    {
        FillColor = SKColor.Parse("#FF292827");
    }

    public override void Draw(SKCanvas canvas)
    {
        SuspendDrawBeforeAfter();
        
        DrawBefore(canvas);

        base.Draw(canvas);

        canvas.Save();
        canvas.Translate(X, Y);

        _text.Draw(canvas);

        canvas.Restore();


        DrawAfter(canvas);

    }


}
