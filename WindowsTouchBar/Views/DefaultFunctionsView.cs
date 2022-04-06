using SkiaSharp;
using WindowsInput.Native;
using WindowsTouchBar.Commands;
using WindowsTouchBar.Components;
using WindowsTouchBar.Models;

namespace WindowsTouchBar.Views;

internal class DefaultFunctionsView : TouchBarView
{
    public override ViewType Type => ViewType.Function;

    public DefaultFunctionsView(int width, int height) : base(width, height)
    {
        CreateButtons();
        Update();
    }

    private void CreateButtons()
    {
        var keyCodes = new[]
        {
            VirtualKeyCode.F1,
            VirtualKeyCode.F2,
            VirtualKeyCode.F3,
            VirtualKeyCode.F4,
            VirtualKeyCode.F5,
            VirtualKeyCode.F6,
            VirtualKeyCode.F7,
            VirtualKeyCode.F8,
            VirtualKeyCode.F9,
            VirtualKeyCode.F10,
            VirtualKeyCode.F11,
            VirtualKeyCode.F12
        };

        var btnW = (Width - ComponentsGap * (keyCodes.Length - 1)) / keyCodes.Length;
        var btnSize = SKRect.Create(btnW, Height - 4);

        Components.AddRange(keyCodes.Select(keyCode =>
        {
            var btn = new Button(btnSize);
            btn.Text = keyCode.ToString();
            btn.Command = new KeyPressCommand(keyCode);

            return btn;
        }));
    }
}
