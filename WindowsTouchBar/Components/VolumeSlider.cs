using SkiaSharp;
using SkiaSharp.Elements;
using WindowsInput.Native;
using WindowsTouchBar.Commands;

namespace WindowsTouchBar.Components;

internal class VolumeSlider : Rectangle, ITouchComponent, IDisposable
{
    public event EventHandler? OnTouchDown;
    public event EventHandler? OnTouchUp;
    public event EventHandler<int>? OnValueChanged;

    private Button _button1;
    private Button _button2;
    private Slider _slider;
    private bool _disableButtons;
    
    public VolumeSlider(SKRect size) : base(size)
    {
        CornerRadius = new SKPoint(15, 15);
        FillColor = new SKColor(65, 65, 65);
        BorderWidth = 0;

        _button1 = new Button(SKRect.Create(60, size.Height));
        _button1.FontFamily = "Segoe MDL2 Assets";
        _button1.FontSize = 30;
        _button1.BorderWidth = 0;
        _button1.Text = "\uE993";
        _button1.Command = new KeyPressCommand(VirtualKeyCode.VOLUME_DOWN);
        _button1.TouchDownInterval = true;

        _button2 = new Button(SKRect.Create(60, size.Height));
        _button2.FontFamily = "Segoe MDL2 Assets";
        _button2.FontSize = 30;
        _button2.BorderWidth = 0;
        _button2.Text = "\uE995";
        _button2.Command = new KeyPressCommand(VirtualKeyCode.VOLUME_UP);
        _button2.TouchDownInterval = true;

        _button1.Center = new SKPoint(_button1.Width / 2 + 20, size.Height / 2);
        _button2.Center = new SKPoint(size.Width - _button2.Width / 2 - 20, size.Height / 2);

        _slider = new Slider(SKRect.Create(size.Width - 200, size.Height));
        _slider.Center = new SKPoint(size.Width / 2, size.Height / 2);
        _slider.OnValueChanged += Slider_OnValueChanged;
    }

    private void Slider_OnValueChanged(object? sender, int e) => OnValueChanged?.Invoke(this, e);

    public int Value
    {
        get => _slider.Value;
        set
        {
            _slider.Value = value;
        }
    }

    public override void Draw(SKCanvas canvas)
    {
        if (Visible)
        {
            SuspendDrawBeforeAfter();

            DrawBefore(canvas);

            base.Draw(canvas);

            canvas.Save();
            canvas.Translate(X, Y);

            _button1.Draw(canvas);
            _slider.Draw(canvas);
            _button2.Draw(canvas);

            canvas.Restore();

            DrawAfter(canvas);
        }
    }

    public void TouchDown(SKPoint point)
    {
        if (_slider.IsPointInside(point))
        {
            _button1.TouchUp(point);
            _button2.TouchUp(point);

            var elementPoint = new SKPoint(point.X - _slider.X, point.Y - _slider.Y);
            _slider.TouchDown(elementPoint);
            _disableButtons = true;
        }
        else if (_button1.IsPointInside(point))
        {
            _button1.TouchDown(point);
        }
        else if (_button2.IsPointInside(point))
        {
            _button2.TouchDown(point);
        }
    }

    public void TouchMove(SKPoint point)
    {
        if (_slider.IsPointInside(point))
        {
            _button1.TouchUp(point);
            _button2.TouchUp(point);

            var elementPoint = new SKPoint(point.X - _slider.X, point.Y - _slider.Y);
            _slider.TouchMove(elementPoint);
        }
        else if (!_disableButtons && _button1.IsPointInside(point))
        {
            _button1.TouchMove(point);
        }
        else if (!_disableButtons && _button2.IsPointInside(point))
        {
            _button2.TouchMove(point);
        }
    }

    public void TouchUp(SKPoint point)
    {
        var elementPoint = new SKPoint(point.X - _slider.X, point.Y - _slider.Y);
        _slider.TouchUp(elementPoint);
        _disableButtons = false;

        _button1.TouchUp(point);
        _button2.TouchUp(point);
    }

    public void Dispose()
    {
        _slider.OnValueChanged -= Slider_OnValueChanged;
    }
}
