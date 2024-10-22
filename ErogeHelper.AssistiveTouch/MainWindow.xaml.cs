﻿using ErogeHelper.AssistiveTouch.Core;
using ErogeHelper.AssistiveTouch.Helper;
using ErogeHelper.AssistiveTouch.NativeMethods;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace ErogeHelper.AssistiveTouch;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    // Thread RootHwndWatch, Stylus Input are from Wpf 
    public static IntPtr Handle { get; private set; }

    public double Dpi { get; }

    public MainWindow()
    {
        InitializeComponent();
        Handle = new WindowInteropHelper(this).EnsureHandle();
        Dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;

        Loaded += (_, _) => ForceSetForegroundWindow(App.GameWindowHandle);
        ContentRendered += (_, _) => IpcRenderer.Send("Loaded");

        HwndTools.RemovePopupAddChildStyle(Handle);
        User32.SetParent(Handle, App.GameWindowHandle);
        User32.GetClientRect(App.GameWindowHandle, out var rectClient);
        User32.SetWindowPos(Handle, IntPtr.Zero, 0, 0, rectClient.Width, rectClient.Height, User32.SetWindowPosFlags.SWP_NOZORDER);

        var hooker = new GameWindowHooker(Handle);
        hooker.SizeChanged += (_, _) => Fullscreen.UpdateFullscreenStatus();
        hooker.FocusLost += (_, _) => { if (Menu.IsOpened) Menu.ManualClose(); };

        if (Config.UseEdgeTouchMask)
        {
            Fullscreen.MaskForScreen(this);
        }
    }

    private static void ForceSetForegroundWindow(IntPtr hWnd)
    {
        // Must use foreground window thread, whatever who is it.(expect task manager, almost explorer predictable:)
        uint foreThread = User32.GetWindowThreadProcessId(User32.GetForegroundWindow(), out _);
        uint appThread = Kernel32.GetCurrentThreadId();

        User32.AttachThreadInput(foreThread, appThread, true);
        User32.BringWindowToTop(hWnd);
        User32.AttachThreadInput(foreThread, appThread, false);
    }
}
