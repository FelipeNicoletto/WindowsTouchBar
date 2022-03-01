using WindowsTouchBar;
using WindowsTouchBar.Components;
using SkiaSharp;
using SkiaSharp.Elements;

var touchBar = new TouchBar();

var surface = SKSurface.Create(new SKImageInfo(TouchBar.Width, TouchBar.Height, SKColorType.Rgba8888, SKAlphaType.Opaque), touchBar.FrameBuffer.Address);

var canvas = surface.Canvas;

var elementsController = new ElementsController();
elementsController.BackgroundColor = SKColors.Black;

elementsController.OnInvalidate += delegate (object? sender, EventArgs e)
{
    elementsController.Clear(canvas);
    elementsController.Draw(canvas);
    //touchBar.FrameBuffer.VSync();
};


var rect = new Button(SKRect.Create(0, 2, 50, 56));
//rect.FillColor = SKColors.Red;

elementsController.Elements.Add(rect);




//using var codec = SKCodec.Create(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "download.png"));
//using var bmp = SKBitmap.Decode(codec, new SKImageInfo(codec.Info.Width, codec.Info.Height, SKImageInfo.PlatformColorType, SKAlphaType.Opaque, SKColorSpace.CreateSrgb()));
//var paint = new SKPaint();
//canvas.Clear(SKColors.Black);
//paint.Color = SKColors.Orange;
//canvas.DrawRect(0, 0, 100, 60, paint);


//canvas.DrawBitmap(bmp, SKRect.Create(0, 0, bmp.Width, bmp.Height), SKRect.Create(0, 0, 60, 60));

Element currElement = null;

touchBar.TouchReceiver.Event += TouchReceiver_Event;


void TouchReceiver_Event(WindowsTouchBar.Events.TouchEventArgs evt)
{
    if (evt.TouchEventType == WindowsTouchBar.Events.TouchEventType.TouchDown ||
        evt.TouchEventType == WindowsTouchBar.Events.TouchEventType.TouchMove)
    {
        var element = elementsController.Elements.GetElementAtPoint(new SKPoint(evt.X, evt.Y));

        if (element != null)
        {
            currElement = element;

            ((Button)element).TouchDown();
        }
        else if (currElement != null)
        {
            ((Button)currElement).TouchUp();
        }
    }
    else if (currElement != null)
    {
        ((Button)currElement).TouchUp();
    }

            //canvas.DrawRect(evt.X, evt.Y, 10, 10, paint);


    //touchBar.FrameBuffer.VSync();


}

while (true) { }