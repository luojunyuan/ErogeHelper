using ErogeHelper.AssistiveTouch.Core;
using ErogeHelper.AssistiveTouch.NativeMethods;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace ErogeHelper.AssistiveTouch;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IntPtr GameWindowHandle { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        var _pipeClient = new AnonymousPipeClientStream(PipeDirection.Out, e.Args[0]);
        _ = new IpcRenderer(_pipeClient);

        GameWindowHandle = (IntPtr)int.Parse(e.Args[1]);

        DisableWPFTabletSupport();

        Config.Load();

        if (Config.UseEnterKeyMapping)
        {
            GlobalKeyHook();
        }

        if (Config.EnableMagTouchMapping)
        {
            StartMagTouch();
        }
    }

    private static void StartMagTouch()
    {
        const string MagTouchSystemPath = @"C:\Windows\ErogeHelper.MagTouch.exe";

        if (!File.Exists(MagTouchSystemPath))
        {
            MessageBox.Show("Please install MagTouch first.", "ErogeHelper");
            return;
        }

        try
        {
            // Send current pid and App.GameWindowHandle
            Process.Start(new ProcessStartInfo()
            {
                FileName = MagTouchSystemPath,
                Arguments = Process.GetCurrentProcess().Id + " " + GameWindowHandle.ToString(),
                Verb = "runas",
            });
        }
        catch (SystemException ex)
        {
            MessageBox.Show("Error with Launching ErogeHelper.MagTouch.exe\r\n" +
                "\r\n" +
                "Please check it installed properly. ErogeHelper would continue run.\r\n" +
                "\r\n" +
                ex.Message,
                "ErogeHelper",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }
    }

    private static void GlobalKeyHook()
    {
        KeyboardHooker.Install(GameWindowHandle);
        Current.Exit += (_, _) => KeyboardHooker.UnInstall();
    }

    private static void DisableWPFTabletSupport()
    {
        // Get a collection of the tablet devices for this window.
        TabletDeviceCollection devices = Tablet.TabletDevices;

        if (devices.Count > 0)
        {
            // Get the Type of InputManager.  
            Type inputManagerType = typeof(InputManager);

            // Call the StylusLogic method on the InputManager.Current instance.  
            object stylusLogic = inputManagerType.InvokeMember("StylusLogic",
                        BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                        null, InputManager.Current, null)!;

            if (stylusLogic != null)
            {
                //  Get the type of the stylusLogic returned from the call to StylusLogic.  
                Type stylusLogicType = stylusLogic.GetType();

                // Loop until there are no more devices to remove.  
                while (devices.Count > 0)
                {
                    // Remove the first tablet device in the devices collection.  
                    stylusLogicType.InvokeMember("OnTabletRemoved",
                            BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic,
                            null, stylusLogic, new object[] { (uint)0 });
                }
            }

        }
    }
}
