using WindowsTouchBar.Device.Hid;
using WindowsTouchBar.Events;
using HidSharp;
using HidSharp.Reports;
using HidSharp.Reports.Input;

namespace WindowsTouchBar;

internal class TouchReceiver
{
    private HidDevice _digitizer;
    private ReportDescriptor _reportDescr;
    private HidStream _hidStream;
    private HidDeviceInputReceiver _hidDeviceInputReceiver;
    private DeviceItemInputParser _hiddeviceInputParser;
    private readonly int _width;
    private readonly int _height;
    private TouchReport[] _prevReports;
    private int _prevTappedSlotIndex;

    public event Action<TouchEventArgs> Event;

    public TouchReceiver(int width, int height)
    {
        _width = width;
        _height = height;
        _digitizer = DeviceList.Local.GetHidDeviceOrNull(0x05ac, 0x8302);
        if (_digitizer == null)
        {
            throw new Exception("iBridge HID digitizer not found");
        }

        _reportDescr = _digitizer.GetReportDescriptor();

        if (_digitizer.TryOpen(out _hidStream))
        {
            _hidDeviceInputReceiver = _reportDescr.CreateHidDeviceInputReceiver();
            _hiddeviceInputParser = _reportDescr.DeviceItems[0].CreateDeviceItemInputParser();
            _hidDeviceInputReceiver.Received += OnDigitizerInputReceived;
            _hidDeviceInputReceiver.Start(_hidStream);
        }
        else
        {
            throw new Exception("Failed to open iBridge HID digitizer");
        }
    }

    private void OnDigitizerInputReceived(object? sender, EventArgs e)
    {
        var inputReportBuffer = new byte[_digitizer.GetMaxInputReportLength()];
        while (_hidDeviceInputReceiver.TryRead(inputReportBuffer, 0, out Report report))
        {
            // Parse the report if possible.
            // This will return false if (for example) the report applies to a different DeviceItem.
            if (_hiddeviceInputParser.TryParseReport(inputReportBuffer, 0, report))
            {
                ProcessEvent();
            }
        }
    }

    private void ProcessEvent()
    {
        if (_hiddeviceInputParser.HasChanged)
        {
            int j = -1;
            var currentReports = new TouchReport[11];

            for (int i = 0; i < _hiddeviceInputParser.ValueCount; i++)
            {
                var data = _hiddeviceInputParser.GetValue(i);
                if (data.Usages.FirstOrDefault() == VendorUsage.FingerIdentifier)
                {
                    j++;
                }
                else
                {
                    continue;
                }

                // Only 11 slots are statically allocated
                if (j >= 11) break;

                // This is defined by the descriptor, we just take the assumption
                var fingerTapData1 = _hiddeviceInputParser.GetValue(i + 1);
                var fingerTapData2 = _hiddeviceInputParser.GetValue(i + 2);
                var xData = _hiddeviceInputParser.GetValue(i + 3);
                var yData = _hiddeviceInputParser.GetValue(i + 4);

                // Register this
                currentReports[j] = new TouchReport(
                    xData.GetPhysicalValue(),
                    xData.DataItem.PhysicalMaximum,
                    yData.GetPhysicalValue(),
                    yData.DataItem.PhysicalMaximum,
                    fingerTapData1.GetPhysicalValue() != 0);
            }

            // Check if need to raise touch leave event for prev slot
            if (_prevTappedSlotIndex >= 0)
            {
                var scaledX = (int)(currentReports[_prevTappedSlotIndex].GetXInPercentage() * _width);
                var scaledY = (int)(currentReports[_prevTappedSlotIndex].GetYInPercentage() * _height);

                if (!currentReports[_prevTappedSlotIndex].FingerStatus)
                {
                    Event?.Invoke(new TouchEventArgs(
                        scaledX, 
                        scaledY, 
                        TouchEventType.TouchUp));

                    _prevTappedSlotIndex = -1;
                }
                else
                {
                    Event?.Invoke(new TouchEventArgs(
                        scaledX,
                        scaledY,
                        TouchEventType.TouchMove));
                }
            }

            // Can raise new tap event
            if (_prevTappedSlotIndex == -1)
            {
                for (int i = 0; i < 11; i++)
                {
                    if (currentReports[i].FingerStatus)
                    {
                        var scaledX = (int)(currentReports[i].GetXInPercentage() * _width);
                        var scaledY = (int)(currentReports[i].GetYInPercentage() * _height);

                        Event?.Invoke(new TouchEventArgs(
                        scaledX,
                        scaledY,
                        TouchEventType.TouchDown));
                        
                        _prevTappedSlotIndex = i;
                        break;
                    }
                }
            }

            // Update cache
            _prevReports = currentReports;
        }
    }
}
