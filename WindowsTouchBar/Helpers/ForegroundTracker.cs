using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using WindowsTouchBar.Models;

namespace WindowsTouchBar.Helpers;

public static class ForegroundTracker
{
    delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    [DllImport("user32.dll")]
    static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr
       hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess,
       uint idThread, uint dwFlags);

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    static extern int GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", EntryPoint = "GetWindowTextA")]
    static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, uint nMaxCount);

    [DllImport("user32.dll", EntryPoint = "GetWindowTextLengthA")]
    static extern uint GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", EntryPoint = "FindWindowExA")]
    static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string? lpszWindow);
   
    [DllImport("user32.dll")]
    static extern bool UnhookWinEvent(IntPtr hWinEventHook);

    [DllImport("user32.dll")]
    static extern bool EnumPropsW(IntPtr hwndSubclass, CallbackFunc lpEnumFunc);

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr SendMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

    // Constants from winuser.h
    const uint EVENT_SYSTEM_FOREGROUND = 3;
    const uint WINEVENT_OUTOFCONTEXT = 0;
    const uint BM_CLICK = 0x00F5;
    const uint WM_LBUTTONDOWN = 0x0201;
    const uint WM_LBUTTONUP = 0x0202;

    public delegate bool CallbackFunc(IntPtr hwndSubclass,  // handle of window with property 
        string lpszString,  // property string or atom 
        object hData);

    internal static event Action<ForegroundProcess>? OnProcessChanged;

    internal static void Init()
    {
        Task.Run(() =>
        {
            IntPtr? _currHwnd = null;

            while (true)
            {
                var hwnd = GetForegroundWindow();

                if (_currHwnd != hwnd)
                {
                    _currHwnd = hwnd;

                    uint pid;
                    GetWindowThreadProcessId(hwnd, out pid);

                    if (OnProcessChanged != null)
                    {
                        var process = Process.GetProcessById((int)pid);

                        OnProcessChanged(new ForegroundProcess
                        {
                            Process = process,
                            Handle = hwnd
                        });
                    }
                }

                Thread.Sleep(200);
            }
        });
    }

    internal static IEnumerable<ForegroundButton> GetWindowButtons(IntPtr hWnd)
    {
        var txt = new StringBuilder();

        IntPtr handle = FindWindowEx(hWnd, IntPtr.Zero, "BUTTON", null);
        while (handle != IntPtr.Zero)
        {
            GetWindowText(handle, txt, 255);

            yield return new ForegroundButton
            {
                Title = txt.ToString(),
                Handle = handle
            };

            handle = FindWindowEx(hWnd, handle, "BUTTON", null);
        }
    }

    internal static void PressForegroundButton(IntPtr handle)
    {
        SendMessage(handle, BM_CLICK, 0, 0);
    }
}
