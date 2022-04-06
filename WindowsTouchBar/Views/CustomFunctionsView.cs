using SkiaSharp;
using WindowsTouchBar.Commands;
using WindowsTouchBar.Components;
using WindowsTouchBar.Models;

namespace WindowsTouchBar.Views;

internal class CustomFunctionsView : TouchBarView
{
    public override ViewType Type => ViewType.Custom;

    public CustomFunctionsView(int width, int height) : base(width, height)
    {
    }

    public override void ConfigureView(Settings.View? view)
    {
        base.ConfigureView(view);

        Components.Clear();
        
        if (view?.Components != null)
        {
            ConfigureComponents(view.Components);
        }        

        Update();
    }

    private void ConfigureComponents(IEnumerable<Settings.Component> components)
    {
        foreach (var c in components)
        {
            IComponent? component = null;
            switch (c.Type)
            {
                case Settings.ComponentType.Button:
                    component = BuildButton(c);
                    break;
                case Settings.ComponentType.Spacer:
                    component = BuildSpacer(c);
                    break;
            }

            if (component != null)
            {
                Components.Add(component);
            }
        }
    }

    private Button BuildButton(Settings.Component component)
    {
        var btn = new Button(SKRect.Create(component.Width ?? 0, Height - 4));

        if (!string.IsNullOrWhiteSpace(component.Image))
        {
            btn.ImagePath = component.Image;
        }
        
        if (!string.IsNullOrWhiteSpace(component.Icon) || !string.IsNullOrWhiteSpace(component.Title))
        {
            btn.Text = component.Icon ?? component.Title;

            if (!string.IsNullOrWhiteSpace(component.TextColor) && SKColor.TryParse(component.TextColor, out var textColor))
            {
                btn.ForeColor = textColor;
            }

            if (!string.IsNullOrWhiteSpace(component.Icon))
            {
                btn.FontFamily = "Segoe MDL2 Assets";
            }
        }

        if (!string.IsNullOrWhiteSpace(component.Color) && SKColor.TryParse(component.Color, out var fillColor))
        {
            btn.FillColor = fillColor;
        }

        btn.Command = new KeyPressCommand
        {
            KeyCodes = component.KeyCodes,
            ModifierKeyCodes = component.ModifierKeyCodes
        };

        return btn;
    }

    private Spacer BuildSpacer(Settings.Component component)
    {
        return component.Width > 0 ? new Spacer(component.Width.Value) : new Spacer(true);
    }
}
