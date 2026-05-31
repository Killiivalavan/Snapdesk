using System;
using System.Runtime.Versioning;
using System.Threading;
using Vanara.PInvoke;

namespace SnapDesk.Platform.Windows;

[SupportedOSPlatform("windows")]
public class WindowsHotkeyMessageLoop : IDisposable
{
    private const uint WM_HOTKEY = 0x0312;
    private const uint WM_QUIT = 0x0012;

    private readonly Action<int> _onHotkeyPressed;
    private Thread? _messageThread;
    private HWND _hiddenWindow;
    private bool _running;
    private bool _disposed;

    public WindowsHotkeyMessageLoop(Action<int> onHotkeyPressed)
    {
        _onHotkeyPressed = onHotkeyPressed ?? throw new ArgumentNullException(nameof(onHotkeyPressed));
    }

    public IntPtr WindowHandle => (IntPtr)_hiddenWindow;

    public void Start()
    {
        if (_running) return;
        _running = true;

        _messageThread = new Thread(MessageLoopProc)
        {
            Name = "SnapDesk Hotkey Message Loop",
            IsBackground = true
        };
        _messageThread.SetApartmentState(ApartmentState.STA);
        _messageThread.Start();
    }

    private void MessageLoopProc()
    {
        _hiddenWindow = User32.CreateWindowEx(
            0,
            "STATIC",
            "SnapDeskHotkeyWindow",
            0,
            0, 0, 0, 0,
            HWND.NULL,
            HMENU.NULL,
            HINSTANCE.NULL,
            IntPtr.Zero);

        if (_hiddenWindow.IsNull)
        {
            _running = false;
            return;
        }

        HotkeyApi.SetHotkeyWindowHandle((IntPtr)_hiddenWindow);

        while (_running)
               {
                   int result = User32.GetMessage(out var msg, _hiddenWindow, 0, 0);
                   if (result == 0)
                       break;
                   if (result == -1)
                       continue;
                   if (msg.message == WM_HOTKEY)
                   {
                       int hotkeyId = (int)(uint)msg.wParam;
                       _onHotkeyPressed(hotkeyId);
                   }
                   else
                   {
                       User32.TranslateMessage(msg);
                       User32.DispatchMessage(msg);
                   }
               }

        if (!_hiddenWindow.IsNull)
        {
            User32.DestroyWindow(_hiddenWindow);
            _hiddenWindow = HWND.NULL;
        }
    }

    public void Stop()
    {
        _running = false;
        if (!_hiddenWindow.IsNull)
        {
            User32.PostMessage(_hiddenWindow, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Stop();
            _disposed = true;
        }
    }
}
