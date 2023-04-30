using ErogeHelper.AssistiveTouch.NativeMethods;
using Microsoft.Win32;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Windows;

namespace ErogeHelper.AssistiveTouch.Core;

internal class MagpieTouchHooker : IDisposable
{
    public const string MagTouchSystemPath = @"C:\Windows\ErogeHelper.MagpieTouch.exe";

    private const uint EVENT_SYSTEM_FOREGROUND = 0x0003;

    private readonly User32.HWINEVENTHOOK _windowsEventHook;

    private readonly GCHandle _gcSafetyHandle;

    public MagpieTouchHooker()
    {
        User32.WinEventProc winEventDelegate = WinEventCallback;
        _gcSafetyHandle = GCHandle.Alloc(winEventDelegate);

        _windowsEventHook = User32.SetWinEventHook(
             EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND,
             IntPtr.Zero, winEventDelegate, 0, 0,
             User32.WINEVENT.WINEVENT_OUTOFCONTEXT);

        // There is no parent child relationship between MagpieTouch, so named pipe is must.
        MagpieTouchPipe = new NamedPipeServerStream("f5fa0cb5-7d11-4569-a2f1-883ee52e92d8", PipeDirection.Out);
        _writer = new StreamWriter(MagpieTouchPipe);

        try
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = MagTouchSystemPath,
                Verb = "runas"
            });
            MagpieTouchPipe.WaitForConnection();
            _writer.AutoFlush = true;
        }
        catch (SystemException ex)
        {
            MessageBox.Show("Error with Launching ErogeHelper.MagpieTouch.exe\r\n" +
                "\r\n" +
                "Please check it installed properlly. ErogeHelper would continue run.\r\n" +
                "\r\n" +
                ex.Message,
                "ErogeHelper",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }
    }

    public void Close() => PipeSend(false, -1, -1, -1, -1, -1, -1, -1, -1);

    private readonly NamedPipeServerStream MagpieTouchPipe;

    private bool _inputTransformActivited;

    private readonly StreamWriter _writer;

    private void PipeSend(bool enable, int sx, int sy, int sw, int sh, int dx, int dy, int dw, int dh)
    {
        var payload = $"{enable} {sx} {sy} {sw} {sh} {dx} {dy} {dw} {dh}";
        _writer.WriteLineAsync(payload);
    }

    private void WinEventCallback(
        User32.HWINEVENTHOOK hWinEventHook,
        uint eventType,
        IntPtr hWnd,
        int idObject,
        int idChild,
        uint dwEventThread,
        uint dwmsEventTime)
    {
        if (eventType == EVENT_SYSTEM_FOREGROUND)
        {
            const string HOST_WINDOW_CLASS_NAME = "Window_Magpie_967EB565-6F73-4E94-AE53-00CC42592A22";
            var handle = User32.FindWindow(HOST_WINDOW_CLASS_NAME, null);
            bool hostExist = IntPtr.Zero != handle;

            if (hostExist && !_inputTransformActivited)
            {
                var source = SourceWindowRectangle();
                User32.GetWindowRect(handle, out var dest);

                var s1 = (double)source.Width / source.Height;
                var s2 = (double)dest.Width / dest.Height;
                var scale = s1 > s2 ? (double)dest.Width / source.Width : (double)dest.Height / source.Height;

                var widthAfterScaled = source.Width * scale;
                var heightAfterScaled = source.Height * scale;
                var destLeft = (dest.Width - widthAfterScaled) / 2;
                var destTop = (dest.Height- heightAfterScaled) / 2;
                PipeSend(true, source.Left, source.Top, source.Width, source.Height, (int)destLeft, (int)destTop, (int)widthAfterScaled, (int)heightAfterScaled);
                _inputTransformActivited = true;
                SetTouchFeedback(false);
            }
            else if (!hostExist && _inputTransformActivited)
            {
                PipeSend(false, 0, 0, 0, 0, 0, 0, 0, 0);
                _inputTransformActivited = false;
                SetTouchFeedback(true);
            }
        }
    }

    private const string TouchFeedback = "Control Panel\\Cursors";
    public void SetTouchFeedback(bool show)
    {
        using var key = Registry.CurrentUser.OpenSubKey(TouchFeedback, true);
        const string ContactVisualization = "ContactVisualization";
        key?.SetValue(ContactVisualization, show ? 1 : 0);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetWindowRect(IntPtr hwnd, out Rectangle lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetClientRect(IntPtr hwnd, out Rectangle lpRect);

    private Rectangle SourceWindowRectangle()
    {
        GetWindowRect(App.GameWindowHandle, out var rect);
        GetClientRect(App.GameWindowHandle, out var rectClient);
        // rect.Right - rect.Left == rect.Width == (0, 0) to client right-bottom point 
        var rectWidth = rect.Width - rect.Left;
        var rectHeight = rect.Height - rect.Top;

        var winShadow = (rectWidth - rectClient.Width) / 2;
        var left = rect.Left + winShadow;

        var winTitleHeight = rectHeight - rectClient.Height - winShadow;
        var top = rect.Top + winTitleHeight;

        var win = (MainWindow)Application.Current.MainWindow;
        return new Rectangle(
            left, top, (int)(rectClient.Width + left), (int)(rectClient.Height + top ));
    }

    public void Dispose()
    {
        _gcSafetyHandle.Free();
        User32.UnhookWinEvent(_windowsEventHook);
    }
}
