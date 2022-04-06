using System.ComponentModel;
using System.Runtime.InteropServices;
using WindowsTouchBar.Interop;

namespace WindowsTouchBar.Device;

public class Locator
{
    static Guid DfrDisplayInterfaceGuid = Guid.Parse("2003cacd-9e7c-477c-ab06-a5a8bbb1a63e");

    public static string? FindDfrDevice()
    {
        string? instancePath = null;
        var bResult = true;
        uint i = 0;

        var h = SetupAPI.SetupDiGetClassDevs(
            ref DfrDisplayInterfaceGuid,
            IntPtr.Zero,
            IntPtr.Zero,
            SetupAPI.DIGCF_PRESENT | SetupAPI.DIGCF_DEVICEINTERFACE
        );

        if (h != IntPtr.Zero)
        {
            // https://www.pinvoke.net/default.aspx/setupapi.setupdienumdeviceinterfaces
            while (bResult)
            {
                SP_DEVICE_INTERFACE_DATA dia = new SP_DEVICE_INTERFACE_DATA();
                dia.cbSize = (uint) Marshal.SizeOf(dia);

                bResult = SetupAPI.SetupDiEnumDeviceInterfaces(h, IntPtr.Zero,
                    ref DfrDisplayInterfaceGuid, i, dia);
                if (bResult)
                {
                    SP_DEVINFO_DATA da = new SP_DEVINFO_DATA();
                    da.cbSize = (uint) Marshal.SizeOf(da);

                    if (!SetupAPI.SetupDiGetDeviceInterfaceDetail(h, dia, IntPtr.Zero, 0, out uint nRequiredSize, null))
                    {
                        // ERROR_INSUFFICIENT_BUFFER 
                        if (Marshal.GetLastWin32Error() != 122)
                        {
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                        }

                        var deviceInterfaceDetailData = Marshal.AllocHGlobal((int) nRequiredSize);

                        try
                        {
                            Marshal.WriteInt32(deviceInterfaceDetailData, 
                                (IntPtr.Size == 4) ? (4 + Marshal.SystemDefaultCharSize) : 8);
                            if (!SetupAPI.SetupDiGetDeviceInterfaceDetail(h, dia,
                                deviceInterfaceDetailData, nRequiredSize, out uint _, null))
                            {
                                throw new Win32Exception(Marshal.GetLastWin32Error());
                            }

                            IntPtr pDevicePathName = new IntPtr(deviceInterfaceDetailData.ToInt64() + 4);
                            return Marshal.PtrToStringAuto(pDevicePathName);
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(deviceInterfaceDetailData);
                        }
                    }
                }

                i++;
            }
        }

        return instancePath;
    }
}
