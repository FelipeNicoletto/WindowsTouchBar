using SkiaSharp;
using System.Text.Json;
using System.Text.Json.Serialization;
using WindowsTouchBar.Device;
using WindowsTouchBar.Helpers;
using WindowsTouchBar.Interop;
using WindowsTouchBar.Models;
using WindowsTouchBar.Views;

namespace WindowsTouchBar;

internal class TouchBar : IDisposable
{
    private const string SETTINGS_FILE = "settings.json";

    private readonly IntPtr deviceHandle;
    private readonly LockedFrameBuffer FrameBuffer;
    private readonly TouchReceiver TouchReceiver;
    private readonly FnKeyNotifier FnKeyNotifier;

    private TouchBarView? currentView;
    private CustomFunctionsView? customFunctionsView;
    private DefaultFunctionsView? defaultFunctionsView;
    private MediaFunctionsView? mediaFunctionsView;
    private ExplorerDialogView? explorerDialogView;
    private SKSurface? skSurface;
    private SKCanvas? canvas;
    private Settings? settings;
    private FileSystemWatcher? settingsFileListener;
    private Settings.View? currentSettingView;
    private ForegroundProcess? currentProcess;
    private bool fnPressed;
    private Events.TouchEventArgs? lastTouchEvent;

    private const int Width = 2008;
    private const int Height = 60;

    public TouchBar()
    {
        var instancePath = Locator.FindDfrDevice();
        if (instancePath == null)
        {
            throw new Exception("DFR Display Device not found");
        }

        deviceHandle = NativeMethods.CreateFile(
            instancePath, FileAccess.Write, FileShare.None,
            IntPtr.Zero, FileMode.Open, FileOptions.None,
            IntPtr.Zero
        );

        FrameBuffer = new LockedFrameBuffer(deviceHandle, Width, Height);
        TouchReceiver = new TouchReceiver(Width, Height);
        TouchReceiver.Event += TouchReceiver_Event;
        FnKeyNotifier = new FnKeyNotifier(instancePath);
        FnKeyNotifier.Event += FnKeyNotifier_Event;

        ForegroundTracker.OnProcessChanged += ForegroundTracker_OnProcessChanged;

        CreateCanvas();
        CreateViews();
    }

    public void Init()
    {
        InitSettings();

        ForegroundTracker.Init();

        currentView?.Invalidate();

        while (true) { };
    }

    private void InitSettings()
    {
        settingsFileListener = new FileSystemWatcher(".", SETTINGS_FILE);
        settingsFileListener.Changed += SettingsFileListener_Changed;
        settingsFileListener.EnableRaisingEvents = true;

        LoadSettings();
    }

    private void LoadSettings()
    {
        if (File.Exists(SETTINGS_FILE))
        {
            using var shortcutsFile = File.OpenRead(SETTINGS_FILE);

            try
            {
                settings = JsonSerializer.Deserialize<Settings>(shortcutsFile, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters =
                {
                    new JsonStringEnumConverter()
                }
                });
            }
            catch { }

            if (!string.IsNullOrWhiteSpace(settings?.BackgroundColor) && SKColor.TryParse(settings.BackgroundColor, out var backgroundColor))
            {
                defaultFunctionsView!.BackgroundColor = backgroundColor;
                customFunctionsView!.BackgroundColor = backgroundColor;
                mediaFunctionsView!.BackgroundColor = backgroundColor;
                explorerDialogView!.BackgroundColor = backgroundColor;
            }

            defaultFunctionsView!.ConfigureView(settings?.Views?.FirstOrDefault(v => v.Type == ViewType.Function));
            mediaFunctionsView!.ConfigureView(settings?.Views?.FirstOrDefault(v => v.Type == ViewType.Media));
        }
        else
        {
            settings = null;
        }

        RefreshCurrentProcess(currentProcess);
    }

    private void SettingsFileListener_Changed(object sender, FileSystemEventArgs e)
    {
        LoadSettings();
    }

    private void CreateCanvas()
    {
        skSurface = SKSurface.Create(new SKImageInfo(Width, Height, SKColorType.Rgba8888, SKAlphaType.Opaque), FrameBuffer.Address);
        canvas = skSurface.Canvas;
    }

    private void CreateViews()
    {
        defaultFunctionsView = new DefaultFunctionsView(Width, Height);
        defaultFunctionsView.OnInvalidate += CurrentView_OnInvalidate;
        customFunctionsView = new CustomFunctionsView(Width, Height);
        customFunctionsView.OnInvalidate += CurrentView_OnInvalidate;
        mediaFunctionsView = new MediaFunctionsView(Width, Height);
        mediaFunctionsView.OnInvalidate += CurrentView_OnInvalidate;
        explorerDialogView = new ExplorerDialogView(Width, Height);
        explorerDialogView.OnInvalidate += CurrentView_OnInvalidate;
    }

    private void ForegroundTracker_OnProcessChanged(ForegroundProcess process)
    {
        RefreshCurrentProcess(process);
    }

    private void RefreshCurrentProcess(ForegroundProcess? process)
    {
        currentProcess = process;

        RefreshView();
    }

    private void RefreshView()
    {
        string? name = null;

        try
        {
            name = currentProcess?.Process?.MainModule?.ModuleName;
        }
        catch { }

        Settings.View? view = null;

        if (!fnPressed && !string.IsNullOrWhiteSpace(name))
        {
            view = settings?.Views?.FirstOrDefault(v => v.Type == ViewType.Custom && string.Equals(v.Application, name, StringComparison.OrdinalIgnoreCase));
        }

        if (!fnPressed &&
            string.Equals("explorer.exe", name, StringComparison.OrdinalIgnoreCase) &&
            currentProcess is not null &&
            currentProcess.Handle != IntPtr.Zero)
        {
            var buttons = ForegroundTracker.GetWindowButtons(currentProcess.Handle).Where(b => b.Title?.StartsWith('&') is true || b.Title?.Contains('&') is not true);

            if (buttons.Any())
            {
                explorerDialogView!.CreateDialog(buttons);

                DisposeViewLastTouch();
                currentView = explorerDialogView;

                currentView?.Invalidate();

                return;
            }
        }

        if (view != null)
        {
            if (view != currentSettingView)
            {
                DisposeViewLastTouch();
                currentSettingView = view;
                customFunctionsView?.ConfigureView(currentSettingView);
                currentView = customFunctionsView;
                currentView?.Invalidate();
            }
        }
        else
        {
            currentSettingView = null;

            DisposeViewLastTouch();
            if (settings?.Views?.Any(v => v.Type == ViewType.Media && v.Default) is true)
            {
                currentView = fnPressed ? defaultFunctionsView : mediaFunctionsView;
            }
            else
            {
                currentView = fnPressed ? mediaFunctionsView : defaultFunctionsView;
            }

            currentView?.Invalidate();
        }
    }

    public void CurrentView_OnInvalidate(object? sender, EventArgs e)
    {
        currentView?.Clear(canvas);
        currentView?.Draw(canvas);
        FrameBuffer.VSync();
    }

    private void TouchReceiver_Event(Events.TouchEventArgs evt)
    {
        lastTouchEvent = evt;
        currentView?.ReceiveTouch(evt);
    }

    private void DisposeViewLastTouch()
    {
        if (lastTouchEvent is not null &&
            currentView is not null &&
            lastTouchEvent.TouchEventType != Events.TouchEventType.TouchUp)
        {
            var evt = new Events.TouchEventArgs(lastTouchEvent.X, lastTouchEvent.Y, Events.TouchEventType.TouchUp);
            lastTouchEvent = null;

            currentView.ReceiveTouch(evt);
        }
    }

    private void FnKeyNotifier_Event(Events.FnKeyEventArgs evt)
    {
        fnPressed = evt.Pressed;

        RefreshView();
    }

    private bool disposedValue = false;
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                
            }
            
            NativeMethods.CloseHandle(deviceHandle);
            TouchReceiver.Event -= TouchReceiver_Event;

            skSurface?.Dispose();
            canvas?.Dispose();
            FrameBuffer.Dispose();
            settingsFileListener?.Dispose();

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
