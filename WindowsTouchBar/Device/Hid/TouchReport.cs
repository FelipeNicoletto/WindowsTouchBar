namespace WindowsTouchBar.Device.Hid;

class TouchReport
{
    public double X { get; }
    public double XMax { get; }
    public double Y { get; }
    public double YMax { get; }
    public bool FingerStatus { get; }

    public TouchReport(double x, double xMax, double y, double yMax, bool status)
    {
        if (xMax <= 0)
        {
            throw new ArgumentException("xMax must be larger than zero");
        }

        X = x;
        XMax = xMax;
        Y = y;
        YMax = yMax;
        FingerStatus = status;
    }

    public double GetXInPercentage() => X / XMax;
    public double GetYInPercentage() => Y / YMax;
}
