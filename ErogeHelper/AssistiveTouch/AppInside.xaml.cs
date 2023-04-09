using System.Windows;
using ErogeHelper.AssistiveTouch.Helper;
using ErogeHelper.AssistiveTouch.NativeMethods;
using WindowsInput.Events;

namespace ErogeHelper.AssistiveTouch;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class AppInside : Application
{
    public static IntPtr GameWindowHandle { get; private set; }

    public AppInside(IntPtr handle)
    {
        InitializeComponent();

        GameWindowHandle = handle;
        Config.Load();

        if (Config.EnterKeyMapping)
        {
            GlobalKeyHook();
        }
    }

    private static void GlobalKeyHook()
    {
        const int debounce = 40;
        var throttle = new Throttle(debounce, () =>
            WindowsInput.Simulate.Events()
                .Click(KeyCode.Enter)
                .Invoke());
        // Thread name MessagePumpingObject
        var keyboard = WindowsInput.Capture.Global.KeyboardAsync();
        keyboard.KeyDown += (_, e) =>
        {
            if (e.Data.Key == KeyCode.Z && User32.GetForegroundWindow() == GameWindowHandle)
            {
                e.Next_Hook_Enabled = false;
                throttle.Signal();
            }
        };
        Current.Exit += (_, _) => keyboard.Dispose();
    }
}
