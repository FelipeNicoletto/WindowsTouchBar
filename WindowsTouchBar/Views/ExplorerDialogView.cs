using SkiaSharp;
using WindowsTouchBar.Commands;
using WindowsTouchBar.Components;
using WindowsTouchBar.Models;

namespace WindowsTouchBar.Views;

internal class ExplorerDialogView : TouchBarView
{
    public override ViewType Type => ViewType.Dialog;

    public ExplorerDialogView(int width, int height) : base(width, height)
    {
    }

    public void CreateDialog(IEnumerable<ForegroundButton> buttons)
    {
        Components.Clear();

        Components.Add(new Spacer(true));
        Components.Add(new Spacer(10));

        foreach (var b in buttons)
        {
            Components.Add(BuildButton(b));
            Components.Add(new Spacer(10));
        }

        Components.Add(new Spacer(true));

        Update();
    }

    private Button BuildButton(ForegroundButton button)
    {
        var btn = new Button(SKRect.Create(350, Height - 4));
        btn.Text = button.Title?.TrimStart('&') ?? string.Empty;

        btn.Command = new ForegroundButtonPressCommand(button.Handle);
        return btn;
    }
}
