using SkiaSharp;
using WindowsInput.Native;
using WindowsTouchBar.Commands;
using WindowsTouchBar.Components;
using WindowsTouchBar.Interop;
using WindowsTouchBar.Models;

namespace WindowsTouchBar.Views;

internal class MediaFunctionsView : TouchBarView
{
    public override ViewType Type => ViewType.Media;

    public MediaFunctionsView(int width, int height) : base(width, height)
    {
        CreateButtons();
        Update();
    }

    private Button CreateButton(VirtualKeyCode? keyCode, string text, bool touchDownInterval = false)
    {
        var btnSize = SKRect.Create(150, Height - 4);
        var btn = new Button(btnSize);
        btn.Text = text;

        if (keyCode.HasValue)
        {
            btn.Command = new KeyPressCommand(keyCode);
        }

        btn.FontFamily = "Segoe MDL2 Assets";
        btn.FontSize = 30;
        btn.TouchDownInterval = touchDownInterval;
        Components.Add(btn);

        return btn;
    }

    private void CreateButtons()
    {
        var spacer = new Spacer(true);
        Components.Add(spacer);

        // BrightlessDown
        var btn = CreateButton(null, "\uE706", true);
        btn.FontSize = 25;
        btn.OnTouchDown += BrightlessDown;

        // BrightlessUp
        btn = CreateButton(null, "\uE706", true);
        btn.OnTouchDown += BrightlessUp;

        spacer = new Spacer(true);
        Components.Add(spacer);

        // KeyboardDown
        CreateButton(null, "\uED3A", true);
        // KeyboardUp
        CreateButton(null, "\uED39", true);

        spacer = new Spacer(true);
        Components.Add(spacer);

        CreateButton(VirtualKeyCode.MEDIA_PREV_TRACK, "\uE100");
        CreateButton(VirtualKeyCode.MEDIA_PLAY_PAUSE, "\uE102\uE769");
        CreateButton(VirtualKeyCode.MEDIA_NEXT_TRACK, "\uE101");

        spacer = new Spacer(true);
        Components.Add(spacer);

        CreateButton(VirtualKeyCode.VOLUME_MUTE, "\uE74F");
        CreateButton(VirtualKeyCode.VOLUME_DOWN, "\uE993", true);
        CreateButton(VirtualKeyCode.VOLUME_UP, "\uE995", true);

        spacer = new Spacer(true);
        Components.Add(spacer);
    }

    private void BrightlessUp(object? sender, EventArgs args)
    {
        var b = BrightlessAPI.GetCurrentBrightness();

        BrightlessAPI.SetCurrentBrightness(Math.Min(b + 2, 100));
    }

    private void BrightlessDown(object? sender, EventArgs args)
    {
        var b = BrightlessAPI.GetCurrentBrightness();

        BrightlessAPI.SetCurrentBrightness(Math.Max(b - 2, 0));
    }
}
