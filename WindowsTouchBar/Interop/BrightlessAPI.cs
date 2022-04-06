using System.Management;

namespace WindowsTouchBar.Interop;

internal class BrightlessAPI
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public static int GetCurrentBrightness()
    {
        var scope = new ManagementScope("root\\WMI");
        var query = new SelectQuery("SELECT * FROM WmiMonitorBrightness");

        using ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
        using ManagementObjectCollection objectCollection = searcher.Get();
            
        foreach (ManagementObject mObj in objectCollection)
        {
            foreach (var item in mObj.Properties)
            {
                if (item?.Name == "CurrentBrightness" && item.Value != null)
                {
                    return int.TryParse(item.Value.ToString(), out var i) ? i : 0;
                }
            }
        }

        return 0;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public static void SetCurrentBrightness(int value)
    {
        var scope = new ManagementScope("root\\WMI");
        var query = new SelectQuery("SELECT * FROM WmiMonitorBrightnessMethods");

        using ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
        using ManagementObjectCollection objectCollection = searcher.Get();

        foreach (ManagementObject mObj in objectCollection)
        {
            mObj?.InvokeMethod("WmiSetBrightness", new object[] { 1, value });
        }
    }
}
