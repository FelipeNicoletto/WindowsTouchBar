using SkiaSharp;
using SkiaSharp.Elements;
using WindowsTouchBar.Components;
using WindowsTouchBar.Events;
using WindowsTouchBar.Models;

namespace WindowsTouchBar.Views;

internal abstract class TouchBarView : ElementsController
{
    public abstract ViewType Type { get; }
    public readonly List<IComponent> Components;
    public readonly int Width;
    public readonly int Height;

    public float ComponentsGap = 10;

    public TouchBarView(int width, int height)
    {
        Width = width;
        Height = height;
        Components = new List<IComponent>();
        BackgroundColor = SKColors.Black;
    }

    public virtual void ConfigureView(Settings.View? view)
    {
        if (!string.IsNullOrWhiteSpace(view?.BackgroundColor) && SKColor.TryParse(view.BackgroundColor, out var backgroundColor))
        {
            BackgroundColor = backgroundColor;
        }
    }

    protected void Update()
    {
        if (Components == null)
            return;

        SuspendLayout();

        Elements.Clear();
        Elements.AddRange(Components.OfType<Element>().ToArray());

        float autoSizeSpacerSize = 0;
        var autoSizeSpacers = Components.Where(c => c is Spacer spacer && spacer.AutoSize);

        if (autoSizeSpacers.Any())
        {
            float totalComponentsWidth = 0;

            for (var i = 0; i < Components.Count; i++)
            {
                var c = Components[i];

                if (c is not Spacer)
                {
                    totalComponentsWidth += c.Width + (i == Components.Count - 1 ? 0 : ComponentsGap);
                }
                else if (c is Spacer spacer)
                {
                    totalComponentsWidth = Math.Max(totalComponentsWidth - ComponentsGap, 0);

                    if (!spacer.AutoSize)
                    {
                        totalComponentsWidth += c.Width;
                    }
                }
            }

            if (totalComponentsWidth < Width)
            {
                autoSizeSpacerSize = (Width - totalComponentsWidth) / autoSizeSpacers.Count();
            }
        }

        float x = 0;
        var index = 0;

        foreach (var component in Components)
        {
            if (component is Spacer spacer)
            {
                x = Math.Max(x - ComponentsGap, 0);
                if (spacer.AutoSize)
                {
                    x += autoSizeSpacerSize;
                }
                else
                {
                    x += spacer.Width;
                }
            }
            else
            {
                var y = (Height - (int)component.Height) / 2;

                component.X = x;
                component.Y = y;

                x += component.Width + ComponentsGap;
            }

            index++;
        }

        ResumeLayout();
    }

    public void ReceiveTouch(TouchEventArgs args)
    {
        SuspendLayout();

        Element? element = null;
        if (args.TouchEventType == TouchEventType.TouchDown ||
            args.TouchEventType == TouchEventType.TouchMove)
        {
            element = Elements.GetElementAtPoint(new SKPoint(args.X, args.Y));

            if (element is ITouchComponent touchComponent)
            {
                var elementPoint = new SKPoint(args.X - touchComponent.X, args.Y - touchComponent.Y);
                switch (args.TouchEventType)
                {
                    case TouchEventType.TouchDown:
                        touchComponent.TouchDown(elementPoint);
                        break;
                    case TouchEventType.TouchMove:
                        touchComponent.TouchMove(elementPoint);
                        break;
                }   
            }
        }

        foreach (var e in Elements.OfType<ITouchComponent>())
        {
            if (e != element)
            {
                e.TouchUp(new SKPoint(args.X - e.X, args.Y - e.Y));
            }
        }

        ResumeLayout();
        Invalidate();
    }
}
