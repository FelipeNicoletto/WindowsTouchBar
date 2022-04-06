using SkiaSharp;
using SkiaSharp.Elements;
using WindowsTouchBar.Commands;

namespace WindowsTouchBar.Components;

internal class Button : Rectangle, ITouchComponent, IDisposable
{
    private bool _pressed;
    private Image? _image;
    private string? _imagePath;
    private Text? _text;
    private float _width;
    private SKColor _defaultColor;

    public event EventHandler? OnTouchDown;
    public event EventHandler? OnTouchUp;
    public ITouchCommand? Command;
    public SKColor DownFillColor = new SKColor(40, 40, 40);

    public bool TouchDownInterval { get; set; }
    public float Padding { get; set; } = 25;

    public Button(SKRect size) : base(size)
    {
        CornerRadius = new SKPoint(15, 15);
        FillColor = new SKColor(65, 65, 65);
        _width = size.Width;
    }

    private Text GetText() {

        if (_text is null)
        {
            _text = new Text(string.Empty);
            _text.ForeColor = SKColors.White;
            _text.FontSize = 25;
        }

        return _text;
    }

    private void UpdateTextPosition()
    {
        if (_text is not null)
        {
            if (_image is not null)
            {
                var x = (Width - ContentWidth) / 2;
                x += _image.Width + 15;

                _text.Center = new SKPoint(x + _text.Width / 2, Height / 2);
            }
            else
            {
                _text.Center = new SKPoint(Width / 2, Height / 2);
            }
        }
    }

    private void UpdateImagePosition()
    {
        if (_image is not null)
        {
            var factor = _image.Width / _image.Height;

            _image.Height = Height - 4;
            _image.Width = _image.Height * factor;

            if (_text is not null)
            {
                var x = (Width - ContentWidth) / 2;
                
                _image.Center = new SKPoint(x + _image.Width / 2, Height / 2);
            }
            else
            {   
                _image.Center = new SKPoint(Width / 2, Height / 2);
            }
        }
    }

    private void UpdateButton()
    {
        if (_width == 0)
        {
            Width = ContentWidth + Padding * 2;
        }

        UpdateTextPosition();
        UpdateImagePosition();
    }

    private float ContentWidth =>
        (_text?.Bounds.Width ?? 0) +
        (_image?.Width ?? 0) +
        (_text?.Bounds.Width > 0 && _image?.Width > 0 ? 15 : 0);

    public string? Text
    {
        get => _text?.Content;
        set
        {
            GetText().Content = value;
            UpdateButton();
        }
    }

    public string? FontFamily
    {
        get => _text?.FontFamily;
        set
        {
            GetText().FontFamily = value;
            UpdateButton();
        }
    }

    public float? FontSize
    {
        get => _text?.FontSize;
        set
        {
            if (value.HasValue)
            {
                GetText().FontSize = value.Value;
                UpdateButton();
            }
        }
    }

    public SKColor? ForeColor
    {
        get => _text?.ForeColor;
        set
        {
            if (value.HasValue)
            {
                GetText().ForeColor = value.Value;
                UpdateButton();
            }
        }
    }

    public string? ImagePath
    {
        get => _imagePath;
        set
        {
            _imagePath = value;

            if (_image is not null)
            {
                _image.Dispose();
                _image = null;
            }

            if (value is not null)
            {
                _image = new Image(SKBitmap.Decode(value))
                {
                    AlignMode = Image.ImageAlignMode.CenterMiddle,
                    SizeMode = Image.ImageSizeMode.Contain
                };
            }

            UpdateButton();
        }
    }

    public override void Draw(SKCanvas canvas)
    {
        if (Visible)
        {
            SuspendDrawBeforeAfter();

            DrawBefore(canvas);

            base.Draw(canvas);

            if (_text?.Content is not null ||
                _image is not null)
            {
                canvas.Save();
                canvas.Translate(X, Y);

                if (_image is not null)
                {
                    _image.Draw(canvas);
                }

                if (_text?.Content is not null)
                {
                    _text.Draw(canvas);
                }

                canvas.Restore();
            }

            DrawAfter(canvas);
        }
    }

    public virtual void TouchDown(SKPoint _)
    {
        if (_pressed)
            return;

        _pressed = true;
        _defaultColor = FillColor;
        FillColor = DownFillColor;

        if (TouchDownInterval)
        {
            Task.Run(() =>
            {
                do
                {
                    Command?.TouchDown();
                    OnTouchDown?.Invoke(this, EventArgs.Empty);
                    Thread.Sleep(50);
                }
                while (_pressed);
            });
        }
        else
        {
            Command?.TouchDown();
            OnTouchDown?.Invoke(this, EventArgs.Empty);
        }
    }

    public virtual void TouchMove(SKPoint point)
    {
        TouchDown(point);
    }

    public virtual void TouchUp(SKPoint _)
    {
        if (_pressed)
        {
            _pressed = false;
            FillColor = _defaultColor;
        }

        Command?.TouchUp();
        OnTouchUp?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        _image?.Dispose();
    }
}
