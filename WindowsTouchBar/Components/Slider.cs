using SkiaSharp;
using SkiaSharp.Elements;

namespace WindowsTouchBar.Components;

internal class Slider : Rectangle, ITouchComponent
{
    public event EventHandler? OnTouchDown;
    public event EventHandler? OnTouchUp;
    public event EventHandler<int>? OnValueChanged;

    private Rectangle _slideRect;
    private Rectangle _fillLine;
    private Rectangle _trackLine;
    private SKPoint? _startPoint;
    private float _startLeft = 0;

    public Slider(SKRect size) : base(size)
    {
        BorderWidth = 0;

        _slideRect = new Rectangle(SKRect.Create(size.Height, size.Height));
        _slideRect.CornerRadius = new SKPoint(15, 15);
        _slideRect.FillColor = SKColors.White;
        _slideRect.BorderWidth = 0;

        _trackLine = new Rectangle(SKRect.Create(0, 0, size.Width, 7));
        _trackLine.CornerRadius = new SKPoint(3, 3);
        _trackLine.FillColor = SKColors.LightGray;
        _trackLine.Center = new SKPoint(_trackLine.Center.X, size.Height / 2);
        _trackLine.BorderWidth = 0;

        _fillLine = new Rectangle(SKRect.Create(0, 0, _slideRect.Center.X, 7));
        _fillLine.CornerRadius = new SKPoint(3, 3);
        _fillLine.FillColor = new SKColor(4, 129, 220);
        _fillLine.Center = new SKPoint(_fillLine.Center.X, size.Height / 2);
        _fillLine.BorderWidth = 0;
    }

    private int _value;
    public int Value
    {
        get => _value;
        set
        {
            _value = Math.Min(Math.Max(value, 0), 100);

            var halfRect = _slideRect.Width / 2;

            _slideRect.X = halfRect + (Width - _slideRect.Width) / 100 * _value - halfRect;
            UpdateFillRect();
        }
    }

    private void UpdateFillRect()
    {
        _fillLine.Width = _slideRect.Center.X;
    }

    public override void Draw(SKCanvas canvas)
    {
        if (Visible)
        {
            SuspendDrawBeforeAfter();

            DrawBefore(canvas);

            //base.Draw(canvas);

            canvas.Save();
            canvas.Translate(X, Y);

            _trackLine.Draw(canvas);
            _fillLine.Draw(canvas);
            _slideRect.Draw(canvas);

            canvas.Restore();

            DrawAfter(canvas);
        }
    }

    public void TouchDown(SKPoint point)
    {
        if (!_startPoint.HasValue && _slideRect.IsPointInside(point))
        {
            _startPoint = point;
            _startLeft = _slideRect.X;
        }

        OnTouchDown?.Invoke(this, EventArgs.Empty);
    }

    public void TouchMove(SKPoint point)
    {
        if (_startPoint.HasValue)
        {
            _slideRect.X = Math.Min(Math.Max(_startLeft + point.X - _startPoint.Value.X, 0), Width - _slideRect.Width);
            UpdateFillRect();

            var halfRect = _slideRect.Width / 2;

            _value = (int)((_slideRect.Center.X - halfRect) * 100 / (Width - _slideRect.Width));

            OnValueChanged?.Invoke(this, _value);
        }
    }

    public void TouchUp(SKPoint point)
    {
        _startPoint = null;
        OnTouchUp?.Invoke(this, EventArgs.Empty);
    }
}
